using DeploymentToolkit.Modals.Actions;

namespace DeploymentToolkit.Extension.INI.Actions
{
    public class IniWrite : IExecutableAction
    {
        public string Path { get; set; }
        public string Section { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }

        public bool Execute()
        {
            return IniManager.SetData(Path, Section, Key, Value);
        }
    }
}
