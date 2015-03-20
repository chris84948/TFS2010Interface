using Microsoft.VisualStudio.Shell;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace chrisbjohnson.TFS2010Interface
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    class ToolsOptions : DialogPage
    {
        private string _tfsServer = @"http://panther:8080/";
        private string _tfsWorkspace;
        private string _tfsPath;

        [Category("General")]
        [DisplayName("TFS Server Address")]
        [Description("Address of TFS 2010 Server")]
        [DefaultValue(@"http://panther:8080/")]
        public string TFSServer
        {
            get { return _tfsServer; }
            set { _tfsServer = value; }
        }

        [Category("General")]
        [DisplayName("TFS Workspace")]
        [Description("Workspace Name of Default Workspace")]
        public string TFSWorkspace
        {
            get { return _tfsWorkspace; }
            set { _tfsWorkspace = value; }
        }

        [Category("General")]
        [DisplayName("File Path")]
        [Description(@"Path To Read Files e.g. ""\ILT\SW\Projects-Dev""")]
        public string TFSPath
        {
            get { return _tfsPath; }
            set { _tfsPath = value; }
        }

        [Category("General")]
        [DisplayName("File Filter")]
        [Description(@"Regex used to filter files. File paths matching this filter will be ignored. e.g. ""\\bin"" ignores bin folder.")]
        public string FileFilter { get; set; }

    }
}
