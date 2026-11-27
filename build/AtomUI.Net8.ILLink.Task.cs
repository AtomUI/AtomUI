// Compiled inside the calling MSBuild project. Inherit the SDK's complete task surface rather
// than copying its target or reconstructing its response file/options in a separate launcher.
using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;

namespace AtomUI.ControlledNet8
{
    /// <summary>
    /// ILLink task of the controlled .NET 8 link. It starts the official ILLink 8.x engine through
    /// <c>dotnet exec</c> with the prepared .NET 10 runtime configuration and a major roll-forward, so that
    /// the engine runs on .NET 10 or later.
    /// </summary>
    public sealed class ILLink : global::ILLink.Tasks.ILLink
    {
        private string _runtimeConfig;

        /// <summary>
        /// Takes the prepared linker and runtime configuration from the <c>CustomData</c> items
        /// <c>AtomUIControlledLinkerPath</c> and <c>AtomUIControlledRuntimeConfig</c>, whose <c>Value</c>
        /// metadata holds the path, removes both items from <c>CustomData</c> and runs the SDK task. Each item
        /// must be present exactly once and its path must be absolute and exist.
        /// </summary>
        /// <returns>
        /// <see langword="true" /> when the SDK task succeeded. <see langword="false" /> when an item is
        /// missing or its path is invalid, which is logged as error <c>ATOMUIREG006</c>, or when the SDK task
        /// failed.
        /// </returns>
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

        /// <summary>
        /// Builds the command line of the .NET host process that starts the linker. <c>exec --roll-forward Major</c>
        /// and the quoted path of the prepared runtime configuration come before the arguments that the SDK task
        /// generates.
        /// </summary>
        /// <returns>
        /// The command line for the .NET host.
        /// </returns>
        protected override string GenerateCommandLineCommands()
        {
            // Command-line roll-forward has priority over environment settings. Major rolls forward only
            // to .NET 10 or later, never down to an installed .NET 8 runtime.
            return "exec --roll-forward Major --runtimeconfig " + Quote(_runtimeConfig) + " " + base.GenerateCommandLineCommands();
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
