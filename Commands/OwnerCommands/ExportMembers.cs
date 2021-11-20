using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TempoWithGUI.Commands
{
    [Command("exmembers")]
    class ExportMembersCommand : CommandBase
    {
        public override void Execute()
        {
            if (!Directory.Exists(App.strWorkPath + "/members"))
            {
                Directory.CreateDirectory(App.strWorkPath + "/members");
            }
            var guild = Client.GuildCache[Message.Guild.Id];
            var members = guild.GetChannelMembersAsync(Message.Channel.Id).GetAwaiter().GetResult();
            File.Delete(App.strWorkPath + "/members/" + guild.Name.ToLower().Replace(' ', '-') + "_member_list.txt");
            while (true)
            {
                try
                {
                    using (StreamWriter stream = new StreamWriter(App.strWorkPath + "/members/" + guild.Name.ToLower().Replace(' ', '-') + "_member_list.txt"))
                    {
                        foreach (var member in members)
                        {
                            stream.WriteLine(member.User.Id.ToString());
                        }
                    }
                    break;
                }
                catch (IOException ex) { Thread.Sleep(100); }
            }
            SendMessageAsync("Successfully saved member list to " + App.strWorkPath + "/members");
        }
    }
}
