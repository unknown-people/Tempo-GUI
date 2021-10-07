using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using TempoWithGUI.Core;
using TempoWithGUI.MVVM.View.RaidView;

namespace TempoWithGUI.MVVM.ViewModel
{
    class GuildRaidModel : ObservableObject
    {
        public RelayCommand JoinCommand { get; set; }
        public RelayCommand SpamCommand { get; set; }

        private object _currentView;

        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }
        public GuildRaidModel()
        {
            var main = new JoinGuild();
            CurrentView = main;

            JoinCommand = new RelayCommand(o =>
            {
                CurrentView = new JoinGuild();
            });
            SpamCommand = new RelayCommand(o =>
            {
                CurrentView = new SpamGuild();
            });
        }
    }
}
