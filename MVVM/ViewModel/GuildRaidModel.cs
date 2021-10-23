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
        public RelayCommand LeaveCommand { get; set; }
        public RelayCommand NukeCommand { get; set; }

        private object _currentView;

        //Views
        public JoinGuild joinGuild { get; set; }
        public SpamGuild spamGuild { get; set; }
        public LeaveGuild leaveGuild { get; set; }
        public NukeGuild nukeGuild { get; set; }
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
            joinGuild = new JoinGuild();
            spamGuild = new SpamGuild();
            leaveGuild = new LeaveGuild();
            nukeGuild = new NukeGuild();

            CurrentView = joinGuild;

            JoinCommand = new RelayCommand(o =>
            {
                CurrentView = joinGuild;
            });
            SpamCommand = new RelayCommand(o =>
            {
                CurrentView = spamGuild;
            });
            LeaveCommand = new RelayCommand(o =>
            {
                CurrentView = leaveGuild;
            });
            NukeCommand = new RelayCommand(o =>
            {
                CurrentView = nukeGuild;
            });
        }
    }
}
