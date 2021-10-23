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
    public class RaidModel : ObservableObject
    {
        public RelayCommand GuildRaid { get; set; }
        public RelayCommand DMRaid { get; set; }
        public RelayCommand FriendRaid { get; set; }
        public RelayCommand ReactionRaid { get; set; }
        public RelayCommand VoiceRaid { get; set; }
        public RelayCommand WebhookRaid { get; set; }
        public RelayCommand MassDMRaid { get; set; }
        public RelayCommand TokenRaid { get; set; }
        public RelayCommand ProfilesRaid { get; set; }

        public GuildRaid guildRaid { get; set; }
        public DmRaid dmRaid { get; set; }
        public FriendRaid friendRaid { get; set; }
        public VoiceRaid voiceRaid { get; set; }
        public MassDM massDM { get; set; }
        public TokenRaid tokenRaid { get; set; }
        public ProfilesRaid profilesRaid { get; set; }

        public static bool guildOn;
        public static bool dmOn;
        public static bool friendOn;
        public static bool reactionOn;
        public static bool voiceOn;
        public static bool webhookOn;
        public static bool massdmOn;
        public static bool tokenOn;
        public static bool profilesOn;


        public RaidModel()
        {
            guildRaid = new GuildRaid();
            dmRaid = new DmRaid();
            friendRaid = new FriendRaid();
            voiceRaid = new VoiceRaid();
            massDM = new MassDM();
            tokenRaid = new TokenRaid();
            profilesRaid = new ProfilesRaid();

            GuildRaid = new RelayCommand(o =>
            {
                if (guildOn)
                    return;
                ((Border)o).Cursor = Cursors.AppStarting;
                guildRaid.Show();
                guildRaid.Activate();
                ((Border)o).Cursor = Cursors.Hand;
                guildOn = true;
            });
            DMRaid = new RelayCommand(o =>
            {
                if (dmOn)
                    return;
                ((Border)o).Cursor = Cursors.AppStarting;
                dmRaid.Show();
                dmRaid.Activate();
                ((Border)o).Cursor = Cursors.Hand;
                dmOn = true;
            });
            FriendRaid = new RelayCommand(o =>
            {
                if (friendOn)
                    return;
                ((Border)o).Cursor = Cursors.AppStarting;
                friendRaid.Show();
                friendRaid.Activate();
                ((Border)o).Cursor = Cursors.Hand;
                friendOn = true;
            });
            ReactionRaid = new RelayCommand(o =>
            {
                if (reactionOn)
                    return;
                ((Border)o).Cursor = Cursors.AppStarting;
                var coming_soon = new ComingSoonWindow();
                coming_soon.Show();
                coming_soon.Activate();
                ((Border)o).Cursor = Cursors.Hand;
                reactionOn = true;
            });
            VoiceRaid = new RelayCommand(o =>
            {
                if (voiceOn)
                    return;
                ((Border)o).Cursor = Cursors.AppStarting;
                voiceRaid.Show();
                voiceRaid.Activate();
                ((Border)o).Cursor = Cursors.Hand;
                voiceOn = true;
            });
            WebhookRaid = new RelayCommand(o =>
            {
                if (webhookOn)
                    return;
                ((Border)o).Cursor = Cursors.AppStarting;
                var coming_soon = new ComingSoonWindow();
                coming_soon.Show();
                coming_soon.Activate();
                ((Border)o).Cursor = Cursors.Hand;
                webhookOn = true;
            });
            MassDMRaid = new RelayCommand(o =>
            {
                if (massdmOn)
                    return;
                ((Border)o).Cursor = Cursors.AppStarting;
                massDM.Show();
                massDM.Activate();
                ((Border)o).Cursor = Cursors.Hand;
                massdmOn = true;
            });
            TokenRaid = new RelayCommand(o =>
            {
                if (tokenOn)
                    return;
                ((Border)o).Cursor = Cursors.AppStarting;
                tokenRaid.Show();
                tokenRaid.Activate();
                ((Border)o).Cursor = Cursors.Hand;
                tokenOn = true;
            });
            ProfilesRaid = new RelayCommand(o =>
            {
                if (profilesOn)
                    return;
                ((Border)o).Cursor = Cursors.AppStarting;
                profilesRaid.Show();
                profilesRaid.Activate();
                ((Border)o).Cursor = Cursors.Hand;
                profilesOn = true;
            });
        }
    }
}
