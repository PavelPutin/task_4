using System.Collections.Specialized;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using task_4.Model;
using task_4.ViewModel;

namespace task_4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ((INotifyCollectionChanged)logMessages.Items.SourceCollection).CollectionChanged += MainWindow_CollectionChanged; ;
            SimulationViewModel simulationViewModel = new();
            simulationViewModel.Init();
            DataContext = simulationViewModel;
        }

        private void MainWindow_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            logMessages.ScrollIntoView(logMessages.Items[logMessages.Items.Count - 1]);
        }
    }
}