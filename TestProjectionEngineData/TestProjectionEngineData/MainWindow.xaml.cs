using Esri.ArcGISRuntime.Mapping;
using System.Windows;

namespace TestProjectionEngineData
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(envelope => 
                TheMapView.SetViewpoint(new Viewpoint(envelope)));
        }
    }
}
