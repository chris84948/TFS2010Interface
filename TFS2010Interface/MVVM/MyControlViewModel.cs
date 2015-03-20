using Microsoft.TeamFoundation.VersionControl.Client;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Text.RegularExpressions;
using EnvDTE;
using System.ComponentModel;

namespace chrisbjohnson.TFS2010Interface
{
    public class MyControlViewModel : ObservableObject
    {
        #region Properties

        private ObservableCollection<TFSItemViewModel> _files;
        private ObservableCollection<TFSItemViewModel> _filteredFiles;
        private ObservableCollection<TFSItemViewModel> _searchedFiles;
        private int _selectedIndex;
        private bool _searchSolution;
        private bool _searchCheckedOut;
        private bool _isLoading;

        private TFSItemController tfsController;
        private string searchText;
        private string currentPath;
        private EnvDTE.SolutionEvents solutionEvents;
        private BackgroundWorker FileLoadingWorker;

        public ObservableCollection<TFSItemViewModel> Files
        {
            get { return _files; }
            set
            {
                _files = value;
                OnPropertyChanged("Files");
            }
        }

        public ObservableCollection<TFSItemViewModel> FilteredFiles
        {
            get { return _filteredFiles; }
            set
            {
                _filteredFiles = value;
                OnPropertyChanged("FilteredFiles");
            }
        }

        public ObservableCollection<TFSItemViewModel> SearchedFiles
        {
            get { return _searchedFiles; }
            set
            {
                _searchedFiles = value;
                OnPropertyChanged("SearchedFiles");
            }
        }

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
                OnPropertyChanged("SelectedIndex");
            }
        }

        public bool SearchSolution
        {
            get { return _searchSolution; }
            set
            {
                _searchSolution = value;

                UpdateFilters();
            }
        }

        public bool SearchCheckedOut
        {
            get { return _searchCheckedOut; }
            set
            {
                _searchCheckedOut = value;

                UpdateFilters();
            }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                OnPropertyChanged("IsLoading");
            }
        }

        #endregion

        public MyControlViewModel()
        {
            // Subscribe to the options updated event
            Singleton.GetObject().OptionsUpdatedEvent += MyControlViewModel_OptionsUpdatedEvent;

            tfsController = new TFSItemController();

            // Sign up for solution events
            DTE dte = Common.GetApplicationReference();
            solutionEvents = ((Events)dte.Events).SolutionEvents;
            solutionEvents.Opened += solutionEvents_Opened;
            solutionEvents.AfterClosing += solutionEvents_AfterClosing;

            // Initially filter is empty
            searchText = "";

            // Setup the background worker for running the slower code
            FileLoadingWorker = new BackgroundWorker();
            FileLoadingWorker.WorkerSupportsCancellation = true;
            FileLoadingWorker.DoWork += FileLoadingWorker_DoWork;

            if (!tfsController.Connected) return;

            ReadFilesFromPath();
        }

        /// <summary>
        /// Options have changed, reload the TFS controller and re-read the files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyControlViewModel_OptionsUpdatedEvent(object sender, System.EventArgs e)
        {
            ReconnectToTFSServer();
        }

        private void ReconnectToTFSServer()
        {
            tfsController = new TFSItemController();

            // If connection was lost or a bad workstation was entered, clear all lists and exit
            if (!tfsController.Connected)
            {
                Files = null;
                FilteredFiles = null;
                SearchedFiles = null;
                return;
            }

            ReadFilesFromPath();
        }


        /// <summary>
        /// Reads all files from the DEV branch to TFS
        /// </summary>
        private void ReadFilesFromPath()
        {
            currentPath = tfsController.Workspace.Folders[0].LocalItem + Singleton.GetObject().TFSPath;

            if (!System.IO.Directory.Exists(currentPath)) return;

            Files = new ObservableCollection<TFSItemViewModel>();

            if (FileLoadingWorker.IsBusy)
                return;

            // Read all files and update filters
            FileLoadingWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Does the heavy lifting in an asynchronous task
        /// </summary>
        private void FileLoadingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Set loading flag
            IsLoading = true;

            List<string> localFiles = tfsController.GetLocalItems(currentPath);

            string filter = Singleton.GetObject().FileFilter;
            bool doFilter= !string.IsNullOrEmpty(filter);

            foreach (string file in localFiles)
            {
                if (doFilter && Regex.IsMatch(file, filter))
                {
                    continue;
                }

                Files.Add(new TFSItemViewModel(file));
            }

            UpdateFilters();

            IsLoading = false;
        }
        /// <summary>
        /// Updates all filters on the FilteredFiles list
        /// </summary>
        private void UpdateFilters()
        {
            if (!tfsController.Connected) return;

            if (SearchCheckedOut)
            {
                FilterFilesOnCheckedOutAndReadWrite();
            }
            else
            {
                FilterFilesOnCurrentSolution();
            }

            SearchOnFiles(searchText);
        }

        /// <summary>
        /// Set the main unfiltered Files list to be only from the current solution
        /// This can then be searched on
        /// </summary>
        private void FilterFilesOnCurrentSolution()
        {
            string solutionDir = Common.GetSolutionDirectory().ToLower();

            // No solution open or search solution not selected so just filter normally
            if (string.IsNullOrEmpty(solutionDir) || !SearchSolution)
                FilteredFiles = Files;
            else
                FilteredFiles = new ObservableCollection<TFSItemViewModel>(Files.Where(p => p.Filepath.ToLower().Contains(solutionDir)).OrderBy(p => p.Filename));
        }

        /// <summary>
        /// Set the main unfiltered Files list to only be checked out files
        /// This can then be searched on
        /// </summary>
        private void FilterFilesOnCheckedOutAndReadWrite()
        {
            string solutionDir = Common.GetSolutionDirectory();

            // Get all files that are checked out for the appropriate path based on the filter selected
            List<string> checkedoutFiles = tfsController.GetFilesWithPendingChanges(SearchSolution && !string.IsNullOrEmpty(solutionDir) ?
                                                                                    solutionDir :
                                                                                    tfsController.Workspace.Folders[0].LocalItem + Singleton.GetObject().TFSPath);

            // First get the read/write files
            FilteredFiles = new ObservableCollection<TFSItemViewModel>(Files.Where(p => p.ReadWrite));

            // Now get the checked out files
            foreach (string file in checkedoutFiles)
            {
                TFSItemViewModel newFile = new TFSItemViewModel(file);
                if (!FilteredFiles.Contains(newFile)) FilteredFiles.Add(newFile);
            }

            // Now sort them
            FilteredFiles = new ObservableCollection<TFSItemViewModel>(FilteredFiles.OrderBy(p => p.Filename));
        }

        /// <summary>
        /// Filter the filtered list of files based on the searchfilter
        /// </summary>
        /// <param name="searchFilter"></param>
        public void SearchOnFiles(string searchFilter)
        {
            if (!tfsController.Connected) return;

            this.searchText = searchFilter;

            // Save a little effort here
            if (searchFilter == "")
                SearchedFiles = new ObservableCollection<TFSItemViewModel>(FilteredFiles.OrderBy(p => p.Filename));
            else
                SearchedFiles = new ObservableCollection<TFSItemViewModel>(FilteredFiles.Where(p => Regex.Match(p.Filepath, searchFilter, RegexOptions.IgnoreCase).Success).OrderBy(p => p.Filename));

            SyncCheckedoutItems();
        }

        /// <summary>
        /// Updates the checked out items on the filtered list
        /// </summary>
        private void SyncCheckedoutItems()
        {
            List<string> checkedoutFiles = tfsController.GetFilesWithPendingChanges(currentPath);

            foreach (string file in checkedoutFiles)
            {
                TFSItemViewModel item = SearchedFiles.FirstOrDefault(x => x.Filepath.ToLower() == file.ToLower());

                // If the item was found
                if (item != null)
                {
                    item.CheckedOut = true;
                }
            }
        }

        /// <summary>
        /// Re-reads all files from the Master files
        /// </summary>
        internal void RefreshAllFiles()
        {
            if (!tfsController.Connected)
            {
                // If we're not connected attempt to reconnect to TFS
                ReconnectToTFSServer();
            }
            else
            {
                // If we're already connected, just re-read the files
                ReadFilesFromPath();
            }
        }

        #region Commands

        /// <summary>
        /// Command for opening a file in the file explorer
        /// </summary>
        public ICommand OpenFileFolder { get { return new RelayCommand(OpenFileFolderExecute, CanOpenFileFolderExecute); } }

        private void OpenFileFolderExecute()
        {
            if (!File.Exists(FilteredFiles[SelectedIndex].Filepath))
            {
                return;
            }

            // combine the arguments together
            // it doesn't matter if there is a space after ','
            string argument = @"/select, " + SearchedFiles[SelectedIndex].Filepath;

            System.Diagnostics.Process.Start("explorer.exe", argument);
        }

        private bool CanOpenFileFolderExecute()
        {
            return true;
        }

        /// <summary>
        /// Checks out the file from TFS
        /// </summary>
        public ICommand Checkout { get { return new RelayCommand(CheckoutExecute, CanCheckoutExecute); } }

        private void CheckoutExecute()
        {
            // Checkout the file and re-filter
            tfsController.CheckoutFile(SearchedFiles[SelectedIndex].Filepath);
            SyncCheckedoutItems();
        }

        private bool CanCheckoutExecute()
        {
            return true;
        }

        #endregion

        #region Events

        void solutionEvents_AfterClosing()
        {
            UpdateFilters();
        }

        void solutionEvents_Opened()
        {
            UpdateFilters();
        }

        #endregion
    }
}