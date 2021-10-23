using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TempoWithGUI.Core;
using TempoWithGUI.MVVM.View.ProfilesView;

namespace TempoWithGUI.MVVM.ViewModel
{
    class ProfilesModel : ObservableObject
    {
        public RelayCommand imageCommand { get; set; }
        public RelayCommand nameCommand { get; set; }
        public RelayCommand bioCommand { get; set; }
        private object _currentView;
        public imageProfiles imageVm { get; set; }
        public nameProfiles nameVm { get; set; }
        public bioProfiles bioVm { get; set; }
        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }
        public ProfilesModel()
        {
            imageVm = new imageProfiles();
            nameVm = new nameProfiles();
            bioVm = new bioProfiles();
            CurrentView = imageVm;

            imageCommand = new RelayCommand(o =>
            {
                CurrentView = imageVm;
            });
            nameCommand = new RelayCommand(o =>
            {
                CurrentView = nameVm;
            });
            bioCommand = new RelayCommand(o =>
            {
                CurrentView = bioVm;
            });
        }
    }
}
