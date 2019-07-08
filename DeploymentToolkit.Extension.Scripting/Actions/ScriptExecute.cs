using DeploymentToolkit.Extension.Scripting.Modals;
using DeploymentToolkit.Modals.Actions;

namespace DeploymentToolkit.Extension.Scripting.Actions
{
    public class ScriptExecute : IExecutableAction
    {
        public ScriptType ScriptType { get; set; }
        public LaunchType LaunchType { get; set; }
        public string ScriptPath { get; set; }
        public bool WaitForExit { get; set; }

        public bool Execute()
        {
            return ScriptManager.StartScript(this);
        }
    }
}
