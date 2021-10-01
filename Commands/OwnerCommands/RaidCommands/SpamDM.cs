
using Discord.Commands;
using Discord;
using Discord.Gateway;
using System;
using TempoWithGUI;
using System.Threading;

namespace Music_user_bot.Commands
{
    [Command("spamdm")]
    class SpamDMCommand : CommandBase
    {
        [Parameter("userId/message")]
        public string content { get; set; }
        public ulong userId { get; set; }
        public static bool isSpamming { get; set; }
        public override void Execute()
        {
            if (!App.isOwner(Message) && !App.isAdmin(Message))
            {
                SendMessageAsync("You need to be the owner or an administrator to execute this command!");
                return;
            }
            userId = ulong.Parse(content.Split(' ')[0]);
            if(userId.ToString().Length != 18 || content.Split(' ').Length < 2)
            {
                SendMessageAsync("Insert a valid user id and message.\n**Usage:** " + CommandHandler.Prefix + "spamdm [userId] [message]");
                return;
            }
            string message = "";
            if(content.Split(' ').Length > 2)
            {
                string[] buffer_arr = RemoveFromBeginning(content.Split(' '), 1);
                message = ArrayToString(buffer_arr);
            }
            else
            {
                message = content.Split(' ')[1];
            }
            var dmChannel = Client.CreateDM(userId);

            isSpamming = true;
            Thread spam = new Thread(() => SpamDm(dmChannel, message));
            spam.Priority = ThreadPriority.AboveNormal;
            spam.Start();

            DiscordClient client = new DiscordClient(App.botToken);
            string discriminator = "";
            for (int i = 0; i < 4 - ((client.GetUser(userId).Discriminator)).ToString().Length; i++)
            {
                discriminator += "0";
            }
            discriminator += client.GetUser(userId).Discriminator;
            var user_name = client.GetUser(userId).Username + "#" + discriminator;
            Client.CreateDM(Message.Author.User.Id).SendMessage("Started spamming to " + user_name + "\n" +
                "To stop me use the command " + CommandHandler.Prefix + "stopspam.");
        }
        public string[] RemoveFromBeginning(string[] input, int offset)
        {
            if (offset >= input.Length)
                return new string[] { };
            string[] return_array = new string[input.Length - offset];
            int i = 0;
            foreach (string value in input)
            {
                if (i <= offset - 1)
                {
                    i += 1;
                    continue;
                }
                try
                {
                    return_array[i - offset] = input[i];
                }
                catch (IndexOutOfRangeException)
                {
                    break;
                }
                i += 1;
            }
            return return_array;
        }
        public string ArrayToString(string[] input)
        {
            var message = "";
            foreach(string value in input)
            {
                message += value + " ";
            }
            return message;
        }
        public void SpamDm(PrivateChannel dmChannel, string message)
        {
            while (!StopSpamDmCommand.stopSpamDm)
            {
                Random r = new Random();
                int interval = r.Next(1, 5);
                Thread.Sleep(interval);
                try
                {
                    dmChannel.SendMessage(message);
                }
                catch (DiscordHttpException)
                {
                    var dmChannelOwner = Client.CreateDM(Settings.Default.OwnerId);
                    dmChannelOwner.SendMessage("Couldn't spam to the specified user");
                    StopSpamCommand.stopSpam = true;
                }
            }
            StopSpamCommand.stopSpam = false;
            isSpamming = false;
        }
    }
}
