using System.Collections;
using System.Globalization;
using System.Reflection;
using AtomUI.Build.Tasks.Isolation;
using Microsoft.Build.Framework;

namespace AtomUI.Build.Tasks;

internal static class TaskProcessHost
{
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length != 1)
            {
                Console.Error.WriteLine("Usage: dotnet AtomUI.Build.Tasks.dll <request.json>");
                return 2;
            }

            var request = TaskWire.Read<TaskRequest>(File.ReadAllText(args[0]));
            var response = Execute(request);
            TaskWire.WriteResponse(response);
            // Task failure is carried by the structured response; nonzero exit codes
            // are reserved for host/protocol failures without a usable response.
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            return 1;
        }
    }

    internal static TaskResponse Execute(TaskRequest request)
    {
        // This executable is build-time infrastructure and is never shipped as
        // application code or trimmed. Keep dispatch explicit and bounded.
        ITask task = request.TaskName switch
        {
            nameof(PrepareConditionalRegistrationInputsTask) => new PrepareConditionalRegistrationInputsTask(),
            nameof(PrepareNet8LinkHostTask) => new PrepareNet8LinkHostTask(),
            nameof(PrepareNative8RegistrationTask) => new PrepareNative8RegistrationTask(),
            nameof(VerifyNative8RegistrationTask) => new VerifyNative8RegistrationTask(),
            nameof(PrepareConditionalRegistrationBridgeTask) => new PrepareConditionalRegistrationBridgeTask(),
            nameof(VerifyConditionalRegistrationBridgeTask) => new VerifyConditionalRegistrationBridgeTask(),
            nameof(ConditionalPublishTransformTask) => new ConditionalPublishTransformTask(),
            nameof(VerifyConditionalRegistrationOutputTask) => new VerifyConditionalRegistrationOutputTask(),
            nameof(ValidateRegistrationToolchainTask) => new ValidateRegistrationToolchainTask(),
            nameof(PrepareTypeMapLinkTask) => new PrepareTypeMapLinkTask(),
            nameof(VerifyTypeMapReceiptTask) => new VerifyTypeMapReceiptTask(),
            nameof(ResolveRegistrationToolsTask) => new ResolveRegistrationToolsTask(),
            nameof(GenerateThemeAssetWrappersTask) => new GenerateThemeAssetWrappersTask(),
            nameof(GenerateBuiltInThemeDocumentTask) => new GenerateBuiltInThemeDocumentTask(),
            nameof(ExportLanguageTemplatesTask) => new ExportLanguageTemplatesTask(),
            nameof(PrepareLanguagePackageAssetsTask) => new PrepareLanguagePackageAssetsTask(),
            nameof(PrepareLanguagePackageTask) => new PrepareLanguagePackageTask(),
            nameof(GenerateLanguagePackagePropsTask) => new GenerateLanguagePackagePropsTask(),
            _ => throw new ArgumentException($"Unknown AtomUI build task '{request.TaskName}'.")
        };
        var response = new TaskResponse();
        task.BuildEngine = new ProcessBuildEngine(request, response.Diagnostics);
        var type = task.GetType();
        foreach (var pair in request.Properties)
        {
            var property = GetInputProperty(type, pair.Key);
            if (property.PropertyType != typeof(string) && property.PropertyType != typeof(bool) &&
                property.PropertyType != typeof(int))
            {
                throw new ArgumentException($"Unsupported property '{pair.Key}'.");
            }
            property.SetValue(task, Convert.ChangeType(pair.Value, property.PropertyType, CultureInfo.InvariantCulture));
        }
        foreach (var pair in request.Items)
        {
            var property = GetInputProperty(type, pair.Key);
            if (property.PropertyType != typeof(ITaskItem[]))
            {
                throw new ArgumentException($"Unsupported item parameter '{pair.Key}'.");
            }

            property.SetValue(task, pair.Value.Cast<ITaskItem>().ToArray());
        }

        response.Success = task.Execute() && response.Diagnostics.All(d => d.Kind != "error");
        foreach (var property in type.GetProperties().Where(p => p.IsDefined(typeof(OutputAttribute))))
        {
            var value = property.GetValue(task);
            if (property.PropertyType == typeof(ITaskItem[]))
            {
                response.Items.Add(property.Name, ((ITaskItem[]?)value ?? []).Select(TaskWireItem.FromItem).ToArray());
            }
            else
            {
                response.Properties.Add(property.Name, Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty);
            }
        }
        return response;
    }

    private static PropertyInfo GetInputProperty(Type type, string name)
    {
        var property = type.GetProperty(name);
        if (property?.SetMethod?.IsPublic != true || property.IsDefined(typeof(OutputAttribute)) ||
            name is nameof(ITask.BuildEngine) or nameof(ITask.HostObject))
        {
            throw new ArgumentException($"Unknown task input '{type.Name}.{name}'.");
        }

        return property;
    }

    private sealed class ProcessBuildEngine(TaskRequest request, List<TaskDiagnostic> diagnostics) : IBuildEngine
    {
        public bool ContinueOnError => false;
        public int LineNumberOfTaskNode => request.Line;
        public int ColumnNumberOfTaskNode => request.Column;
        public string ProjectFileOfTaskNode => request.ProjectFile;
        public void LogErrorEvent(BuildErrorEventArgs e) => diagnostics.Add(new TaskDiagnostic
        {
            Kind = "error", Message = e.Message, Code = e.Code, Subcategory = e.Subcategory,
            HelpKeyword = e.HelpKeyword, File = e.File, Line = e.LineNumber, Column = e.ColumnNumber,
            EndLine = e.EndLineNumber, EndColumn = e.EndColumnNumber
        });
        public void LogWarningEvent(BuildWarningEventArgs e) => diagnostics.Add(new TaskDiagnostic
        {
            Kind = "warning", Message = e.Message, Code = e.Code, Subcategory = e.Subcategory,
            HelpKeyword = e.HelpKeyword, File = e.File, Line = e.LineNumber, Column = e.ColumnNumber,
            EndLine = e.EndLineNumber, EndColumn = e.EndColumnNumber
        });
        public void LogMessageEvent(BuildMessageEventArgs e) => diagnostics.Add(new TaskDiagnostic
        {
            Kind = "message", Message = e.Message, Importance = (int)e.Importance
        });
        public void LogCustomEvent(CustomBuildEventArgs e) => diagnostics.Add(new TaskDiagnostic
        {
            Kind = "message", Message = e.Message, Importance = (int)MessageImportance.Normal
        });
        public bool BuildProjectFile(string projectFileName, string[] targetNames,
            IDictionary globalProperties, IDictionary targetOutputs) =>
            throw new NotSupportedException("AtomUI task workers cannot schedule nested builds.");
    }
}
