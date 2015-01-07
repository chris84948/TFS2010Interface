using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;

namespace chrisbjohnson.TFS2010Interface
{
    public class TFSItemController
    {
        /// <summary>
        /// Reference to the workspace
        /// </summary>
        public Workspace Workspace;

        public Boolean Connected;

        /// <summary>
        /// Constructor to connect to the local TFS workspace
        /// </summary>
        public TFSItemController()
        {
            try
            {
                TfsTeamProjectCollection server = new TfsTeamProjectCollection(new Uri(Singleton.GetObject().TFSServer));
                VersionControlServer vcs = (VersionControlServer)server.GetService(typeof(VersionControlServer));

                string workspaceName = GetWorkspaceName(vcs);

                Workspace = vcs.GetWorkspace(workspaceName, System.Security.Principal.WindowsIdentity.GetCurrent().Name);
            }
            catch 
            { 
                // do nothing, we should just exit 
            }

            if (Workspace != null) Connected = true;
        }

        /// <summary>
        /// Checks the resources of the application to see if the workspace name has already been selected
        /// If it has not, it calls another method to determine the workspace
        /// </summary>
        /// <param name="vcs">VersionControlServer reference</param>
        /// <returns>String workspace name</returns>
        private string GetWorkspaceName(VersionControlServer vcs)
        {
            // Read the workspace from the resources
            string tfsWorkspace = Singleton.GetObject().TFSWorkspace;

            // Get all available workspaces for this user on this computer
            Workspace[] workspaces = vcs.QueryWorkspaces(null, System.Security.Principal.WindowsIdentity.GetCurrent().Name, Environment.MachineName);

            foreach (Workspace workspace in workspaces)
            {
                if (workspace.Name == tfsWorkspace)
                {
                    return tfsWorkspace;
                }
            }

            return "";
        }

        /// <summary>
        /// Checks in the changes passed
        /// </summary>
        /// <param name="pendingChanges">Array of pending changes for all the files changed</param>
        /// <param name="comment">Comment to associate with the check-in</param>
        /// <returns>Changeset number associated with the checkin</returns>
        public int CheckInChangesToWorkspace(PendingChange[] pendingChanges, string comment)
        {
            // Checkin all passed pending changes with the passed comment
            return Workspace.CheckIn(pendingChanges, comment);
        }

        /// <summary>
        /// Method to check a file out
        /// </summary>
        /// <param name="filePath">Full file path of the file to checkout</param>
        public void CheckoutFile(string filePath)
        {
            string serverPath = Workspace.GetServerItemForLocalItem(filePath);
            int numFiles = Workspace.PendEdit(serverPath);
        }

        /// <summary>
        /// Reads pending changes from a selected file path
        /// </summary>
        /// <param name="filepath">File path to search for pending changes in</param>
        /// <returns>List of strings containing the filenames of the checkout out files</returns>
        public List<string> GetFilesWithPendingChanges(string filepath)
        {
            PendingChange[] changes = Workspace.GetPendingChanges(filepath, RecursionType.Full);

            List<string> checkedoutFiles = new List<string>();

            foreach (PendingChange change in changes)
            {
                checkedoutFiles.Add(change.LocalItem);
            }

            return checkedoutFiles;
        }

        /// <summary>
        /// Gets the local items for a workspace and a given path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal List<string> GetLocalItems(string path)
        {
            ItemSet items = this.Workspace.VersionControlServer.GetItems(path, RecursionType.Full);
            
            List<string> localVersionFilenames = new List<string>();

            foreach (Item fileItem in items.Items)
            {
                if (fileItem.ItemType == ItemType.File)
                    localVersionFilenames.Add(Workspace.GetLocalItemForServerItem(fileItem.ServerItem));
            }

            return localVersionFilenames;
        }
    }
}
