using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;
using System.Linq;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using EnvDTE;

namespace chrisbjohnson.TFS2010Interface
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    ///
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
    /// usually implemented by the package implementer.
    ///
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its 
    /// implementation of the IVsUIElementPane interface.
    /// </summary>
    [Guid("e4af25bb-0a65-4658-88f1-2dd17953d2ca")]
    public class ToolWindow : ToolWindowPane
    {
        private TFSUserControl control;

        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public ToolWindow() :
            base(null)
        {
            this.ToolBar = new CommandID(GuidList.guidTFS2010InterfaceCmdSet, PkgCmdIDList.TFS2010InterfaceToolbar);

            // Set the window title reading it from the resources.
            this.Caption = Resources.ToolWindowTitle;
            // Set the image that will appear on the tab of the window frame
            // when docked with an other window
            // The resource ID correspond to the one defined in the resx file
            // while the Index is the offset in the bitmap strip. Each image in
            // the strip being 16x16.
            this.BitmapResourceID = 301;
            this.BitmapIndex = 1;


            // Add all the commands to the toolbar in the tool window
            OleMenuCommandService oOleMenuCommandService = (OleMenuCommandService)GetService(typeof(IMenuCommandService));

            if (oOleMenuCommandService != null)
            {
                CommandID refreshID = new CommandID(GuidList.guidTFS2010InterfaceCmdSet, (int)PkgCmdIDList.cmdidRefreshButton);
                CommandID solutionSearchID = new CommandID(GuidList.guidTFS2010InterfaceCmdSet, (int)PkgCmdIDList.cmdidSolutionSearchButton);
                CommandID checkedOutSearchID = new CommandID(GuidList.guidTFS2010InterfaceCmdSet, (int)PkgCmdIDList.cmdidCheckedOutSearchButton);

                OleMenuCommand refreshMenuItem = new OleMenuCommand(RefreshButtonCallback, refreshID);
                OleMenuCommand solutionMenuItem = new OleMenuCommand(SolutionSearchButtonCallback, solutionSearchID);
                OleMenuCommand checkedOutMenuItem = new OleMenuCommand(CheckedOutSearchButtonCallback, checkedOutSearchID);

                oOleMenuCommandService.AddCommand(refreshMenuItem);
                oOleMenuCommandService.AddCommand(solutionMenuItem);
                oOleMenuCommandService.AddCommand(checkedOutMenuItem);
            }
            
            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on 
            // the object returned by the Content property.
            control = new TFSUserControl();
            base.Content = control;
        }

        /// <summary>
        /// Called when the solution refresh button is clicked
        /// </summary>
        private void RefreshButtonCallback(object sender, EventArgs e)
        {
            control.ViewModel.RefreshAllFiles();
        }

        /// <summary>
        /// Called when the solution search button is clicked - toggles on/off
        /// </summary>
        private void SolutionSearchButtonCallback(object sender, EventArgs e)
        {
            OleMenuCommand myCommand = sender as OleMenuCommand;
            myCommand.Checked = !myCommand.Checked;

            control.ViewModel.SearchSolution = myCommand.Checked;
        }

        /// <summary>
        /// Called when the checked out search button is clicked - toggles on/off
        /// </summary>
        private void CheckedOutSearchButtonCallback(object sender, EventArgs e)
        {
            OleMenuCommand myCommand = sender as OleMenuCommand;
            myCommand.Checked = !myCommand.Checked;

            control.ViewModel.SearchCheckedOut = myCommand.Checked;
        }
        
        #region Search Stuff
        
        public override bool SearchEnabled
        {
            get
            {
                return true;
            }
        }

        public override IVsSearchTask CreateSearch(uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchCallback pSearchCallback)
        {
            if (pSearchQuery == null || pSearchCallback == null)
                return null;
            return new TestSearchTask(dwCookie, pSearchQuery, pSearchCallback, control);
        }

        public override void ClearSearch()
        {
            // Send the empty search string
            control.ViewModel.SearchOnFiles("");
        }

        internal class TestSearchTask : VsSearchTask
        {
            private TFSUserControl control;

            public TestSearchTask(uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchCallback pSearchCallback, TFSUserControl control)
                : base(dwCookie, pSearchQuery, pSearchCallback)
            {
                this.control = control;
            }

            /// <summary>
            /// Sends the updated search string to the view and viewmodel
            /// </summary>
            protected override void OnStartSearch()
            {
                control.ViewModel.SearchOnFiles(this.SearchQuery.SearchString);
                base.OnStartSearch();

            }
        }

        #endregion
    }
}
