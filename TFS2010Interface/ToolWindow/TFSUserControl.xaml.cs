using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace chrisbjohnson.TFS2010Interface
{
    /// <summary>
    /// Interaction logic for MyControl.xaml
    /// </summary>
    public partial class TFSUserControl : UserControl
    {
        public MyControlViewModel ViewModel;

        public TFSUserControl()
        {
            InitializeComponent();

            ViewModel = new MyControlViewModel();
            this.DataContext = ViewModel;
        }
    }
}