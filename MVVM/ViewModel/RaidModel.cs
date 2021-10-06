using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using TempoWithGUI.Core;

namespace TempoWithGUI.MVVM.ViewModel
{
    class RaidModel : ObservableObject
    {
        public RelayCommand GuildRaid { get; set; }
        public RelayCommand DMRaid { get; set; }
        public RelayCommand FriendRaid { get; set; }
        public RelayCommand ReactionRaid { get; set; }
        public RelayCommand VoiceRaid { get; set; }
        public RelayCommand WebhookRaid { get; set; }
        public RelayCommand MassDMRaid { get; set; }
        public RelayCommand TokenRaid { get; set; }
        public RelayCommand CallRaid { get; set; }


        public RaidModel()
        {
            GuildRaid = new RelayCommand(o =>
            {
                ((Border)o).Cursor = Cursors.AppStarting;
            });
            DMRaid = new RelayCommand(o =>
            {
                ((Border)o).Cursor = Cursors.AppStarting;
            });
            FriendRaid = new RelayCommand(o =>
            {
                ((Border)o).Cursor = Cursors.AppStarting;
            });
            ReactionRaid = new RelayCommand(o =>
            {
                ((Border)o).Cursor = Cursors.AppStarting;
            });
            VoiceRaid = new RelayCommand(o =>
            {
                ((Border)o).Cursor = Cursors.AppStarting;
            });
            WebhookRaid = new RelayCommand(o =>
            {
                ((Border)o).Cursor = Cursors.AppStarting;
            });
            MassDMRaid = new RelayCommand(o =>
            {
                ((Border)o).Cursor = Cursors.AppStarting;
            });
            TokenRaid = new RelayCommand(o =>
            {
                ((Border)o).Cursor = Cursors.AppStarting;
            });
            CallRaid = new RelayCommand(o =>
            {
                ((Border)o).Cursor = Cursors.AppStarting;
            });
        }
    }
}
