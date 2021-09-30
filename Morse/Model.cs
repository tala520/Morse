using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace Morse
{
    public class Model : INotifyPropertyChanged
    {

        private string selectedModeItem;
        private ObservableCollection<string> modeItems;
        private string color1;
        private string color2;
        
        public string SelectedModeItem
        {
            get { return selectedModeItem; }
            set
            {
                this.selectedModeItem = value;
                OnPropertyChanged("SelectedModeItem");
            }
        }

        public ObservableCollection<string> ModeItems
        {
            get { return modeItems; }
            set
            {
                this.modeItems = value;
                OnPropertyChanged("PluginItems");
            }
        }

        public string Color1
        {
            get { return color1; }
            set
            {
                this.color1 = value;
                OnPropertyChanged("Color1");
            }
        }
        
        public string Color2
        {
            get { return color2; }
            set
            {
                this.color2 = value;
                OnPropertyChanged("Color2");
            }
        }
        
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}