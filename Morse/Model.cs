using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Morse
{
    public sealed class Model : INotifyPropertyChanged
    {
        private string _selectedModeItem;
        private ObservableCollection<string> _modeItems;
        private string _selectedFilePath;
        private string _status;
        private byte _syncBlock;
        private ObservableCollection<byte> _dataBlocks;
        private int _progress;


        public Model()
        {
            ModeItems = new ObservableCollection<string>
            {
                Constants.Sed,
                Constants.Rev
            };
            SelectedModeItem =  Constants.Rev;
            _dataBlocks = new ObservableCollection<byte>();
            for (var i = 0; i < Constants.DataRowCount; i++)
            {
                for (var j = 0; j < Constants.DataColCount; j++)
                {
                    _dataBlocks.Add(Constants.Eof);
                }
            }
        }

        public string SelectedModeItem
        {
            get => _selectedModeItem;
            set
            {
                _selectedModeItem = value;
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

        public byte SyncBlock
        {
            get => _syncBlock;
            set
            {
                _syncBlock = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<byte> DataBlocks
        {
            get => _dataBlocks;
            set
            {
                _dataBlocks = value;
                OnPropertyChanged();
            }
        }

        public void FireDataBlocks()
        {
            OnPropertyChanged("DataBlocks");
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

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}