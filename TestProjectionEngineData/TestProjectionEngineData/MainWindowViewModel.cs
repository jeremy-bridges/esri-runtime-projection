using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using MessageBox = System.Windows.MessageBox;

namespace TestProjectionEngineData
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly Action<Envelope> _zoom;
        private Map _map;

        public MainWindowViewModel(Action<Envelope> zoom)
        {
            _zoom = zoom;
            AddReplicaCommand = new SimpleCommand(AddReplica);

            MapInit();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Map Map 
        {
            get => _map;
            set
            {
                _map = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Map)));
            }
        }

        public ICommand AddReplicaCommand { get; }

        private async Task MapInit()
        {
            await Task.Delay(TimeSpan.FromSeconds(3));

            var folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                TransformationCatalog.ProjectionEngineDirectory = folderDialog.SelectedPath;
                Map = new Map(new Basemap(new ArcGISTiledLayer(new Uri(
                    "https://services.arcgisonline.com/arcgis/rest/services/World_Street_Map/MapServer"))));
                
                foreach (var file in Directory.GetFiles(folderDialog.SelectedPath, "*.geodatabase"))
                {
                    await AddReplica(file);
                }

                _zoom(new Envelope(
                    x1: 400177.58867753856,
                    y1: 650887.0249267563,
                    x2: 400367.92690205923,
                    y2: 650988.133270391,
                    SpatialReference.Create(27700)));
            }
        }

        private void AddReplica()
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                AddReplica(dialog.FileName);
            }
        }

        private async Task AddReplica(string gdbPath)
        {
            try
            {
                var gdb = await Geodatabase.OpenAsync(gdbPath);

                foreach (var table in gdb.GeodatabaseFeatureTables)
                {
                    Map.OperationalLayers.Add(new FeatureLayer(table));
                }

                foreach (var dimTable in gdb.GeodatabaseDimensionTables)
                {
                    Map.OperationalLayers.Add(new DimensionLayer(dimTable));
                }

                foreach (var annoTable in gdb.GeodatabaseAnnotationTables)
                {
                    Map.OperationalLayers.Add(new AnnotationLayer(annoTable));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Geodatabase open failed: {e.Message}");
            }
        }
    }
}
