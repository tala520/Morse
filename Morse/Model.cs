using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace Morse
{
    public sealed class Model : INotifyPropertyChanged
    {
        private string _selectedModeItem;
        private ObservableCollection<string> _modeItems;
        private string _selectedFilePath;
        private string _status;
        private string _mouse;
        private byte _syncByte;
        private ObservableCollection<byte?> _dataBytes;
        private int _progress;


        public Model()
        {
            ModeItems = new ObservableCollection<string>
            {
                Configs.Sed,
                Configs.Rev
            };
            SelectedModeItem = Configs.GetModeConfig();
            _dataBytes = new ObservableCollection<byte?>();
            for (var i = 0; i < Configs.DataRowCount; i++)
            {
                for (var j = 0; j < Configs.DataColCount; j++)
                {
                    _dataBytes.Add(0);
                }
            }
        }

        public string SelectedModeItem
        {
            get => _selectedModeItem;
            set
            {
                _selectedModeItem = value;
                Configs.SetModeConfig(value);
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> ModeItems
        {
            get => _modeItems;
            set
            {
                _modeItems = value;
                OnPropertyChanged();
            }
        }

        public string SelectedFilePath
        {
            get => _selectedFilePath;
            set
            {
                _selectedFilePath = value;
                OnPropertyChanged();
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }
        
        public string Mouse
        {
            get => _mouse;
            set
            {
                _mouse = value;
                OnPropertyChanged();
            }
        }
        
        public byte SyncByte
        {
            get => _syncByte;
            set
            {
                _syncByte = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<byte?> DataBytes
        {
            get => _dataBytes;
            set
            {
                _dataBytes = value;
                OnPropertyChanged();
            }
        }

        public void FireDataBlocks()
        {
            OnPropertyChanged("DataBytes");
        }

        public int Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                OnPropertyChanged();
            }
        }

        public Point SyncBlockLeftLocation { get; set; }
        
        public Point SyncBlockRightLocation { get; set; }

        public Point SyncBlockCenterPoint =>
            new Point(SyncBlockLeftLocation.X+(SyncBlockRightLocation.X - SyncBlockLeftLocation.X) / 2,
                SyncBlockLeftLocation.Y+(SyncBlockRightLocation.Y - SyncBlockLeftLocation.Y) / 2);

        public int SyncBlockClickedCount { get; set; }
        public Point DataBlockLeftLocation { get; set; }
        
        public Point DataBlockRightLocation { get; set; }
        

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}