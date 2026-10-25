using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Tesseract;

namespace InetBot.Modules
{
    internal class ScamDetector
    {
        HttpClient webClient = new HttpClient();
        Commands commands = new Commands();

        ulong authorID = 0;

        public void ScanImage(SocketMessage message, SocketGuild guild, DiscordSocketClient client)
        {
            string filename = DateTime.Now.Ticks.ToString() + ".jpg";

            if (message.Attachments.FirstOrDefault().Filename != "image.jpg") return;

            string foundText;

            try
            {

                Stream fileStream = webClient.GetStreamAsync(message.Attachments.FirstOrDefault().Url).Result;
                using (FileStream outputFileStream = new FileStream(filename, FileMode.Create))
                {
                    fileStream.CopyTo(outputFileStream);
                }

                string? tessdata_prefix = Environment.GetEnvironmentVariable("TESSDATA_PREFIX");
                if (tessdata_prefix == null) tessdata_prefix = "./tessdata";

                using (var engine = new TesseractEngine(tessdata_prefix, "eng", EngineMode.Default))
                using (var img = Pix.LoadFromFile(filename))
                using (var page = engine.Process(img))
                {
                    foundText = page.GetText();
                }

                Console.WriteLine(foundText);

                if (foundText.ToLower().Contains("my own cryptocurrency casino"))
                {
                    Console.WriteLine($"Deleting message from @{message.Author.Username} [{message.Author.Id}] for being a Mr. Beast scam.");
                    message.DeleteAsync();

                    ScammerBanner(message.Author, guild, client);

                    authorID = message.Author.Id;
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally 
            {
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }
            }

        }

        async Task ScammerBanner(SocketUser user, SocketGuild guild, DiscordSocketClient client)
        {
            commands._user = client.CurrentUser;
            commands.isSlashCommand = false;
            commands._modChannel = guild.GetTextChannel(commands.modChannelID);
            commands._message = null;

            if (user.Id != authorID)
            {
                Console.WriteLine($"Ding ding ding! Banning @{user.Username} [{user.Id}] for being a Mr. Beast scam.");
                await commands.HandleBanCommand(guild.GetUser(user.Id), "Compromised account. (Auto)", guild);


                EmbedBuilder bannedBuilder = new EmbedBuilder()
                    .WithAuthor($"{commands._user.Username} [{commands._user.Id}]", commands._user.GetAvatarUrl() ?? commands._user.GetDefaultAvatarUrl())
                    .WithTitle("Scammer banned!")
                    .WithDescription($"Banned @{user.Username} `[{user.Id}]` for posting a Mr. Beast scam.")
                    .WithColor(Color.Red);

                await guild.GetTextChannel(248509081789136896).SendMessageAsync(embed:bannedBuilder.Build());

            }
        }
    }
}
