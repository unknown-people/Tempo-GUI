using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TempoWithGUI.Core;
using TempoWithGUI.MVVM.View;
using TempoWithGUI.MVVM.View.RaidView;

namespace TempoWithGUI.MVVM.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        public RelayCommand HomeViewCommand { get; set; }
        public RelayCommand MusicBotCommand { get; set; }
        public RelayCommand RaidCommand { get; set; }
        public RelayCommand ProxiesCommand { get; set; }
        public RelayCommand TokensCommand { get; set; }
        public RelayCommand MoreCommand { get; set; }
        //Window change commands


        public MusicBotModel MusicBotVm { get; set; }
        public RaidModel RaidVm { get; set; }
        public TokensModel TokensVm { get; set; }
        public ProxiesModel ProxiesVm { get; set; }
        public MoreModel MoreVm { get; set; }
        public static Log log { get; set; }

        private object _currentView;

        public object  CurrentView
        {
            get { return  _currentView; }
            set { 
                _currentView = value;
                OnPropertyChanged();
            }
        }
        public bool IsBot
        {
            get {return Settings.Default.isBot; }
            set
            {
                Settings.Default.isBot = value;
            }
        }
        public bool IsNotBot
        {
            get { return !Settings.Default.isBot; }
        }
        public void logPrint(string input)
        {
            var now = DateTime.Now.ToString();
            log.LogText.Text += input + $" {now}\n";
        }
        public MainViewModel()
        {
            MusicBotVm = new MusicBotModel();
            RaidVm = new RaidModel();
            TokensVm = new TokensModel();
            ProxiesVm = new ProxiesModel();
            MoreVm = new MoreModel();

            CurrentView = MusicBotVm;

            MusicBotCommand = new RelayCommand(o =>
            {
                CurrentView = MusicBotVm;
            });
            RaidCommand = new RelayCommand(o =>
            {
                CurrentView = RaidVm;
            });
            ProxiesCommand = new RelayCommand(o =>
            {
                CurrentView = ProxiesVm;
            });
            TokensCommand = new RelayCommand(o =>
            {
                CurrentView = TokensVm;
            });
            MoreCommand = new RelayCommand(o =>
            {
                CurrentView = MoreVm;
            });
            App.mainView = this;
        }
    }
}
