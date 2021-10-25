using Discord;
using Discord.Commands;
using Discord.Gateway;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TempoWithGUI.Commands
{
    [Command("dmall")]
    class DmAllCommand : CommandBase
    {
        public static List<GuildMember> members {get;set;}
        public override void Execute()
        {
            var rnd = new Random();
            if(members == null || members.Count == 0)
                members = (List<GuildMember>)Client.GetGuildChannelMembers(784063251775619102, 785654803187367986);
            while (true)
            {
                var mems = new GuildMember[3];
                mems[0] = members[rnd.Next(0, members.Count)];
                members = members.Where(o => o != mems[0]).ToList();
                mems[1] = members[rnd.Next(0, members.Count)];
                members = members.Where(o => o != mems[1]).ToList();
                mems[2] = members[rnd.Next(0, members.Count)];
                members = members.Where(o => o != mems[2]).ToList();
                var dms = new List<PrivateChannel>();
                foreach (var member in mems)
                {
                    var dm1 = Client.CreateDM(member.User.Id);
                    try
                    {
                        dm1.GetMessages(new MessageFilters()
                        {
                            Limit = 50
                        });
                    }
                    catch (Exception ex) { }
                    dms.Add(dm1);
                    Thread.Sleep(rnd.Next(500, 1000));
                }
                foreach (var dm in dms)
                {
                    try
                    {
                        var message = dm.SendMessage("Test dm");
                        if (message == null)
                            break;
                    }
                    catch (Exception ex) { break; }
                    Thread.Sleep(rnd.Next(2000, 5000));
                }
                Thread.Sleep(360000);
            }
        }
    }
}
