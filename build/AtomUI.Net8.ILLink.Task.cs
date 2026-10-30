// Compiled inside the calling MSBuild project. Inherit the SDK's complete task surface rather
// than copying its target or reconstructing its response file/options in a separate launcher.
using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;

namespace AtomUI.ControlledNet8
{
    public sealed class ILLink : global::ILLink.Tasks.ILLink
    {
        private string _runtimeConfig;

        public override bool Execute()
        {
            try
            {
                var data = CustomData ?? new ITaskItem[0];
                var linker = data.Where(item => item.ItemSpec == "AtomUIControlledLinkerPath").ToArray();
                var config = data.Where(item => item.ItemSpec == "AtomUIControlledRuntimeConfig").ToArray();
                if (linker.Length != 1 || config.Length != 1)
                {
                    throw new InvalidOperationException("Controlled ILLink8 requires exactly one prepared linker and runtimeconfig.");
                }
                ILLinkPath = linker[0].GetMetadata("Value");
                _runtimeConfig = config[0].GetMetadata("Value");
                if (!Path.IsPathRooted(ILLinkPath) || !Path.IsPathRooted(_runtimeConfig) ||
                    !File.Exists(ILLinkPath) || !File.Exists(_runtimeConfig))
                {
                    throw new InvalidOperationException("Prepared ILLink8 host files are missing or not absolute.");
                }
                CustomData = data.Where(item => item.ItemSpec != "AtomUIControlledLinkerPath" &&
                    item.ItemSpec != "AtomUIControlledRuntimeConfig").ToArray();
                Log.LogMessage(MessageImportance.High, "AtomUI ILLink8 uses explicit .NET 10 runtimeconfig: {0}", _runtimeConfig);
                return base.Execute();
            }
            catch (Exception error)
            {
                Log.LogError("ATOMUIREG006: {0}", error.Message);
                return false;
            }
        }

        protected override string GenerateCommandLineCommands()
        {
            // Command-line roll-forward has priority over environment settings. It permits only
            // .NET 10.0 servicing patches, even when a matching .NET 8 runtime is also installed.
            return "exec --roll-forward LatestPatch --runtimeconfig " + Quote(_runtimeConfig) + " " + base.GenerateCommandLineCommands();
        }

        private static string Quote(string value)
        {
            if (value.IndexOf('"') >= 0 || value.IndexOf('\r') >= 0 || value.IndexOf('\n') >= 0)
            {
                throw new InvalidOperationException("Unsupported quote or line break in controlled host path.");
            }
            return "\"" + value + "\"";
        }
    }
}
