#nullable disable
// The adapter is compiled by the SDK; the value protocol is shared with the worker.
// Only this source adapter enters the persistent build node. The real task DLL
// and its dependencies are loaded by a single-use dotnet process.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using AtomUI.Build.Tasks.Isolation;
using Microsoft.Build.Framework;
#if !ATOMUI_BUILD_TASK_HOST
using Microsoft.Build.Utilities;
#endif

namespace AtomUI.Build.Tasks.Isolation
{
    // The protocol contains values only. No task-defined types or MSBuild runtime
    // objects cross the process boundary, including item implementations.
    [DataContract]
    public sealed class TaskRequest
    {
        [DataMember] public string TaskName { get; set; }
        [DataMember] public string ProjectFile;
        [DataMember] public int Line;
        [DataMember] public int Column;
        [DataMember] public Dictionary<string, string> Properties = new Dictionary<string, string>();
        [DataMember] public Dictionary<string, TaskWireItem[]> Items = new Dictionary<string, TaskWireItem[]>();
    }

    [DataContract]
    public sealed class TaskResponse
    {
        [DataMember] public bool Success;
        [DataMember] public Dictionary<string, string> Properties = new Dictionary<string, string>();
        [DataMember] public Dictionary<string, TaskWireItem[]> Items = new Dictionary<string, TaskWireItem[]>();
        [DataMember] public List<TaskDiagnostic> Diagnostics = new List<TaskDiagnostic>();
    }

    [DataContract]
    public sealed class TaskWireItem : ITaskItem
    {
        [DataMember] public string ItemSpec { get; set; }
        [DataMember] public string FullPath;
        [DataMember] public Dictionary<string, string> Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public int MetadataCount { get { return Metadata.Count; } }
        public ICollection MetadataNames { get { return Metadata.Keys.ToArray(); } }
        public string GetMetadata(string name)
        {
            string value;
            if (Metadata.TryGetValue(name, out value))
            {
                return value;
            }

            return name.Equals("FullPath", StringComparison.OrdinalIgnoreCase) ? FullPath ?? string.Empty : string.Empty;
        }
        public void SetMetadata(string name, string value) { Metadata[name] = value; }
        public void RemoveMetadata(string name) { Metadata.Remove(name); }
        public IDictionary CloneCustomMetadata() { return new Dictionary<string, string>(Metadata, StringComparer.OrdinalIgnoreCase); }
        public void CopyMetadataTo(ITaskItem destination)
        {
            foreach (var pair in Metadata)
            {
                if (string.IsNullOrEmpty(destination.GetMetadata(pair.Key)))
                {
                    var literalDestination = destination as ITaskItem2;
                    if (literalDestination != null)
                    {
                        literalDestination.SetMetadataValueLiteral(pair.Key, pair.Value);
                    }
                    else
                    {
                        destination.SetMetadata(pair.Key, pair.Value);
                    }
                }
            }
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            Metadata = new Dictionary<string, string>(Metadata, StringComparer.OrdinalIgnoreCase);
        }

        public static TaskWireItem FromItem(ITaskItem item)
        {
            var result = new TaskWireItem { ItemSpec = item.ItemSpec, FullPath = item.GetMetadata("FullPath") };
            foreach (DictionaryEntry entry in item.CloneCustomMetadata())
            {
                result.Metadata.Add((string)entry.Key, (string)entry.Value);
            }

            return result;
        }
    }

    [DataContract]
    public sealed class TaskDiagnostic
    {
        [DataMember] public string Kind;
        [DataMember] public string Message;
        [DataMember] public string Subcategory;
        [DataMember] public string Code;
        [DataMember] public string HelpKeyword;
        [DataMember] public string File;
        [DataMember] public int Line;
        [DataMember] public int Column;
        [DataMember] public int EndLine;
        [DataMember] public int EndColumn;
        [DataMember] public int Importance;
    }

    public static class TaskWire
    {
        public static void Write<T>(string path, T value)
        {
            using (var stream = File.Create(path))
            {
                Serializer<T>().WriteObject(stream, value);
            }
        }

        public static T Read<T>(string json)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                return (T)Serializer<T>().ReadObject(stream);
            }
        }

        public static void WriteResponse(TaskResponse response)
        {
            using (var stream = Console.OpenStandardOutput())
            {
                Serializer<TaskResponse>().WriteObject(stream, response);
            }
        }

        private static DataContractJsonSerializer Serializer<T>()
        {
            return new DataContractJsonSerializer(typeof(T),
                new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true });
        }
    }
}


#if !ATOMUI_BUILD_TASK_HOST
namespace AtomUI.Build.Tasks
{
    public abstract class IsolatedBuildTask : Task, ICancelableTask
    {
        [Required] public string TaskAssembly { get; set; }
        public string DotNetPath { get; set; }

        private readonly object processGate = new object();
        private Process process;
        private bool cancelled;

        public void Cancel()
        {
            lock (processGate)
            {
                cancelled = true;
                if (process != null && !process.HasExited)
                    process.Kill();
            }
        }

        public override bool Execute()
        {
            string directory = Path.Combine(Path.GetTempPath(), "atomui-build-task-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                return ExecuteProcess(Path.Combine(directory, "request.json"));
            }
            catch (Exception error)
            {
                Log.LogErrorFromException(error, true);
                return false;
            }
            finally
            {
                lock (processGate)
                {
                    if (process != null)
                    {
                        // A failed read or cancellation must not let a worker outlive
                        // the invocation and keep the toolset locked in the background.
                        if (!process.HasExited)
                        {
                            process.Kill();
                            process.WaitForExit();
                        }
                        process.Dispose();
                        process = null;
                    }
                }
                Directory.Delete(directory, true);
            }
        }

        private bool ExecuteProcess(string projectFile)
        {
            var parameters = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.DeclaringType == GetType()).ToArray();
            var outputs = parameters.Where(p => p.IsDefined(typeof(OutputAttribute), true)).ToArray();
            var request = new TaskRequest
            {
                TaskName = GetType().Name, ProjectFile = BuildEngine.ProjectFileOfTaskNode,
                Line = BuildEngine.LineNumberOfTaskNode, Column = BuildEngine.ColumnNumberOfTaskNode
            };
            foreach (var parameter in parameters.Except(outputs))
            {
                object value = parameter.GetValue(this, null);
                if (value == null)
                    continue;
                if (parameter.PropertyType == typeof(ITaskItem[]))
                    request.Items.Add(parameter.Name, ((ITaskItem[])value).Select(TaskWireItem.FromItem).ToArray());
                else
                    request.Properties.Add(parameter.Name, Convert.ToString(value, CultureInfo.InvariantCulture));
            }
            TaskWire.Write(projectFile, request);
            string arguments = Quote(Path.GetFullPath(TaskAssembly)) + " " + Quote(projectFile);
            var start = new ProcessStartInfo
            {
                FileName = string.IsNullOrWhiteSpace(DotNetPath) ? "dotnet" : DotNetPath,
                Arguments = arguments,
                WorkingDirectory = Directory.GetCurrentDirectory(),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };
            lock (processGate)
            {
                if (cancelled)
                    return false;
                process = Process.Start(start);
            }
            var standardOutput = process.StandardOutput.ReadToEndAsync();
            var standardError = process.StandardError.ReadToEndAsync();
            process.WaitForExit();
            string outputText = standardOutput.GetAwaiter().GetResult();
            string errorText = standardError.GetAwaiter().GetResult();
            foreach (string line in errorText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                Log.LogMessageFromText(line, MessageImportance.High);
            if (process.ExitCode != 0)
            {
                Log.LogError("AtomUI build task {0} exited with code {1}. {2}", GetType().Name, process.ExitCode, outputText);
                return false;
            }

            TaskResponse result = TaskWire.Read<TaskResponse>(outputText);
            foreach (TaskDiagnostic diagnostic in result.Diagnostics)
            {
                if (diagnostic.Kind == "error")
                    Log.LogError(diagnostic.Subcategory, diagnostic.Code, diagnostic.HelpKeyword, diagnostic.File,
                        diagnostic.Line, diagnostic.Column, diagnostic.EndLine, diagnostic.EndColumn, diagnostic.Message);
                else if (diagnostic.Kind == "warning")
                    Log.LogWarning(diagnostic.Subcategory, diagnostic.Code, diagnostic.HelpKeyword, diagnostic.File,
                        diagnostic.Line, diagnostic.Column, diagnostic.EndLine, diagnostic.EndColumn, diagnostic.Message);
                else
                    Log.LogMessage((MessageImportance)diagnostic.Importance, "{0}", diagnostic.Message);
            }
            if (!result.Success)
                return false;
            foreach (var output in outputs)
            {
                if (output.PropertyType == typeof(ITaskItem[]))
                {
                    // Keep the original task's literal ITaskItem semantics, including
                    // custom metadata named Identity. TaskItem would reinterpret both.
                    output.SetValue(this, result.Items[output.Name].Cast<ITaskItem>().ToArray(), null);
                }
                else
                    output.SetValue(this, Convert.ChangeType(result.Properties[output.Name], output.PropertyType,
                        CultureInfo.InvariantCulture), null);
            }
            return !Log.HasLoggedErrors;
        }

        private static string Quote(string path)
        {
            if (path.IndexOf('"') >= 0)
                throw new ArgumentException("A process path cannot contain a quote.", "path");
            return "\"" + path + "\"";
        }

    }
}

namespace AtomUI.Build.Tasks
{
    public sealed class ConditionalPublishTransformTask : IsolatedBuildTask
    {
        public string Operation { get; set; } = string.Empty;
        public string InputsPath { get; set; } = string.Empty;
        public string VerifiedReceiptPath { get; set; } = string.Empty;
        public string StatePath { get; set; } = string.Empty;
        public string PublishDirectory { get; set; } = string.Empty;
        public string BundlePath { get; set; } = string.Empty;
        public string Options { get; set; } = string.Empty;
        public ITaskItem[] Files { get; set; } = Array.Empty<ITaskItem>();
        public ITaskItem[] References { get; set; } = Array.Empty<ITaskItem>();
        public ITaskItem[] Tools { get; set; } = Array.Empty<ITaskItem>();
        public ITaskItem[] ExtraOutputs { get; set; } = Array.Empty<ITaskItem>();
        [Output] public ITaskItem[] PreparedTools { get; set; } = Array.Empty<ITaskItem>();
    }
    public sealed class PrepareNative8RegistrationTask : IsolatedBuildTask
    {
        public string ManagedCompilerBundle { get; set; } = string.Empty;
        public string IlcHostPackagePath { get; set; } = string.Empty;
        public string HostRid { get; set; } = string.Empty;
        public string TargetRid { get; set; } = string.Empty;
        public string IntermediateRoot { get; set; } = string.Empty;
        public string DotNetHostPath { get; set; } = string.Empty;
        public string NativeObject { get; set; } = string.Empty;
        public string ExportsFile { get; set; } = string.Empty;
        public string NativeBinary { get; set; } = string.Empty;
        public string NativeLinker { get; set; } = string.Empty;
        public string InvocationId { get; set; } = string.Empty;
        public string OwnedRoot { get; set; } = string.Empty;
        [Output] public string ScopedIlcToolsPath { get; set; } = string.Empty;
        [Output] public string PrepareInputsRoot { get; set; } = string.Empty;
        [Output] public string ReceiptPointer { get; set; } = string.Empty;
        [Output] public string HostFingerprint { get; set; } = string.Empty;
        [Output] public string ScopedNativeLinkerPath { get; set; } = string.Empty;
    }
    public sealed class VerifyNative8RegistrationTask : IsolatedBuildTask
    {
        public string ReceiptPointer { get; set; } = string.Empty;
        public string NativeObject { get; set; } = string.Empty;
        public string ExportsFile { get; set; } = string.Empty;
        public string NativeBinary { get; set; } = string.Empty;
        public string InvocationId { get; set; } = string.Empty;
        public string OwnedRoot { get; set; } = string.Empty;
        public string Phase { get; set; } = string.Empty;
        public string ExpectedLinkedReceiptHash { get; set; } = string.Empty;
        [Output] public string VerifiedReceipt { get; set; } = string.Empty;
        [Output] public string LinkedReceiptHash { get; set; } = string.Empty;
    }
    public sealed class PrepareConditionalRegistrationBridgeTask : IsolatedBuildTask
    {
        public string OutputRoot { get; set; } = string.Empty;
        public string Configuration { get; set; } = string.Empty;
        public ITaskItem[] Assemblies { get; set; } = Array.Empty<ITaskItem>();
        public ITaskItem[] AdditionalInputs { get; set; } = Array.Empty<ITaskItem>();
        public string EngineArguments { get; set; } = string.Empty;
        public string BridgeAssembly { get; set; } = string.Empty;
        public string CecilAssembly { get; set; } = string.Empty;
        public string RuntimePackDirectory { get; set; } = string.Empty;
        public string ApplicationAssemblyIdentity { get; set; } = string.Empty;
        public string DotNetExecutable { get; set; } = string.Empty;
        public bool ProbeOnly { get; set; }
        [Output] public bool NeedsBridge { get; set; }
        [Output] public string InputsPath { get; set; } = string.Empty;
        [Output] public string TransformationPath { get; set; } = string.Empty;
        [Output] public ITaskItem[] PreparedAssemblies { get; set; } = Array.Empty<ITaskItem>();
        [Output] public ITaskItem[] PreparedAdditionalInputs { get; set; } = Array.Empty<ITaskItem>();
        [Output] public string PreparedEngineArguments { get; set; } = string.Empty;
    }
    public sealed class VerifyConditionalRegistrationBridgeTask : IsolatedBuildTask
    {
        public string InputsPath { get; set; } = string.Empty;
        public string TransformationPath { get; set; } = string.Empty;
        public ITaskItem[] ConsumedAssemblies { get; set; } = Array.Empty<ITaskItem>();
        public string OutputDirectory { get; set; } = string.Empty;
        public string ReceiptPath { get; set; } = string.Empty;
        public string Phase { get; set; } = string.Empty;
        public string EngineKind { get; set; } = string.Empty;
        public ITaskItem[] OutputFiles { get; set; } = Array.Empty<ITaskItem>();
        public ITaskItem[] ConsumedAdditionalInputs { get; set; } = Array.Empty<ITaskItem>();
    }
    public sealed class VerifyConditionalRegistrationOutputTask : IsolatedBuildTask
    {
        public string InputsPath { get; set; } = string.Empty;
        public string OutputDirectory { get; set; } = string.Empty;
        public string ReceiptPath { get; set; } = string.Empty;
        public string LinkerAssembly { get; set; } = string.Empty;
        public bool VerifyConsumed { get; set; }
    }
    public sealed class PrepareNet8LinkHostTask : IsolatedBuildTask
    {
        public string LinkerTaskAssembly { get; set; } = string.Empty;
        public string BackendAssembly { get; set; } = string.Empty;
        public string OutputRoot { get; set; } = string.Empty;
        public string OverrideSourceTemplate { get; set; } = string.Empty;
        public string OverrideSourceText { get; set; } = string.Empty;
        [Output] public string LinkerPath { get; set; } = string.Empty;
        [Output] public string TaskOverrideSource { get; set; } = string.Empty;
        [Output] public string HostFingerprint { get; set; } = string.Empty;
        [Output] public ITaskItem[] HostInputs { get; set; } = Array.Empty<ITaskItem>();
    }
    public sealed class PrepareConditionalRegistrationInputsTask : IsolatedBuildTask
    {
        public string OutputRoot { get; set; } = string.Empty;
        public string Configuration { get; set; } = string.Empty;
        public string EngineArguments { get; set; } = string.Empty;
        public ITaskItem[] Assemblies { get; set; } = Array.Empty<ITaskItem>();
        public ITaskItem[] AdditionalInputs { get; set; } = Array.Empty<ITaskItem>();
        [Output] public string InputsPath { get; set; } = string.Empty;
        [Output] public string InputFingerprint { get; set; } = string.Empty;
        [Output] public string PreparedEngineArguments { get; set; } = string.Empty;
        [Output] public ITaskItem[] PreparedAssemblies { get; set; } = Array.Empty<ITaskItem>();
        [Output] public ITaskItem[] PreparedAdditionalInputs { get; set; } = Array.Empty<ITaskItem>();
    }
    public sealed class ValidateRegistrationToolchainTask : IsolatedBuildTask
    {
        public string TargetFrameworkIdentifier { get; set; } = string.Empty;
        public string TargetFrameworkVersion { get; set; } = string.Empty;
        public string LinkerAssembly { get; set; } = string.Empty;
        public string NativeCompiler { get; set; } = string.Empty;
    }
    public sealed class PrepareTypeMapLinkTask : IsolatedBuildTask
    {
        public string SdkVersion { get; set; } = string.Empty;
        public string WasmSdkTasksPath { get; set; } = string.Empty;
        public string BackendAssembly { get; set; } = string.Empty;
        public string LinkerAssembly { get; set; } = string.Empty;
        public string Capability { get; set; } = string.Empty;
        public string ReceiptPath { get; set; } = string.Empty;
        public string SignaturePath { get; set; } = string.Empty;
        public string LinkSemaphore { get; set; } = string.Empty;
        public string OutputDirectory { get; set; } = string.Empty;
        public string Configuration { get; set; } = string.Empty;
        public ITaskItem[] Inputs { get; set; } = Array.Empty<ITaskItem>();
        public ITaskItem[] Assemblies { get; set; } = Array.Empty<ITaskItem>();
        public ITaskItem[] LinkOptions { get; set; } = Array.Empty<ITaskItem>();
        public ITaskItem[] LinkItems { get; set; } = Array.Empty<ITaskItem>();
    }
    public sealed class VerifyTypeMapReceiptTask : IsolatedBuildTask
    {
        public string ReceiptPath { get; set; } = string.Empty;
        public string SignaturePath { get; set; } = string.Empty;
        public bool BindReceipt { get; set; }
        public bool RequireConsumedAssemblies { get; set; }
        public ITaskItem[] ConsumedAssemblies { get; set; } = Array.Empty<ITaskItem>();
    }
    public sealed class ResolveRegistrationToolsTask : IsolatedBuildTask
    {
        public ITaskItem[] Candidates { get; set; } = Array.Empty<ITaskItem>();
        public bool IncludeBackend { get; set; }
        public bool IncludeNet8Backend { get; set; }
        [Output] public string GeneratorAssembly { get; set; } = string.Empty;
        [Output] public string BackendAssembly { get; set; } = string.Empty;
        [Output] public string Net8BackendAssembly { get; set; } = string.Empty;
    }
    public sealed class GenerateBuiltInThemeDocumentTask : IsolatedBuildTask
    {
        [Required] public string SourcePath { get; set; } = string.Empty;
        [Required] public string SourceIdentity { get; set; } = string.Empty;
        [Required] public string GeneratedCodePath { get; set; } = string.Empty;
    }
    public sealed class GenerateThemeAssetWrappersTask : IsolatedBuildTask
    {
        [Required] public ITaskItem[] ThemeAssets { get; set; } = new ITaskItem[0];
        [Required] public string OutputDirectory { get; set; } = string.Empty;
        [Required] public string AssemblyName { get; set; } = string.Empty;
        [Required] public string GeneratedCodePath { get; set; } = string.Empty;
        [Output] public ITaskItem[] GeneratedAssets { get; set; } = new ITaskItem[0];
    }
    public sealed class ExportLanguageTemplatesTask : IsolatedBuildTask
    {
        [Required] public ITaskItem[] SourceFiles { get; set; } = Array.Empty<ITaskItem>();
        [Required] public string TargetLanguage { get; set; } = string.Empty;
        public string OutputRootDirectory { get; set; }
        [Output] public ITaskItem[] ExportedFiles { get; set; } = Array.Empty<ITaskItem>();
    }
    public sealed class PrepareLanguagePackageAssetsTask : IsolatedBuildTask
    {
        [Required] public string PackageId { get; set; } = string.Empty;
        [Required] public ITaskItem[] LanguageFiles { get; set; } = Array.Empty<ITaskItem>();
        public ITaskItem[] SourceLanguageFiles { get; set; } = Array.Empty<ITaskItem>();
        public string ExpectedLanguage { get; set; }
        public string MinimumTargetState { get; set; } = "final";
        public bool RequireVerifiedContract { get; set; }
        [Output] public ITaskItem[] PreparedLanguageFiles { get; set; } = Array.Empty<ITaskItem>();
    }
    public sealed class PrepareLanguagePackageTask : IsolatedBuildTask
    {
        [Required] public string PackageId { get; set; } = string.Empty;
        [Required] public ITaskItem[] LanguageFiles { get; set; } = Array.Empty<ITaskItem>();
        public ITaskItem[] SourceLanguageFiles { get; set; } = Array.Empty<ITaskItem>();
        public ITaskItem[] PackageFiles { get; set; } = Array.Empty<ITaskItem>();
        [Required] public string OutputManifestPath { get; set; } = string.Empty;
        public string ExpectedLanguage { get; set; }
        public string MinimumTargetState { get; set; } = "final";
        public bool RequireVerifiedContract { get; set; }
        [Output] public ITaskItem[] PreparedLanguageFiles { get; set; } = Array.Empty<ITaskItem>();
    }
    public sealed class GenerateLanguagePackagePropsTask : IsolatedBuildTask
    {
        [Required] public string PackageId { get; set; } = string.Empty;
        [Required] public ITaskItem[] LanguageFiles { get; set; } = Array.Empty<ITaskItem>();
        [Required] public string OutputPath { get; set; } = string.Empty;
        public bool RequireTargetLanguage { get; set; } = true;
        public string SourceKind { get; set; } = "StaticLanguagePack";
    }
}

#endif
