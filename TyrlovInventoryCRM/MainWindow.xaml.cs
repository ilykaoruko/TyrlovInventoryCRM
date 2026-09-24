using System.Windows;
using TyrlovInventoryCRM.ViewModels;

namespace TyrlovInventoryCRM
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}