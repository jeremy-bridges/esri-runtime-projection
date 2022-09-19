using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Input;

namespace TestProjectionEngineData
{
    internal class MainWindowViewModel
    {
        private readonly Action<Envelope> _zoom;

        public MainWindowViewModel(Action<Envelope> zoom)
        {
            _zoom = zoom;
            TransformationCatalog.ProjectionEngineDirectory = @"C:\pedata";
            Map = new Map(new Basemap(new ArcGISTiledLayer(new Uri(
                "https://services.arcgisonline.com/arcgis/rest/services/World_Street_Map/MapServer"))));
            AddReplicaCommand = new SimpleCommand(AddReplica);
        }

        public Map Map { get; }

        public ICommand AddReplicaCommand { get; }

        private void AddReplica()
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() ?? false)
            {
                Geodatabase.OpenAsync(dialog.FileName).ContinueWith(t =>
                {
                    if (t.IsCanceled)
                    {
                        MessageBox.Show("Geodatabase open cancelled");
                        return;
                    }
                    if (t.IsFaulted)
                    {
                        MessageBox.Show($"Geodatabase open failed: {t.Exception.Message}");
                        return;
                    }

                    foreach (var table in t.Result.GeodatabaseFeatureTables)
                    {
                        Map.OperationalLayers.Add(new FeatureLayer(table));
                    }

                    foreach (var dimTable in t.Result.GeodatabaseDimensionTables)
                    {
                        Map.OperationalLayers.Add(new DimensionLayer(dimTable));
                    }

                    foreach (var annoTable in t.Result.GeodatabaseAnnotationTables)
                    {
                        Map.OperationalLayers.Add(new AnnotationLayer(annoTable));
                    }

                    _zoom(new Envelope(
                        x1: 400177.58867753856,
                        y1: 650887.0249267563,
                        x2: 400367.92690205923,
                        y2: 650988.133270391,
                        SpatialReference.Create(27700)));
                });
            }
        }
    }
}
