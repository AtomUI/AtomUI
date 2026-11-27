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
    /// <summary>
    /// The request that the adapter writes to a file before it starts the worker process. It names the task to
    /// run and carries the inputs of the task as strings and as <see cref="TaskWireItem" /> arrays.
    /// </summary>
    [DataContract]
    public sealed class TaskRequest
    {
        /// <summary>
        /// Name of the adapter task type. The worker runs the task of the same name; an unknown name fails the
        /// worker process before a task runs.
        /// </summary>
        [DataMember] public string TaskName { get; set; }
        /// <summary>
        /// Project file of the task invocation, as the build engine reports it to the adapter. The worker hands
        /// it to the task as the project file of its task node.
        /// </summary>
        [DataMember] public string ProjectFile;
        /// <summary>
        /// Line of the task invocation in the project file, as the build engine reports it to the adapter. The
        /// worker hands it to the task as the line of its task node.
        /// </summary>
        [DataMember] public int Line;
        /// <summary>
        /// Column of the task invocation in the project file, as the build engine reports it to the adapter.
        /// The worker hands it to the task as the column of its task node.
        /// </summary>
        [DataMember] public int Column;
        /// <summary>
        /// Scalar inputs keyed by property name. Every public input property of the adapter that is not an item
        /// array is written as an invariant-culture string. A <see langword="null" /> value is left out, so the
        /// worker task keeps its own default.
        /// </summary>
        [DataMember] public Dictionary<string, string> Properties = new Dictionary<string, string>();
        /// <summary>
        /// Item inputs keyed by property name: every item array of the adapter as copies made by
        /// <see cref="TaskWireItem.FromItem" />.
        /// </summary>
        [DataMember] public Dictionary<string, TaskWireItem[]> Items = new Dictionary<string, TaskWireItem[]>();
    }

    /// <summary>
    /// The result that the worker process writes to its standard output: the outcome of the task, its output
    /// values and the diagnostics it logged. The adapter reads it after the process exited with code 0.
    /// </summary>
    [DataContract]
    public sealed class TaskResponse
    {
        /// <summary>
        /// Whether the task succeeded and logged no error: it is <see langword="true" /> only when the task's
        /// <c>Execute</c> returned <see langword="true" /> and no diagnostic of kind <c>error</c> exists. The
        /// adapter fails the invocation when it is <see langword="false" />.
        /// </summary>
        [DataMember] public bool Success;
        /// <summary>
        /// Scalar outputs keyed by output property name, each as an invariant-culture string. The adapter
        /// converts them back to the type of its output property.
        /// </summary>
        [DataMember] public Dictionary<string, string> Properties = new Dictionary<string, string>();
        /// <summary>
        /// Item outputs keyed by output property name. The adapter assigns the arrays to its output properties
        /// as they are.
        /// </summary>
        [DataMember] public Dictionary<string, TaskWireItem[]> Items = new Dictionary<string, TaskWireItem[]>();
        /// <summary>
        /// The messages, warnings and errors that the task logged, in logging order. The adapter replays them
        /// into the build log.
        /// </summary>
        [DataMember] public List<TaskDiagnostic> Diagnostics = new List<TaskDiagnostic>();
    }

    /// <summary>
    /// Value-only stand-in for an MSBuild item that crosses the process boundary. It implements
    /// <see cref="ITaskItem" />, so the worker task and the adapter use it as the type of their items.
    /// </summary>
    [DataContract]
    public sealed class TaskWireItem : ITaskItem
    {
        /// <summary>
        /// The item spec of the item, which for a file item is its path as MSBuild saw it.
        /// </summary>
        [DataMember] public string ItemSpec { get; set; }
        /// <summary>
        /// The <c>FullPath</c> value of the item. It is kept apart from <see cref="Metadata" /> and is what
        /// <see cref="GetMetadata" /> returns for <c>FullPath</c>; it is <see langword="null" /> until it is
        /// set.
        /// </summary>
        [DataMember] public string FullPath;
        /// <summary>
        /// The custom metadata entries, keyed by a name that ignores case. The <c>FullPath</c> value lives in
        /// <see cref="FullPath" /> instead.
        /// </summary>
        [DataMember] public Dictionary<string, string> Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Number of custom metadata entries, not counting <see cref="FullPath" />.
        /// </summary>
        public int MetadataCount { get { return Metadata.Count; } }
        /// <summary>
        /// Names of the custom metadata entries as a snapshot array: changes made to <see cref="Metadata" />
        /// after the call are not reflected.
        /// </summary>
        public ICollection MetadataNames { get { return Metadata.Keys.ToArray(); } }
        /// <summary>
        /// Returns the value of the custom metadata entry <paramref name="name" />, or <see cref="FullPath" />
        /// for the name <c>FullPath</c>. A missing entry yields an empty string, never
        /// <see langword="null" />.
        /// </summary>
        /// <param name="name">
        /// The metadata name, compared without regard to case.
        /// </param>
        /// <returns>
        /// The metadata value, or an empty string when there is none.
        /// </returns>
        public string GetMetadata(string name)
        {
            string value;
            if (Metadata.TryGetValue(name, out value))
            {
                return value;
            }

            return name.Equals("FullPath", StringComparison.OrdinalIgnoreCase) ? FullPath ?? string.Empty : string.Empty;
        }
        /// <summary>
        /// Adds or replaces the custom metadata entry <paramref name="name" />.
        /// </summary>
        /// <param name="name">
        /// The metadata name, compared without regard to case.
        /// </param>
        /// <param name="value">
        /// The value to store, taken as given.
        /// </param>
        public void SetMetadata(string name, string value) { Metadata[name] = value; }
        /// <summary>
        /// Removes the custom metadata entry <paramref name="name" />. A missing name is ignored.
        /// </summary>
        /// <param name="name">
        /// The metadata name, compared without regard to case.
        /// </param>
        public void RemoveMetadata(string name) { Metadata.Remove(name); }
        /// <summary>
        /// Returns a copy of the custom metadata entries. <see cref="FullPath" /> is not part of it.
        /// </summary>
        /// <returns>
        /// A new dictionary of metadata names to values, with names that ignore case.
        /// </returns>
        public IDictionary CloneCustomMetadata() { return new Dictionary<string, string>(Metadata, StringComparer.OrdinalIgnoreCase); }
        /// <summary>
        /// Copies every custom metadata entry to <paramref name="destination" /> that has no value of that
        /// name yet. A value is set literally when the destination implements <see cref="ITaskItem2" />, so
        /// that no escape sequence in it is interpreted; otherwise <see cref="SetMetadata" /> is used.
        /// </summary>
        /// <param name="destination">
        /// The item that receives the metadata entries.
        /// </param>
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

        /// <summary>
        /// Copies an MSBuild item into a wire item: its item spec, its <c>FullPath</c> and its custom metadata
        /// entries.
        /// </summary>
        /// <param name="item">
        /// The item to copy.
        /// </param>
        /// <returns>
        /// A new <see cref="TaskWireItem" /> with the values of <paramref name="item" />.
        /// </returns>
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

    /// <summary>
    /// One message that the worker task logged. The adapter writes it to the build log as an error, a
    /// warning or a message, according to <see cref="Kind" />.
    /// </summary>
    [DataContract]
    public sealed class TaskDiagnostic
    {
        /// <summary>
        /// <c>error</c>, <c>warning</c> or <c>message</c>. An error also makes
        /// <see cref="TaskResponse.Success" /> <see langword="false" />.
        /// </summary>
        [DataMember] public string Kind;
        /// <summary>
        /// The text of the message.
        /// </summary>
        [DataMember] public string Message;
        /// <summary>
        /// The subcategory of the error or warning. It is not read for a message.
        /// </summary>
        [DataMember] public string Subcategory;
        /// <summary>
        /// The code of the error or warning. It is not read for a message.
        /// </summary>
        [DataMember] public string Code;
        /// <summary>
        /// The help keyword of the error or warning. It is not read for a message.
        /// </summary>
        [DataMember] public string HelpKeyword;
        /// <summary>
        /// The file the error or warning belongs to. It is not read for a message.
        /// </summary>
        [DataMember] public string File;
        /// <summary>
        /// The line in <see cref="File" /> that the error or warning points to. It is not read for a message.
        /// </summary>
        [DataMember] public int Line;
        /// <summary>
        /// The column in <see cref="File" /> that the error or warning points to. It is not read for a message.
        /// </summary>
        [DataMember] public int Column;
        /// <summary>
        /// The last line of the range that the error or warning points to. It is not read for a message.
        /// </summary>
        [DataMember] public int EndLine;
        /// <summary>
        /// The last column of the range that the error or warning points to. It is not read for a message.
        /// </summary>
        [DataMember] public int EndColumn;
        /// <summary>
        /// The numeric value of the <see cref="MessageImportance" /> of a message; the adapter logs the
        /// message with that importance. It is not read for an error or a warning.
        /// </summary>
        [DataMember] public int Importance;
    }

    /// <summary>
    /// Serializes the request and the response between the adapter and the worker process as JSON.
    /// </summary>
    public static class TaskWire
    {
        /// <summary>
        /// Creates or overwrites the file at <paramref name="path" /> and writes <paramref name="value" />
        /// to it as JSON.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the value. Only its members marked <c>DataMember</c> are written.
        /// </typeparam>
        /// <param name="path">
        /// The file to write.
        /// </param>
        /// <param name="value">
        /// The value to serialize.
        /// </param>
        public static void Write<T>(string path, T value)
        {
            using (var stream = File.Create(path))
            {
                Serializer<T>().WriteObject(stream, value);
            }
        }

        /// <summary>
        /// Reads a value of type <typeparamref name="T" /> from JSON text.
        /// </summary>
        /// <typeparam name="T">
        /// The type to deserialize. Only its members marked <c>DataMember</c> are filled from the JSON.
        /// </typeparam>
        /// <param name="json">
        /// The JSON document as text.
        /// </param>
        /// <returns>
        /// The deserialized value.
        /// </returns>
        public static T Read<T>(string json)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                return (T)Serializer<T>().ReadObject(stream);
            }
        }

        /// <summary>
        /// Writes <paramref name="response" /> as JSON to the standard output of the worker process. The
        /// adapter reads the whole standard output as this response, so nothing else may be written to that
        /// stream.
        /// </summary>
        /// <param name="response">
        /// The result of the worker.
        /// </param>
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
    /// <summary>
    /// Base class of the adapters through which MSBuild reaches the tasks of the toolchain. An adapter
    /// carries no task logic: it writes the public input properties it declares into a
    /// <see cref="TaskRequest" /> file, starts the worker process on <see cref="TaskAssembly" />, which
    /// runs the task of the same name, and copies the outputs of the <see cref="TaskResponse" /> into the
    /// properties marked <c>[Output]</c>. The properties of an adapter therefore carry the inputs and
    /// outputs of that task.
    /// </summary>
    public abstract class IsolatedBuildTask : Task, ICancelableTask
    {
        /// <summary>
        /// Path of the task assembly that holds the real tasks. The worker process loads it; it is passed to
        /// the process as its first argument.
        /// </summary>
        [Required] public string TaskAssembly { get; set; }
        /// <summary>
        /// Path of the dotnet host that starts the worker process. When it is <see langword="null" /> or
        /// blank, the process is started as <c>dotnet</c> and resolved through <c>PATH</c>.
        /// </summary>
        public string DotNetPath { get; set; }

        private readonly object processGate = new object();
        private Process process;
        private bool cancelled;

        /// <summary>
        /// Stops the invocation when MSBuild cancels the build. A worker process that is running is killed; a
        /// worker that was not started yet is never started, so the invocation fails without one.
        /// </summary>
        public void Cancel()
        {
            lock (processGate)
            {
                cancelled = true;
                if (process != null && !process.HasExited)
                    process.Kill();
            }
        }

        /// <summary>
        /// Runs the task in the worker process and copies its outputs into the output properties of this
        /// adapter. The diagnostics of the worker are written to the build log, and the text it wrote to its
        /// error stream is logged as high-importance messages. A worker that exits with a non-zero code fails
        /// the task. The temporary directory that holds the request file is always deleted, and a worker that
        /// is still running is terminated.
        /// </summary>
        /// <returns>
        /// <see langword="true" /> when the worker reported success and no error was logged; otherwise
        /// <see langword="false" />.
        /// </returns>
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
    /// <summary>
    /// Connects verified linked inputs to the SDK's R2R and bundle outputs without repeating the dependency
    /// analysis.
    /// </summary>
    public sealed class ConditionalPublishTransformTask : IsolatedBuildTask
    {
        /// <summary>
        /// The transform step to run: <c>capture-r2r</c>, <c>verify-r2r</c>, <c>capture-bundle</c>,
        /// <c>verify-bundle</c> or <c>verify-published</c>. Each step requires a stage that an earlier step
        /// produced, and any other value fails the task.
        /// </summary>
        public string Operation { get; set; } = string.Empty;
        /// <summary>
        /// Path of the inputs file of the link invocation whose verified output this task transforms. The SHA-256
        /// of this file ties the transform state to that invocation: it must equal the <c>inputHash</c> in the
        /// verified receipt and the hash stored in an existing state, otherwise the task fails.
        /// </summary>
        public string InputsPath { get; set; } = string.Empty;
        /// <summary>
        /// Path of the receipt that verified linking wrote for the same invocation. When no state exists yet, the
        /// receipt must report the stage <c>verified</c> and the hash of <see cref="InputsPath" />, and its
        /// <c>outputs</c> entries become the files that the chain starts from. A receipt replaced between steps
        /// fails the task.
        /// </summary>
        public string VerifiedReceiptPath { get; set; } = string.Empty;
        /// <summary>
        /// Path of the JSON file that carries the transform state from one step to the next. When the file is
        /// missing, the state is built from the verified receipt. Every successful step rewrites the file, and a
        /// failed step deletes it, so that the chain restarts from the verified link.
        /// </summary>
        public string StatePath { get; set; } = string.Empty;
        /// <summary>
        /// Directory that the SDK published to. Only <c>verify-published</c> reads it: the bundle must be the
        /// file of that name in this directory, and every other tracked file that the bundle did not embed must
        /// be there under its relative path with the verified hash.
        /// </summary>
        public string PublishDirectory { get; set; } = string.Empty;
        /// <summary>
        /// Path of the single-file bundle that the SDK produced. <c>capture-bundle</c> records it,
        /// <c>verify-bundle</c> reads the hashes of the embedded files from it, and <c>verify-published</c>
        /// requires that it lies in <see cref="PublishDirectory" /> under the same file name and is unchanged.
        /// </summary>
        public string BundlePath { get; set; } = string.Empty;
        /// <summary>
        /// Option text that a capture step records in the state: <c>capture-r2r</c> stores it as the state's
        /// options and <c>capture-bundle</c> appends it with the <c>bundle:</c> prefix. The task does not
        /// interpret it.
        /// </summary>
        public string Options { get; set; } = string.Empty;
        /// <summary>
        /// Files that the SDK feeds to the step. <c>capture-r2r</c> expects the assemblies to compile: each must
        /// be in the verified linked set and carry the path of its R2R image in the <c>OutputR2RImage</c>
        /// metadata. <c>capture-bundle</c> expects the files that go into the bundle, each with its
        /// bundle-relative path in the <c>RelativePath</c> metadata.
        /// </summary>
        public ITaskItem[] Files { get; set; } = Array.Empty<ITaskItem>();
        /// <summary>
        /// Reference assemblies of the R2R compilation. Only <c>capture-r2r</c> reads them: each must be in the
        /// verified linked set with an unchanged hash, so that the references stay inside the closure that
        /// linking verified.
        /// </summary>
        public ITaskItem[] References { get; set; } = Array.Empty<ITaskItem>();
        /// <summary>
        /// Tools that the chain must keep unchanged. <c>capture-r2r</c> copies each item under <c>r2r-tools</c>
        /// next to the state file and tracks the copies: an item whose <c>Kind</c> metadata is
        /// <c>profile</c> is copied as a single file, any other item is a file whose whole directory is copied.
        /// The verification steps fail when a tracked copy or item changed.
        /// </summary>
        public ITaskItem[] Tools { get; set; } = Array.Empty<ITaskItem>();
        /// <summary>
        /// Outputs of a composite R2R compilation. Each needs a <c>RelativePath</c> metadata value that is
        /// relative and has no <c>..</c> part. Only <c>capture-r2r</c> reads them: an output whose relative
        /// path is that of a verified file becomes the R2R image of that file, any other is a generated output
        /// that the following steps track by hash.
        /// </summary>
        public ITaskItem[] ExtraOutputs { get; set; } = Array.Empty<ITaskItem>();
        /// <summary>
        /// The <see cref="Tools" /> items as <c>capture-r2r</c> froze them: each points to its copy under the
        /// <c>r2r-tools</c> directory next to the state file, and metadata values of a copied tool directory
        /// that were fully qualified paths under its source directory point into the copy as well. It is empty
        /// for the other steps.
        /// </summary>
        [Output] public ITaskItem[] PreparedTools { get; set; } = Array.Empty<ITaskItem>();
    }
    /// <summary>
    /// Combines a maintainer-built managed ILC8 compiler with the host native helpers of the SDK's .NET 8 ILC
    /// package and adds the generated <c>ilc</c> and <c>native-linker</c> launchers.
    /// </summary>
    public sealed class PrepareNative8RegistrationTask : IsolatedBuildTask
    {
        /// <summary>
        /// Directory of the maintainer-built managed ILC8 compiler. It must contain exactly the audited compiler
        /// files and the <c>atomui-ilc8-capability.json</c> that describes them, and every compiler file must
        /// match the hash that the capability file records.
        /// </summary>
        public string ManagedCompilerBundle { get; set; } = string.Empty;
        /// <summary>
        /// Directory of the SDK-restored <c>runtime.osx-arm64.microsoft.dotnet.ilcompiler</c> 8.x package. The
        /// native helpers <c>libjitinterface</c>, <c>libobjwriter</c> and the JIT for
        /// <see cref="TargetRid" /> are read from its <c>tools</c> subdirectory.
        /// </summary>
        public string IlcHostPackagePath { get; set; } = string.Empty;
        /// <summary>
        /// Runtime identifier of the build host. It must be <c>osx-arm64</c>, and the process must run on macOS
        /// ARM64; any other host or target fails the task. It is part of <see cref="HostFingerprint" />.
        /// </summary>
        public string HostRid { get; set; } = string.Empty;
        /// <summary>
        /// Runtime identifier that the native compilation targets, <c>osx-arm64</c> or <c>osx-x64</c>. It
        /// selects the JIT helper that is copied from <see cref="IlcHostPackagePath" /> and is part of
        /// <see cref="HostFingerprint" />.
        /// </summary>
        public string TargetRid { get; set; } = string.Empty;
        /// <summary>
        /// Root directory for what this task derives from its inputs: the inputs directory
        /// <see cref="PrepareInputsRoot" />, the prepared host under <c>hosts</c> and the lock file that
        /// serializes the preparation of a host.
        /// </summary>
        public string IntermediateRoot { get; set; } = string.Empty;
        /// <summary>
        /// Path of the dotnet host that executes the managed compiler. The generated <c>ilc</c> launcher runs
        /// the compiler through it, and the cleanup command appended to both launchers runs the compiler through
        /// it as well.
        /// </summary>
        public string DotNetHostPath { get; set; } = string.Empty;
        /// <summary>
        /// Path of the object file that the native compilation produces. It must lie under
        /// <see cref="OwnedRoot" /> and is passed to the cleanup command that the launchers run after a failure.
        /// </summary>
        public string NativeObject { get; set; } = string.Empty;
        /// <summary>
        /// Path of the export list that the native compilation produces, one symbol per line. It must lie under
        /// <see cref="OwnedRoot" /> and is passed to the cleanup command that the launchers run after a failure.
        /// </summary>
        public string ExportsFile { get; set; } = string.Empty;
        /// <summary>
        /// Path of the native binary that the native compilation produces. It must lie under
        /// <see cref="OwnedRoot" /> and is passed to the cleanup command that the launchers run after a failure.
        /// </summary>
        public string NativeBinary { get; set; } = string.Empty;
        /// <summary>
        /// Path of the native linker. The generated <c>native-linker</c> launcher calls it with every argument
        /// it received and, when the linker exits non-zero, runs the cleanup command of this invocation.
        /// </summary>
        public string NativeLinker { get; set; } = string.Empty;
        /// <summary>
        /// Identifier of this native compilation invocation. The task records it with <see cref="OwnedRoot" />
        /// in the invocation file under that root, and the launchers pass it to the cleanup command that they run
        /// after a failure.
        /// </summary>
        public string InvocationId { get; set; } = string.Empty;
        /// <summary>
        /// Absolute root directory of this invocation. <see cref="NativeObject" />, <see cref="ExportsFile" />
        /// and <see cref="NativeBinary" /> must lie under it; the task replaces them with the result of that
        /// ownership check. The invocation file is written under this root, the receipt is expected under it,
        /// and a failed run deletes the files that it created there.
        /// </summary>
        public string OwnedRoot { get; set; } = string.Empty;
        /// <summary>
        /// The <c>tools</c> directory of the prepared host, with a trailing directory separator. It holds the
        /// generated <c>ilc</c> and <c>native-linker</c> launchers. It is empty when the task fails.
        /// </summary>
        [Output] public string ScopedIlcToolsPath { get; set; } = string.Empty;
        /// <summary>
        /// Directory <c>inputs</c> under <see cref="IntermediateRoot" /> that this task designates for the
        /// inputs of the native compilation. The task only names the directory and does not create it. It is
        /// empty when the task fails.
        /// </summary>
        [Output] public string PrepareInputsRoot { get; set; } = string.Empty;
        /// <summary>
        /// Path of the <c>receipt.json</c> file of this invocation under <see cref="OwnedRoot" />, where the
        /// receipt of the native compilation is expected and is read back by the verification step. It is empty
        /// when the task fails.
        /// </summary>
        [Output] public string ReceiptPointer { get; set; } = string.Empty;
        /// <summary>
        /// SHA-256 of the serialized host state: the host and target RIDs with the path and hash of every file
        /// that is copied into the prepared host. It names the host directory under <c>hosts</c> and the lock
        /// file that guards its preparation. It is empty when the task fails.
        /// </summary>
        [Output] public string HostFingerprint { get; set; } = string.Empty;
        /// <summary>
        /// Path of the generated <c>native-linker</c> launcher in the prepared host. It is empty when the task
        /// fails.
        /// </summary>
        [Output] public string ScopedNativeLinkerPath { get; set; } = string.Empty;
    }
    /// <summary>
    /// Verifies that the native image, the object file and the export list belong to one native compilation
    /// invocation and come from its unchanged frozen inputs, then writes the receipt that the next task
    /// takes.
    /// </summary>
    public sealed class VerifyNative8RegistrationTask : IsolatedBuildTask
    {
        /// <summary>
        /// Path of the compilation receipt, the invocation's <c>receipt.json</c>. The native compilation task
        /// wrote it for this invocation: it must carry the invocation's identifier and name the analysis report
        /// with its hash.
        /// </summary>
        public string ReceiptPointer { get; set; } = string.Empty;
        /// <summary>
        /// Object file of the native compilation. It must lie under <see cref="OwnedRoot" /> and match the path
        /// and hash that the analysis report records.
        /// </summary>
        public string NativeObject { get; set; } = string.Empty;
        /// <summary>
        /// Export list of the native compilation, one symbol per line. It must lie under
        /// <see cref="OwnedRoot" /> and match the path and hash in the analysis report, and contain the
        /// compilation export exactly once.
        /// </summary>
        public string ExportsFile { get; set; } = string.Empty;
        /// <summary>
        /// The native executable to verify. In the <c>linked</c> phase it is the image the SDK just linked,
        /// which must be under <see cref="OwnedRoot" />. In the <c>published</c> phase it is the published
        /// copy, which must be byte-identical to the verified linked image.
        /// </summary>
        public string NativeBinary { get; set; } = string.Empty;
        /// <summary>
        /// Identifier of the native compilation invocation: a lowercase GUID of 32 hex digits. The compilation
        /// receipt, the analysis report, the linked proof and the exported compilation symbol must all carry it.
        /// </summary>
        public string InvocationId { get; set; } = string.Empty;
        /// <summary>
        /// Absolute root directory of this invocation. Every input except a published
        /// <see cref="NativeBinary" /> must be under it, and the receipts the task writes are placed in it.
        /// </summary>
        public string OwnedRoot { get; set; } = string.Empty;
        /// <summary>
        /// Verification phase, <c>linked</c> or <c>published</c>; anything else fails the task. <c>linked</c>
        /// verifies the image the SDK just linked and records its proof, and fails when a proof already exists.
        /// <c>published</c> verifies that a published copy still matches that proof, and fails when no proof
        /// exists.
        /// </summary>
        public string Phase { get; set; } = string.Empty;
        /// <summary>
        /// Hash of the linked proof as <see cref="LinkedReceiptHash" /> returned it in the <c>linked</c>
        /// phase. In the <c>published</c> phase the task fails when the stored proof no longer has this hash.
        /// It is not read in the <c>linked</c> phase.
        /// </summary>
        public string ExpectedLinkedReceiptHash { get; set; } = string.Empty;
        /// <summary>
        /// Path of the receipt to hand to the next task: the linked proof (<c>linked.json</c>) in the
        /// <c>linked</c> phase, or a new receipt (<c>verified.json</c>) that binds the published copy to that
        /// proof in the <c>published</c> phase. It is empty when the task failed.
        /// </summary>
        [Output] public string VerifiedReceipt { get; set; } = string.Empty;
        /// <summary>
        /// Hash of the linked proof, set in both phases. Pass it back as
        /// <see cref="ExpectedLinkedReceiptHash" /> when the <c>published</c> phase runs. It is empty when the
        /// task failed.
        /// </summary>
        [Output] public string LinkedReceiptHash { get; set; } = string.Empty;
    }
    /// <summary>
    /// Probes whether the assemblies need the conditional registration bridge and, when they do, freezes the
    /// inputs, runs the bridge tool in a separate process and validates the transformation it wrote. Without a
    /// bridge the inputs pass through to the outputs unchanged.
    /// </summary>
    public sealed class PrepareConditionalRegistrationBridgeTask : IsolatedBuildTask
    {
        /// <summary>
        /// Root under which the task writes: the frozen input snapshot in its <c>inputs</c> directory and the copy
        /// of the bridge tool in its <c>host</c> directory.
        /// </summary>
        public string OutputRoot { get; set; } = string.Empty;
        /// <summary>
        /// Text that identifies the settings of the invocation, such as the target framework, the runtime identifier
        /// and the build configuration. The task appends <c>bridge-v1</c>, <see cref="RuntimePackDirectory" /> and
        /// <see cref="ApplicationAssemblyIdentity" /> to it.
        /// </summary>
        public string Configuration { get; set; } = string.Empty;
        /// <summary>
        /// The assemblies to prepare for the engine, each item spec a managed assembly path. The probe for the
        /// bridge looks for <c>AtomUI.Core</c> among them: none means no bridge, and more than one
        /// <c>AtomUI.Core</c> identity fails the task.
        /// </summary>
        public ITaskItem[] Assemblies { get; set; } = Array.Empty<ITaskItem>();
        /// <summary>
        /// Inputs other than <see cref="Assemblies" />. An item whose <c>EngineOption</c> is
        /// <c>substitutions</c> must be quoted in <see cref="EngineArguments" /> exactly once, and a
        /// <c>custom-step</c> item of type <c>AtomUI.TypeMap.Linker.MaterializeTypeMapsStep</c> must have the same
        /// content as <see cref="BridgeAssembly" />. Only a <c>tool</c> item may carry
        /// <c>AtomUIExecutable=true</c>.
        /// </summary>
        public ITaskItem[] AdditionalInputs { get; set; } = Array.Empty<ITaskItem>();
        /// <summary>
        /// The engine arguments the SDK produced. Each substitution input declared in
        /// <see cref="AdditionalInputs" /> must be quoted in them exactly once as a <c>--substitutions</c>
        /// argument. Those arguments are removed before the input preparation sees them.
        /// </summary>
        public string EngineArguments { get; set; } = string.Empty;
        /// <summary>
        /// Path of the <c>AtomUI.TypeMap.Linker.dll</c> bridge tool. It is copied with its <c>.deps.json</c> and
        /// <c>.runtimeconfig.json</c> into a host bundle under <see cref="OutputRoot" />, which is then frozen
        /// with the other inputs.
        /// </summary>
        public string BridgeAssembly { get; set; } = string.Empty;
        /// <summary>
        /// Path of the <c>Mono.Cecil.dll</c> the bridge tool uses. It is copied into the host bundle, and the
        /// frozen copy is passed to the bridge tool as <c>--cecil</c>.
        /// </summary>
        public string CecilAssembly { get; set; } = string.Empty;
        /// <summary>
        /// Directory of the runtime pack resolved for the target runtime. Its <c>data/RuntimeList.xml</c> is
        /// added to the inputs as a <c>configuration</c> item, and the directory is passed to the bridge tool as
        /// <c>--runtime-pack</c>.
        /// </summary>
        public string RuntimePackDirectory { get; set; } = string.Empty;
        /// <summary>
        /// Full name of the application assembly the bridge runs for. It is passed to the bridge tool as
        /// <c>--application</c> and compared with the <c>applicationIdentity</c> the bridge reports; a mismatch
        /// fails the task.
        /// </summary>
        public string ApplicationAssemblyIdentity { get; set; } = string.Empty;
        /// <summary>
        /// Path of the .NET host executable that starts the bridge tool process. The tool is started from the
        /// frozen copy of the host bundle with the command <c>bridge</c>.
        /// </summary>
        public string DotNetExecutable { get; set; } = string.Empty;
        /// <summary>
        /// When <see langword="true" />, only the probe runs: <see cref="NeedsBridge" /> reports its result,
        /// <see cref="PreparedAssemblies" />, <see cref="PreparedAdditionalInputs" /> and
        /// <see cref="PreparedEngineArguments" /> pass the inputs through unchanged, nothing is written under
        /// <see cref="OutputRoot" /> and the bridge tool is not started.
        /// </summary>
        public bool ProbeOnly { get; set; }
        /// <summary>
        /// Whether the engine must run on the bridge result instead of the original inputs. It starts as the
        /// result of probing <see cref="Assemblies" />. When the bridge tool ran, the stage it reports decides:
        /// <c>transformed-input</c> means <see langword="true" /> and <c>unchanged-input</c> means
        /// <see langword="false" />.
        /// </summary>
        [Output] public bool NeedsBridge { get; set; }
        /// <summary>
        /// Path of the input list that names the frozen inputs. The bridge tool reads it as <c>--input</c> and
        /// has to report its hash as <c>originalInputHash</c>. Unlike the other outputs it is not cleared when
        /// the task fails.
        /// </summary>
        [Output] public string InputsPath { get; set; } = string.Empty;
        /// <summary>
        /// Path of the <c>transformation.json</c> the bridge tool wrote. It records the stage the bridge reached
        /// and each assembly it transformed. It is empty when the bridge tool did not run or the task failed.
        /// </summary>
        [Output] public string TransformationPath { get; set; } = string.Empty;
        /// <summary>
        /// The assemblies to use instead of <see cref="Assemblies" />. They are unchanged when
        /// <see cref="NeedsBridge" /> is <see langword="false" />; otherwise every item points at its frozen
        /// copy, or at the transformed assembly when the bridge replaced it. It is empty when the task fails.
        /// </summary>
        [Output] public ITaskItem[] PreparedAssemblies { get; set; } = Array.Empty<ITaskItem>();
        /// <summary>
        /// The additional inputs to use instead of <see cref="AdditionalInputs" />. They are unchanged when
        /// <see cref="NeedsBridge" /> is <see langword="false" />; otherwise they are the frozen copies, which
        /// also include the bridge host bundle files and the runtime pack manifest. It is empty when the task
        /// fails.
        /// </summary>
        [Output] public ITaskItem[] PreparedAdditionalInputs { get; set; } = Array.Empty<ITaskItem>();
        /// <summary>
        /// The engine arguments to use instead of <see cref="EngineArguments" />. They are unchanged when
        /// <see cref="NeedsBridge" /> is <see langword="false" />; otherwise they are followed by one quoted
        /// <c>--substitutions</c> argument per frozen substitution input. It is empty when the task fails.
        /// </summary>
        [Output] public string PreparedEngineArguments { get; set; } = string.Empty;
    }
    /// <summary>
    /// Verifies the frozen inputs and the transformation for one boundary of the build, and carries the bridge
    /// receipt from one phase to the next.
    /// </summary>
    public sealed class VerifyConditionalRegistrationBridgeTask : IsolatedBuildTask
    {
        /// <summary>
        /// Path of the frozen inputs document that lists the assemblies and additional inputs with their hashes.
        /// Its hash must equal the transformation's <c>originalInputHash</c>, and the receipt records it.
        /// </summary>
        public string InputsPath { get; set; } = string.Empty;
        /// <summary>
        /// Path of the transformation document that was written for these inputs. Every replacement it lists
        /// must match a frozen input assembly.
        /// </summary>
        public string TransformationPath { get; set; } = string.Empty;
        /// <summary>
        /// The assembly paths the SDK compiler actually consumed. In the <c>analysis-input</c> phase they must be
        /// exactly the prepared set; with the <c>wasm</c> engine they are the final compiler assemblies.
        /// </summary>
        public ITaskItem[] ConsumedAssemblies { get; set; } = Array.Empty<ITaskItem>();
        /// <summary>
        /// The directory that holds the compiler output; required in the <c>linked-output</c> and
        /// <c>published-output</c> phases. Every recorded file must lie under it.
        /// </summary>
        public string OutputDirectory { get; set; } = string.Empty;
        /// <summary>
        /// Path of the bridge receipt. The <c>analysis-input</c> phase writes it; each later phase reads the
        /// receipt of the preceding phase from this path and replaces it. A failed run deletes the file.
        /// </summary>
        public string ReceiptPath { get; set; } = string.Empty;
        /// <summary>
        /// The boundary to record. <c>analysis-input</c> checks what the SDK compiler consumed,
        /// <c>linked-output</c> checks the compiler output and <c>published-output</c> checks the published
        /// assets.
        /// </summary>
        public string Phase { get; set; } = string.Empty;
        /// <summary>
        /// The engine that produced the output: <c>illink</c>, <c>nativeaot</c> or <c>wasm</c>.
        /// </summary>
        public string EngineKind { get; set; } = string.Empty;
        /// <summary>
        /// The compiler output files to verify and record in the <c>linked-output</c> and
        /// <c>published-output</c> phases. When empty, every <c>.dll</c> and <c>.pdb</c> file under
        /// <see cref="OutputDirectory" /> is taken, plus every <c>.wasm</c> file for the <c>wasm</c> engine.
        /// </summary>
        public ITaskItem[] OutputFiles { get; set; } = Array.Empty<ITaskItem>();
        /// <summary>
        /// The non-assembly inputs the SDK compiler actually consumed. In the <c>analysis-input</c> phase they
        /// must equal the frozen additional inputs whose kind is <c>root-descriptor</c>, <c>reference</c>,
        /// <c>custom-step</c> or <c>opaque</c>; with the <c>wasm</c> engine they are the final native WASM
        /// asset.
        /// </summary>
        public ITaskItem[] ConsumedAdditionalInputs { get; set; } = Array.Empty<ITaskItem>();
    }
    /// <summary>
    /// Verifies the conditional registration output of a link run, or that the SDK publish consumed the
    /// verified output recorded in an existing receipt.
    /// </summary>
    public sealed class VerifyConditionalRegistrationOutputTask : IsolatedBuildTask
    {
        /// <summary>
        /// Path of the conditional registration input snapshot. The analysis report to verify with is read from
        /// the <c>analysisReportPath</c> it names.
        /// </summary>
        public string InputsPath { get; set; } = string.Empty;
        /// <summary>
        /// The directory that holds the linker output to verify. With <see cref="VerifyConsumed" /> it is the SDK
        /// publish directory, which must hold every output recorded in the receipt.
        /// </summary>
        public string OutputDirectory { get; set; } = string.Empty;
        /// <summary>
        /// Path of the verification receipt. The default mode writes a receipt for the verified outputs there;
        /// with <see cref="VerifyConsumed" /> the existing receipt is checked and rewritten as consumed.
        /// </summary>
        public string ReceiptPath { get; set; } = string.Empty;
        /// <summary>
        /// Path of the linker assembly whose content hash is recorded with the receipt and checked against it.
        /// </summary>
        public string LinkerAssembly { get; set; } = string.Empty;
        /// <summary>
        /// When <see langword="true" />, checks that every output recorded in the existing receipt is present,
        /// with its recorded hash, under <see cref="OutputDirectory" />, and marks the receipt as consumed. When
        /// <see langword="false" />, verifies the linker output and writes a new receipt.
        /// </summary>
        public bool VerifyConsumed { get; set; }
    }
    /// <summary>
    /// Freezes the SDK-selected official ILLink 8 tools with an explicit, isolated .NET 10 or later host
    /// configuration.
    /// </summary>
    public sealed class PrepareNet8LinkHostTask : IsolatedBuildTask
    {
        /// <summary>
        /// Path of the official <c>ILLink.Tasks.dll</c>. Its directory is the tool directory:
        /// <c>illink.dll</c> must sit beside it, and both must be the official ILLink 8.x tasks and engine from
        /// one servicing release.
        /// </summary>
        public string LinkerTaskAssembly { get; set; } = string.Empty;
        /// <summary>
        /// Path of the <c>AtomUI.Registration.ILLink8.dll</c> adapter, which must declare the ILLink8 linker
        /// capability. It is frozen into the host as <c>adapter/AtomUI.Registration.ILLink8.dll</c>.
        /// </summary>
        public string BackendAssembly { get; set; } = string.Empty;
        /// <summary>
        /// Root directory for the frozen host. The host is materialized in the subdirectory named by
        /// <see cref="HostFingerprint" />.
        /// </summary>
        public string OutputRoot { get; set; } = string.Empty;
        /// <summary>
        /// Path of the template file that is frozen into the host as <c>AtomUI.Net8.ILLink.Task.cs</c>. It is
        /// read only when <see cref="OverrideSourceText" /> is empty.
        /// </summary>
        public string OverrideSourceTemplate { get; set; } = string.Empty;
        /// <summary>
        /// Text that replaces the <see cref="OverrideSourceTemplate" /> file as the content of the frozen
        /// <c>AtomUI.Net8.ILLink.Task.cs</c>. Empty means read the template file.
        /// </summary>
        public string OverrideSourceText { get; set; } = string.Empty;
        /// <summary>
        /// Path of the frozen <c>illink.dll</c> under <see cref="OutputRoot" />. It is empty when the task
        /// failed.
        /// </summary>
        [Output] public string LinkerPath { get; set; } = string.Empty;
        /// <summary>
        /// Path of the frozen <c>AtomUI.Net8.ILLink.Task.cs</c> file. It is empty when the task failed.
        /// </summary>
        [Output] public string TaskOverrideSource { get; set; } = string.Empty;
        /// <summary>
        /// SHA-256 hash of the host state, which is the linker version and the hash of every frozen file; it
        /// names the host directory under <see cref="OutputRoot" />. It is empty when the task failed.
        /// </summary>
        [Output] public string HostFingerprint { get; set; } = string.Empty;
        /// <summary>
        /// The frozen files as items, each with <c>FullPath</c> set, followed by <c>host-state.json</c>. It is
        /// empty when the task failed.
        /// </summary>
        [Output] public ITaskItem[] HostInputs { get; set; } = Array.Empty<ITaskItem>();
    }
    /// <summary>
    /// Freezes the final SDK implementation inputs in a snapshot and returns the paths that the engine must consume.
    /// </summary>
    public sealed class PrepareConditionalRegistrationInputsTask : IsolatedBuildTask
    {
        /// <summary>
        /// Directory under which the input snapshot is created. Every later task must read the snapshot paths this
        /// task returns instead of the original input paths.
        /// </summary>
        public string OutputRoot { get; set; } = string.Empty;
        /// <summary>
        /// Configuration of this build. The snapshot records it together with <see cref="EngineArguments" /> and the
        /// item metadata of the inputs.
        /// </summary>
        public string Configuration { get; set; } = string.Empty;
        /// <summary>
        /// The linker arguments the SDK produced. Raw arguments that name files (response files, descriptors,
        /// references, custom steps, custom data or substitutions) are rejected as mutable; such inputs must come in
        /// as <see cref="Assemblies" /> or <see cref="AdditionalInputs" /> items.
        /// </summary>
        public string EngineArguments { get; set; } = string.Empty;
        /// <summary>
        /// The implementation assemblies to freeze, each item spec a managed assembly path. Exactly one
        /// <c>AtomUI.Core</c> identity must be present. The item metadata is recorded with the snapshot and kept on
        /// <see cref="PreparedAssemblies" />.
        /// </summary>
        public ITaskItem[] Assemblies { get; set; } = Array.Empty<ITaskItem>();
        /// <summary>
        /// Inputs other than assemblies. The <c>Kind</c> metadata names the role, for example <c>tool</c>,
        /// <c>custom-step</c> or <c>opaque</c>; a missing or blank kind means <c>configuration</c>. An <c>opaque</c>
        /// item must declare <c>CustomDataKey</c> and <c>Value</c>, with the value being its own path. An item whose
        /// <c>EngineOption</c> is <c>link-attributes</c> must appear in <see cref="EngineArguments" /> as
        /// <c>--link-attributes</c> arguments exactly once.
        /// </summary>
        public ITaskItem[] AdditionalInputs { get; set; } = Array.Empty<ITaskItem>();
        /// <summary>
        /// Path of the input list the engine must read. It is written into a fresh run directory under the snapshot
        /// and names the frozen assemblies, the frozen additional inputs and the analysis report the engine has to
        /// write. It is empty on failure.
        /// </summary>
        [Output] public string InputsPath { get; set; } = string.Empty;
        /// <summary>
        /// Name of the snapshot directory under <see cref="OutputRoot" /> that holds the frozen inputs. It
        /// identifies the frozen input set. It is empty on failure.
        /// </summary>
        [Output] public string InputFingerprint { get; set; } = string.Empty;
        /// <summary>
        /// <see cref="EngineArguments" /> with the <c>link-attributes</c> arguments the SDK produced removed and
        /// re-created from the frozen link-attributes items, so the linker reads only frozen copies. It is empty on
        /// failure.
        /// </summary>
        [Output] public string PreparedEngineArguments { get; set; } = string.Empty;
        /// <summary>
        /// <see cref="Assemblies" /> with each item pointing at its frozen copy: item spec and full path are the
        /// snapshot path, and the <c>AtomUIOriginalPath</c> metadata keeps the original path. It is empty on failure.
        /// </summary>
        [Output] public ITaskItem[] PreparedAssemblies { get; set; } = Array.Empty<ITaskItem>();
        /// <summary>
        /// <see cref="AdditionalInputs" /> with each item pointing at its frozen copy. The
        /// <c>AtomUIOriginalPath</c> and <c>AtomUIOriginalItemSpec</c> metadata keep the original location;
        /// <c>Kind</c> is set explicitly. It is empty on failure.
        /// </summary>
        [Output] public ITaskItem[] PreparedAdditionalInputs { get; set; } = Array.Empty<ITaskItem>();
    }
    /// <summary>
    /// Checks that the target framework, the ILLink binary and the NativeAOT compiler are the official ones that
    /// support the TypeMap contract.
    /// </summary>
    public sealed class ValidateRegistrationToolchainTask : IsolatedBuildTask
    {
        /// <summary>
        /// Identifier of the target framework, such as <c>.NETCoreApp</c>. Together with
        /// <see cref="TargetFrameworkVersion" /> it must classify as the official TypeMap framework.
        /// </summary>
        public string TargetFrameworkIdentifier { get; set; } = string.Empty;
        /// <summary>
        /// Version of the target framework, such as <c>v10.0</c>. Its major and minor version are the baseline for
        /// the linker and compiler checks.
        /// </summary>
        public string TargetFrameworkVersion { get; set; } = string.Empty;
        /// <summary>
        /// Path of the ILLink binary. When set, it must be the official <c>illink</c> with the Microsoft key, at a
        /// version that has the major of the target framework and is at least the target framework version and the
        /// minimum TypeMap version. When empty, the linker is not checked.
        /// </summary>
        public string LinkerAssembly { get; set; } = string.Empty;
        /// <summary>
        /// Path of the NativeAOT compiler. When set, the task starts it with <c>--version</c> and requires a version
        /// that has the major of the target framework and is at least the target framework version and the minimum
        /// TypeMap version. When empty, the compiler is not checked.
        /// </summary>
        public string NativeCompiler { get; set; } = string.Empty;
    }
    /// <summary>
    /// Freezes the inputs of a TypeMap link in a signature file and clears the previous link result when the
    /// inputs changed. It does not run the link.
    /// </summary>
    public sealed class PrepareTypeMapLinkTask : IsolatedBuildTask
    {
        /// <summary>
        /// SDK version string that older build assets still pass. The task does not read it.
        /// </summary>
        public string SdkVersion { get; set; } = string.Empty;
        /// <summary>
        /// Path of the WebAssembly SDK tasks assembly. When set, the file is hashed into the signature as a link
        /// input, so a change to it discards the previous link result. An empty value adds no file.
        /// </summary>
        public string WasmSdkTasksPath { get; set; } = string.Empty;
        /// <summary>
        /// Path of the ILLink backend adapter assembly. Its directory locates the backend files, and its full path,
        /// identity and hash are recorded in the signature.
        /// </summary>
        public string BackendAssembly { get; set; } = string.Empty;
        /// <summary>
        /// Path of the ILLink executable (<c>illink</c>). It must be an official ILLink binary of the minimum
        /// version with TypeMap support. Its directory also supplies the linker support files that are hashed into
        /// the signature.
        /// </summary>
        public string LinkerAssembly { get; set; } = string.Empty;
        /// <summary>
        /// Capability string of the TypeMap contract. The task fails unless it equals the capability of the TypeMap
        /// build contract, and the receipt must report it.
        /// </summary>
        public string Capability { get; set; } = string.Empty;
        /// <summary>
        /// Absolute path of the receipt that the TypeMap converter writes when it completes. When the inputs changed
        /// or the receipt is missing, the task deletes the receipt and its <c>.binding</c> file.
        /// </summary>
        public string ReceiptPath { get; set; } = string.Empty;
        /// <summary>
        /// Path of the JSON file that lists the link inputs with their hashes. The task rewrites it when its content
        /// differs from the current inputs.
        /// </summary>
        public string SignaturePath { get; set; } = string.Empty;
        /// <summary>
        /// Path of a semaphore file that this task never creates. It is deleted when the inputs changed or the
        /// receipt is missing.
        /// </summary>
        public string LinkSemaphore { get; set; } = string.Empty;
        /// <summary>
        /// Directory that receives the linked assemblies. This task only records its full path in the signature.
        /// <see cref="VerifyTypeMapReceiptTask" /> looks there for the linked output of each assembly it finds in
        /// the receipt, under the same file name.
        /// </summary>
        public string OutputDirectory { get; set; } = string.Empty;
        /// <summary>
        /// Build configuration. This task only records it in the signature.
        /// </summary>
        public string Configuration { get; set; } = string.Empty;
        /// <summary>
        /// Link inputs other than assemblies, each with a <c>Kind</c> metadata value that names its role.
        /// <c>Reference</c> items are reference assemblies; one whose full identity equals an assembly in
        /// <see cref="Assemblies" /> is dropped from the hashed files. <c>Tool</c> items are passed with
        /// <see cref="LinkerAssembly" /> to the link configuration. Items of any other kind are only hashed.
        /// </summary>
        public ITaskItem[] Inputs { get; set; } = Array.Empty<ITaskItem>();
        /// <summary>
        /// Assemblies to link. The path, full identity, hash and the <c>TrimMode</c> and <c>IsTrimmable</c>
        /// metadata of each item are recorded in the signature, sorted by identity.
        /// </summary>
        public ITaskItem[] Assemblies { get; set; } = Array.Empty<ITaskItem>();
        /// <summary>
        /// Scalar ILLink options. The item spec is the option name and the <c>Value</c> metadata is its value. The
        /// names must be unique or the task fails. The value of <c>NoWarn</c> is reduced to its IL warning codes.
        /// </summary>
        public ITaskItem[] LinkOptions { get; set; } = Array.Empty<ITaskItem>();
        /// <summary>
        /// Items of the ILLink input collections, in the order ILLink receives them. The <c>Kind</c> metadata names
        /// the collection: <c>RootAssemblyNames</c>, <c>FeatureSettings</c>, <c>CustomData</c>,
        /// <c>CustomSteps</c>, <c>RootDescriptorFiles</c> or <c>KeepMetadata</c>. Any other kind fails the task.
        /// </summary>
        public ITaskItem[] LinkItems { get; set; } = Array.Empty<ITaskItem>();
    }
    /// <summary>
    /// Checks that the TypeMap receipt belongs to the current link inputs and that the linked outputs were not
    /// replaced afterwards, then binds the receipt to the signature or verifies the existing binding.
    /// </summary>
    public sealed class VerifyTypeMapReceiptTask : IsolatedBuildTask
    {
        /// <summary>
        /// Path of the receipt that the TypeMap converter wrote. It must exist, and its path and capability must
        /// match the signature.
        /// </summary>
        public string ReceiptPath { get; set; } = string.Empty;
        /// <summary>
        /// Path of the signature written by <see cref="PrepareTypeMapLinkTask" />. It is the baseline for every
        /// check, and its hash together with the hash of the receipt forms the binding.
        /// </summary>
        public string SignaturePath { get; set; } = string.Empty;
        /// <summary>
        /// Controls how the <c>.binding</c> file next to the receipt is treated. When <see langword="true" />, the
        /// task writes it from the hashes of the signature and the receipt. When <see langword="false" />, the file
        /// must already exist with that content, otherwise the task fails.
        /// </summary>
        public bool BindReceipt { get; set; }
        /// <summary>
        /// When <see langword="true" />, the task fails when <see cref="ConsumedAssemblies" /> is empty.
        /// </summary>
        public bool RequireConsumedAssemblies { get; set; }
        /// <summary>
        /// Assembly files that the AOT or bundle step takes as its input. When not empty, every output listed in the
        /// receipt must match exactly one item with the same file name and the recorded output hash.
        /// </summary>
        public ITaskItem[] ConsumedAssemblies { get; set; } = Array.Empty<ITaskItem>();
    }
    /// <summary>
    /// Resolves complete build-tool bundles by identity and content, before analyzer and backend injection.
    /// </summary>
    public sealed class ResolveRegistrationToolsTask : IsolatedBuildTask
    {
        /// <summary>
        /// The tool assemblies to choose from. Each item's path is the main assembly of a bundle and its
        /// <c>Kind</c> metadata is <c>Generator</c>, <c>Backend</c>, <c>Net8Backend</c> or <c>Tasks</c>. All
        /// candidates of a kind must be the same bundle; the first one in ordinal path order is selected.
        /// </summary>
        public ITaskItem[] Candidates { get; set; } = Array.Empty<ITaskItem>();
        /// <summary>
        /// <see langword="true" /> makes the <c>Backend</c> kind required and resolves it: the Mono TypeMap backend
        /// (<c>AtomUI.TypeMap.Linker.dll</c>). While it is <see langword="false" />, <c>Backend</c> candidates are
        /// ignored. The task fails with <c>ATOMUIREG006</c> when it is <see langword="true" /> and no
        /// <c>Backend</c> candidate remains.
        /// </summary>
        public bool IncludeBackend { get; set; }
        /// <summary>
        /// <see langword="true" /> makes the <c>Net8Backend</c> kind required and resolves it: the .NET 8
        /// conditional registration backend (<c>AtomUI.Registration.ILLink8.dll</c>). It follows
        /// <see cref="IncludeBackend" /> in every other respect.
        /// </summary>
        public bool IncludeNet8Backend { get; set; }
        /// <summary>
        /// Path of the selected <c>AtomUI.Generator.dll</c>, or empty when no <c>Generator</c> candidate was given.
        /// </summary>
        [Output] public string GeneratorAssembly { get; set; } = string.Empty;
        /// <summary>
        /// Path of the selected <c>AtomUI.TypeMap.Linker.dll</c>, or empty when no <c>Backend</c> candidate was
        /// given.
        /// </summary>
        [Output] public string BackendAssembly { get; set; } = string.Empty;
        /// <summary>
        /// Path of the selected <c>AtomUI.Registration.ILLink8.dll</c>, or empty when no <c>Net8Backend</c>
        /// candidate was given.
        /// </summary>
        [Output] public string Net8BackendAssembly { get; set; } = string.Empty;
    }
    /// <summary>
    /// Converts theme definition documents into the encoded <c>CompiledThemeInput</c> intermediates for the compiled
    /// theme generator and reports document diagnostics to the build log.
    /// </summary>
    public sealed class GenerateCompiledThemeInputTask : IsolatedBuildTask
    {
        /// <summary>
        /// Path of the theme document to compile when <see cref="ThemeInputs" /> is empty. With item inputs it only
        /// locates the error for an input that lacks a path, an identity or an output path.
        /// </summary>
        public string SourcePath { get; set; } = string.Empty;
        /// <summary>
        /// Logical identity of the theme document when <see cref="ThemeInputs" /> is empty, such as the assembly name
        /// followed by the document's logical path.
        /// </summary>
        public string SourceIdentity { get; set; } = string.Empty;
        /// <summary>
        /// Path of the intermediate to write when <see cref="ThemeInputs" /> is empty. The file is replaced only when
        /// its content differs, and it is deleted when the input fails.
        /// </summary>
        public string OutputPath { get; set; } = string.Empty;
        /// <summary>
        /// Theme documents to compile in one run. When it is empty, the task compiles the single document that
        /// <see cref="SourcePath" />, <see cref="SourceIdentity" /> and <see cref="OutputPath" /> describe instead.
        /// An item is read from its full path; its <c>SourceIdentity</c> and <c>InputPath</c> metadata give the
        /// identity and the intermediate to write.
        /// </summary>
        public ITaskItem[] ThemeInputs { get; set; } = Array.Empty<ITaskItem>();
    }
    /// <summary>
    /// Rewrites every v1 <c>*.theme.xml</c> file of a directory as v2.
    /// </summary>
    public sealed class ConvertThemeDefinitionsTask : IsolatedBuildTask
    {
        /// <summary>
        /// The directory to read the v1 <c>*.theme.xml</c> files from. Subdirectories are not searched.
        /// </summary>
        [Required] public string InputDirectory { get; set; } = string.Empty;
        /// <summary>
        /// The directory the converted files are written to. When it is blank, the sources are replaced in
        /// <see cref="InputDirectory" /> and the variant files that were merged into another file are deleted.
        /// </summary>
        public string OutputDirectory { get; set; } = string.Empty;
        /// <summary>
        /// The v2 files the task wrote, one item per output file.
        /// </summary>
        [Output] public ITaskItem[] ConvertedFiles { get; set; } = Array.Empty<ITaskItem>();
    }
    /// <summary>
    /// Generates a <c>ResourceDictionary</c> wrapper, with an internal code-behind class, for each theme asset of an
    /// assembly and writes the code-behind classes to one C# file.
    /// </summary>
    public sealed class GenerateThemeAssetWrappersTask : IsolatedBuildTask
    {
        /// <summary>
        /// The XAML assets to wrap. The logical resource path is the <c>Link</c> metadata, or the item spec when it is
        /// empty, and the file is read from the item's full path. Only a <c>.axaml</c> file in a <c>Themes</c>
        /// directory is processed, never one named <c>*Themes.axaml</c>, and only when its root is a
        /// <c>ResourceDictionary</c> or the default typed <c>ControlTheme</c> of a control.
        /// </summary>
        [Required] public ITaskItem[] ThemeAssets { get; set; } = new ITaskItem[0];
        /// <summary>
        /// Directory that receives the generated <c>.axaml</c> files. It is created when it does not exist.
        /// </summary>
        [Required] public string OutputDirectory { get; set; } = string.Empty;
        /// <summary>
        /// Name of the assembly that owns the assets. It forms the <c>avares://</c> URIs of the merged sources and the
        /// namespace of the generated classes.
        /// </summary>
        [Required] public string AssemblyName { get; set; } = string.Empty;
        /// <summary>
        /// Path of the C# file that declares an internal class per generated wrapper dictionary. The deferred
        /// dictionaries are merged only and get no class.
        /// </summary>
        [Required] public string GeneratedCodePath { get; set; } = string.Empty;
        /// <summary>
        /// The generated files: one wrapper per processed asset and, for a typed <c>ControlTheme</c>, its deferred
        /// dictionary. Each item's spec is the file path and its <c>Link</c> metadata is
        /// <c>AtomUI.Generated/&lt;class name&gt;.axaml</c>.
        /// </summary>
        [Output] public ITaskItem[] GeneratedAssets { get; set; } = new ITaskItem[0];
    }
    /// <summary>
    /// Exports language templates: for one target language it turns source XLIFF documents into
    /// <c>&lt;targetLanguage&gt;.xlf</c> files, merging into the template files that already exist.
    /// </summary>
    public sealed class ExportLanguageTemplatesTask : IsolatedBuildTask
    {
        /// <summary>
        /// The source-language XLIFF documents to export. Each must have no target language (<c>trgLang</c>). An
        /// item's <c>AtomUILanguageTemplateOutputPath</c> metadata overrides where its template is written.
        /// </summary>
        [Required] public ITaskItem[] SourceFiles { get; set; } = Array.Empty<ITaskItem>();
        /// <summary>
        /// The canonical BCP 47 tag of the language the templates are written for. It must already be in canonical
        /// form; the task fails with <c>ATOMUILOC005</c> otherwise.
        /// </summary>
        [Required] public string TargetLanguage { get; set; } = string.Empty;
        /// <summary>
        /// The root directory the templates are written to, each under the directory of its item's
        /// <c>AtomUILanguagePackagePath</c> metadata with a leading <c>Localization/</c> removed. When it is
        /// <see langword="null" /> or blank, each template is written next to its source.
        /// </summary>
        public string OutputRootDirectory { get; set; }
        /// <summary>
        /// The template files the task wrote. It lists the sources that succeeded even when the task fails.
        /// </summary>
        [Output] public ITaskItem[] ExportedFiles { get; set; } = Array.Empty<ITaskItem>();
    }
    /// <summary>
    /// Validates the translated XLIFF files of a language package, against the authoritative en-US sources of the
    /// module when they are available, and publishes the validated items for packaging.
    /// </summary>
    public sealed class PrepareLanguagePackageAssetsTask : IsolatedBuildTask
    {
        /// <summary>
        /// Id of the language package being built. It is written to the <c>AtomUILanguageSourceIdentity</c> metadata
        /// of each prepared file and names the package in diagnostics.
        /// </summary>
        [Required] public string PackageId { get; set; } = string.Empty;
        /// <summary>
        /// The translated XLIFF files of the package. Every item must carry the same non-empty
        /// <c>AtomUILanguageModuleId</c> metadata, which selects the module being translated.
        /// </summary>
        [Required] public ITaskItem[] LanguageFiles { get; set; } = Array.Empty<ITaskItem>();
        /// <summary>
        /// The authoritative en-US source XLIFF files. Only items whose <c>AtomUILanguageModuleId</c> metadata equals
        /// the module of <see cref="LanguageFiles" /> take part. With at least one match every translation is checked
        /// against its source; with none the entries are marked deferred and a warning is logged.
        /// </summary>
        public ITaskItem[] SourceLanguageFiles { get; set; } = Array.Empty<ITaskItem>();
        /// <summary>
        /// Language the package is expected to translate into, as a canonical BCP 47 tag. When set, the task fails
        /// unless the tag is canonical and equals the target language of <see cref="LanguageFiles" />.
        /// <see langword="null" /> or white space skips the check.
        /// </summary>
        public string ExpectedLanguage { get; set; }
        /// <summary>
        /// Lowest XLIFF target state a translation must have to pass validation, <c>final</c> by default. A target
        /// below it is logged as an error and fails the task.
        /// </summary>
        public string MinimumTargetState { get; set; } = "final";
        /// <summary>
        /// <see langword="true" /> fails the task when <see cref="SourceLanguageFiles" /> holds no file of the target
        /// module, so the package is never built without validation against the en-US contract. With
        /// <see langword="false" /> that case only logs a warning.
        /// </summary>
        public bool RequireVerifiedContract { get; set; }
        /// <summary>
        /// The <see cref="LanguageFiles" /> items, enriched with the package metadata. It is set only when the task
        /// succeeds and stays empty otherwise.
        /// </summary>
        [Output] public ITaskItem[] PreparedLanguageFiles { get; set; } = Array.Empty<ITaskItem>();
    }
    /// <summary>
    /// Checks that the files of a static language package contain no build logic, script, source code, runtime
    /// binary or <c>.atomlang</c> payload, then validates the language files and writes the package manifest.
    /// </summary>
    public sealed class PrepareLanguagePackageTask : IsolatedBuildTask
    {
        /// <summary>
        /// Identity of the language package. It is recorded in the manifest and used in validation messages.
        /// </summary>
        [Required] public string PackageId { get; set; } = string.Empty;
        /// <summary>
        /// The translated language files of the package. Their target language is recorded in the manifest and compared
        /// with <see cref="ExpectedLanguage" /> when that is set.
        /// </summary>
        [Required] public ITaskItem[] LanguageFiles { get; set; } = Array.Empty<ITaskItem>();
        /// <summary>
        /// The authoritative en-US source language files. Only items whose <c>AtomUILanguageModuleId</c> metadata
        /// equals the module of <see cref="LanguageFiles" /> take part. With at least one match every translation is
        /// checked against its source; with none the entries are marked deferred, and
        /// <see cref="RequireVerifiedContract" /> decides whether that is a warning or an error.
        /// </summary>
        public ITaskItem[] SourceLanguageFiles { get; set; } = Array.Empty<ITaskItem>();
        /// <summary>
        /// Files that will be packed into the package. Each is checked by the extension of the path it takes inside
        /// the package, which is its <c>PackagePath</c> metadata or else its item spec. Build logic, a script, source
        /// code, a runtime binary or an <c>.atomlang</c> payload fails the task before any language file is read.
        /// </summary>
        public ITaskItem[] PackageFiles { get; set; } = Array.Empty<ITaskItem>();
        /// <summary>
        /// Path of the language package manifest to write, which lists the package identity, the language and the
        /// catalog entries. It is written only after the package contents and the language files passed validation.
        /// </summary>
        [Required] public string OutputManifestPath { get; set; } = string.Empty;
        /// <summary>
        /// Language the package is expected to translate into, as a canonical BCP 47 tag. When set, the task fails
        /// unless the tag is canonical and equals the target language of <see cref="LanguageFiles" />.
        /// <see langword="null" /> or white space skips the check.
        /// </summary>
        public string ExpectedLanguage { get; set; }
        /// <summary>
        /// Lowest XLIFF target state a translation must have to pass validation, <c>final</c> by default. A
        /// translation below it fails the task.
        /// </summary>
        public string MinimumTargetState { get; set; } = "final";
        /// <summary>
        /// <see langword="true" /> fails the task when <see cref="SourceLanguageFiles" /> holds no file of the module
        /// that <see cref="LanguageFiles" /> translates, so the package is never built without validation against the
        /// en-US contract. With <see langword="false" /> that case only logs a warning.
        /// </summary>
        public bool RequireVerifiedContract { get; set; }
        /// <summary>
        /// The <see cref="LanguageFiles" /> items, enriched with the package metadata. It is set only when the task
        /// succeeds and stays empty otherwise.
        /// </summary>
        [Output] public ITaskItem[] PreparedLanguageFiles { get; set; } = Array.Empty<ITaskItem>();
    }
    /// <summary>
    /// Generates the MSBuild props file listing the catalog entries of a language package.
    /// </summary>
    public sealed class GenerateLanguagePackagePropsTask : IsolatedBuildTask
    {
        /// <summary>
        /// The ID of the language package the props file is generated for.
        /// </summary>
        [Required] public string PackageId { get; set; } = string.Empty;
        /// <summary>
        /// The XLIFF documents of the package, one catalog entry each. Every item needs the
        /// <c>AtomUILanguageModuleId</c> and <c>AtomUILanguagePackagePath</c> metadata;
        /// <c>AtomUILanguageContractValidation</c> is optional.
        /// </summary>
        [Required] public ITaskItem[] LanguageFiles { get; set; } = Array.Empty<ITaskItem>();
        /// <summary>
        /// The path of the props file to write. Its directory is created when it does not exist.
        /// </summary>
        [Required] public string OutputPath { get; set; } = string.Empty;
        /// <summary>
        /// When <see langword="true" />, every language file must be a target-language XLIFF document; when
        /// <see langword="false" />, every file must be an en-US document without <c>trgLang</c>.
        /// </summary>
        public bool RequireTargetLanguage { get; set; } = true;
        /// <summary>
        /// The source kind of the language files, <c>StaticLanguagePack</c> by default. <c>ModuleBuiltIn</c> makes
        /// an item without <c>AtomUILanguageContractValidation</c> metadata default to <c>Verified</c>.
        /// </summary>
        public string SourceKind { get; set; } = "StaticLanguagePack";
    }
}

#endif
