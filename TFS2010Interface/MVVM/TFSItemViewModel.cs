using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace chrisbjohnson.TFS2010Interface
{
    public class TFSItemViewModel : ObservableObject
    {
        public TFSItemData TFSItem;

        public string Filename
        {
            get { return TFSItem.Filename; }
            set
            {
                TFSItem.Filename = value;
                OnPropertyChanged("Filename");
            }
        }

        public string Filepath
        {
            get { return TFSItem.Filepath; }
            set
            {
                TFSItem.Filepath = value;
                OnPropertyChanged("Filepath");
            }
        }

        public bool CheckedOut
        {
            get { return TFSItem.CheckedOut; }
            set
            {
                TFSItem.CheckedOut = value;
                OnPropertyChanged("CheckedOut");
            }
        }

        public bool ReadWrite
        {
            get { return TFSItem.ReadWrite; }
            set
            {
                TFSItem.ReadWrite = value;
                OnPropertyChanged("ReadWrite");
            }
        }

        /// <summary>
        /// Constructor for creating view model
        /// </summary>
        /// <param name="filepath"></param>
        public TFSItemViewModel(string filepath)
        {
            // Create a new TFSItemData object first
            TFSItem = new TFSItemData();

            this.Filepath = filepath;

            this.Filename = Path.GetFileName(filepath);

            // Default checkout to false - will update value later when filtering
            this.CheckedOut = false;

            this.ReadWrite = !new FileInfo(filepath).IsReadOnly;
        }

        /// <summary>
        /// Items should be compared on full filename
        /// </summary>
        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            TFSItemViewModel item = obj as TFSItemViewModel;
            if ((System.Object)item == null)
            {
                return false;
            }

            return item.Filepath == Filepath;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
