using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ManualMaximize
{
    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private bool maximizeButtonVisible = true;
        public bool MaximizeButtonVisible { get
            {
                return maximizeButtonVisible;
            }
            set
            {
                if (maximizeButtonVisible != value)
                {
                    maximizeButtonVisible = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private bool minimizeButtonVisible = true;
        public bool MinimizeButtonVisible { get
            { 
                return minimizeButtonVisible; 
            }
            set
            {
                if (minimizeButtonVisible != value)
                {
                    minimizeButtonVisible = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private bool closeButtonVisible = true;
        public bool CloseButtonVisible { get
            {
                return closeButtonVisible;
            }          
            set
            {
                if (closeButtonVisible != value)
                {
                    closeButtonVisible = value;
                    NotifyPropertyChanged();
                }
            }
        }

    }
}
