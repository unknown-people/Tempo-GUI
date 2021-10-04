using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TempoWithGUI.Core;

namespace TempoWithGUI.MVVM.ViewModel
{
    class MainViewModel : ObservableObject
    {
        public RelayCommand HomeViewCommand { get; set; }
        public RelayCommand MusicBotCommand { get; set; }
        public RelayCommand RaidCommand { get; set; }
        public RelayCommand ProxiesCommand { get; set; }
        public RelayCommand TokensCommand { get; set; }
        public RelayCommand MoreCommand { get; set; }
        //Window change commands

        public RelayCommand MusicBotBtn { get; set; }
        public RelayCommand RaidBtn { get; set; }


        public MusicBotModel MusicBotVm { get; set; }
        public RaidModel RaidVm { get; set; }
        public TokensModel TokensVm { get; set; }
        public ProxiesModel ProxiesVm { get; set; }
        public MoreModel MoreVm { get; set; }

        private object _currentView;

        public object  CurrentView
        {
            get { return  _currentView; }
            set { 
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            MusicBotVm = new MusicBotModel();
            CurrentView = MusicBotVm;

            MusicBotCommand = new RelayCommand(o =>
            {
                CurrentView = MusicBotVm;
            });
            RaidCommand = new RelayCommand(o =>
            {
                CurrentView = new RaidModel();
            });
            ProxiesCommand = new RelayCommand(o =>
            {
                CurrentView = new ProxiesModel();
            });
            TokensCommand = new RelayCommand(o =>
            {
                CurrentView = new TokensModel();
            });
            MoreCommand = new RelayCommand(o =>
            {
                CurrentView = new MoreModel();
            });

        }
    }
}
