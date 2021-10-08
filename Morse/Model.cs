using System;
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
            for (var i = 0; i < Configs.GridRowCount; i++)
            {
                for (var j = 0; j < Configs.GridColCount; j++)
                {
                    _dataBytes.Add(0);
                }
            }

            FileData = Array.Empty<byte>();
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

        public byte CurrBlockByte { get; set; }

        #region send

        public byte[] FileData { get; set; }
        public int TotalFrames { get; set; }
        public int SentFrames { get; set; }
        public int SendOffset { get; set; }
        public DateTime LastConfirmTime { get; set; }

        #endregion

        #region receive

        public Point DataBlockLeftLocation { get; set; }
        public Point DataBlockRightLocation { get; set; }

        public Point DataBlockCenterLocation
        {
            get
            {
                int x = DataBlockLeftLocation.X + (DataBlockRightLocation.X - DataBlockLeftLocation.X) / 2;
                int y = DataBlockLeftLocation.Y + (DataBlockRightLocation.Y - DataBlockLeftLocation.Y) / 2;
                return new Point(x, y);
            }
        }

        public Point OutOfDataBlockLocation
        {
            get
            {
                int x = DataBlockLeftLocation.X - 100;
                int y = DataBlockLeftLocation.Y - 200;
                return new Point(x, y);
            }
        }

        public int ReceivedFrames { get; set; }
        #endregion


        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}