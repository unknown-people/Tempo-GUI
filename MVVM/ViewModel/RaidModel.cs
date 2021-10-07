using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using TempoWithGUI.Core;
using TempoWithGUI.MVVM.View;
using TempoWithGUI.MVVM.View.RaidView;

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

        public static bool guildOn;
        public static bool dmOn;
        public static bool friendOn;
        public static bool reactionOn;
        public static bool voiceOn;
        public static bool webhookOn;
        public static bool massdmOn;
        public static bool tokenOn;
        public static bool callOn;


        public RaidModel()
        {
            GuildRaid = new RelayCommand(o =>
            {
                if (guildOn)
                    return;
                ((Border)o).Cursor = Cursors.AppStarting;
                var guild_raid = new GuildRaid();
                guild_raid.Show();
                ((Border)o).Cursor = Cursors.Hand;
                guildOn = true;
            });
            DMRaid = new RelayCommand(o =>
            {
                if (dmOn)
                    return;
                ((Border)o).Cursor = Cursors.AppStarting;
                var dm_raid = new DmRaid();
                dm_raid.Show();
                ((Border)o).Cursor = Cursors.Hand;
                dmOn = true;
            });
            FriendRaid = new RelayCommand(o =>
            {
                if (friendOn)
                    return;
                ((Border)o).Cursor = Cursors.AppStarting;
                
                ((Border)o).Cursor = Cursors.Hand;
                friendOn = true;
            });
            ReactionRaid = new RelayCommand(o =>
            {
                if (reactionOn)
                    return;
                ((Border)o).Cursor = Cursors.AppStarting;
                ((Border)o).Cursor = Cursors.Hand;
                reactionOn = true;
            });
            VoiceRaid = new RelayCommand(o =>
            {
                if (voiceOn)
                    return;
                ((Border)o).Cursor = Cursors.AppStarting;
                var voice_raid = new VoiceRaid();
                voice_raid.Show();
                ((Border)o).Cursor = Cursors.Hand;
                voiceOn = true;
            });
            WebhookRaid = new RelayCommand(o =>
            {
                if (webhookOn)
                    return;
                ((Border)o).Cursor = Cursors.AppStarting;
                ((Border)o).Cursor = Cursors.Hand;
                webhookOn = true;
            });
            MassDMRaid = new RelayCommand(o =>
            {
                if (massdmOn)
                    return;
                ((Border)o).Cursor = Cursors.AppStarting;
                ((Border)o).Cursor = Cursors.Hand;
                massdmOn = true;
            });
            TokenRaid = new RelayCommand(o =>
            {
                if (tokenOn)
                    return;
                ((Border)o).Cursor = Cursors.AppStarting;
                ((Border)o).Cursor = Cursors.Hand;
                tokenOn = true;
            });
            CallRaid = new RelayCommand(o =>
            {
                if (callOn)
                    return;
                ((Border)o).Cursor = Cursors.AppStarting;
                ((Border)o).Cursor = Cursors.Hand;
                callOn = true;
            });
        }
    }
}
