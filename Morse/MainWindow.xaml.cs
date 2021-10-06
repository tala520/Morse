using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Shapes;

namespace Morse
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            BuildDataGrid();
        }

        private void BuildDataGrid()
        {
            DataGrid.ShowGridLines = true;
            for (var i = 0; i < Constants.DataColCount; i++)
            {
                DataGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (var i = 0; i < Constants.DataRowCount; i++)
            {
                DataGrid.RowDefinitions.Add(new RowDefinition());
            }

            for (var i = 0; i < Constants.DataRowCount; i++)
            {
                for (var j = 0; j < Constants.DataColCount; j++)
                {
                    Rectangle rect = new Rectangle();
                    DataGrid.Children.Add(rect);
                    Grid.SetRow(rect, i);
                    Grid.SetColumn(rect, j);
                    
                    Binding binding = new Binding
                    {
                        Source = Resources["ViewModel"],
                        Path = new PropertyPath("Model.DataBlocks"),
                        Mode = BindingMode.OneWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                        Converter = new ByteToBrushConverter(),
                        ConverterParameter = i * Constants.DataColCount + j
                    };
                    BindingOperations.SetBinding(rect, Shape.FillProperty, binding);
                }
            }
        }
    }
}