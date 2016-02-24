using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using C3PO.Modules;
using C3PO.Net;
using Discord;
using Discord.Commands;
using Discord.Modules;

namespace C3PO
{
    class Program
    {
        static DiscordClient _client;
        static Timer _timer;

        static void Main(string[] args)
        {
            StartTimer();
            //Create the Discord Client and define our startup variables
            _client = new DiscordClient(builder =>
            {
                builder.AppName = "C3PO";
                builder.AppUrl = "http://www.freeworldsgaming.com";
                builder.AppVersion = "1.0.0";
                builder.MessageCacheSize = 0;
                builder.UsePermissionsCache = false;
                builder.EnablePreUpdateEvents = true;
                builder.LogLevel = LogSeverity.Info;
                builder.LogHandler = LogHandler;
            })

            .UsingCommands(builder =>
            {
                builder.AllowMentionPrefix = true;
                builder.HelpMode = HelpMode.Public;
                builder.ExecuteHandler = ExecuteHandler;
                builder.ErrorHandler = ErrorHandler;
            });

            // Create a new module service to hold our command module
            var modules = _client.AddService(new ModuleService());

            // Install modules
            _client.Modules().Add(new ChannelCreation(), "Channel Creation", ModuleFilter.None);

            //Convert our sync method to an async one and block the Main function until the bot disconnects
            _client.ExecuteAndWait(async () =>
            {
                GlobalSettings.Load();
                //Connect to the Discord server using our email and password
                await _client.Connect(GlobalSettings.Discord.Email, GlobalSettings.Discord.Password);
                _client.SetGame("FWG Bot");
            });
        }

        // Create a timer to check for empty channels every 60 seconds
        static void StartTimer()
        {
            _timer = new Timer(60000);
            _timer.Elapsed += _timer_Elapsed;
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        static void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ChannelSweeper();
        }

        static void ErrorHandler(object sender, CommandErrorEventArgs e)
        {
            string msg = e.Exception?.GetBaseException().Message;
            if (msg == null) //No exception - show a generic message
            {
                switch (e.ErrorType)
                {
                    case CommandErrorType.Exception:
                        //msg = "Unknown error.";
                        break;
                    case CommandErrorType.BadPermissions:
                        msg = "You do not have permission to run this command.";
                        break;
                    case CommandErrorType.BadArgCount:
                        //msg = "You provided the incorrect number of arguments for this command.";
                        break;
                    case CommandErrorType.InvalidInput:
                        //msg = "Unable to parse your command, please check your input.";
                        break;
                    case CommandErrorType.UnknownCommand:
                        //msg = "Unknown command.";
                        break;
                }
            }
            if (msg != null)
            {
                _client.Log.Error("Command", msg);
            }
        }

        // Write successful command executions to the console
        static void ExecuteHandler(object sender, CommandEventArgs e)
        {
            _client.Log.Info("Command", $"{e.Command.Text} ({e.User.Name})");
        }

        static void LogHandler(object sender, LogMessageEventArgs e)
        {
            //Color
            ConsoleColor color;
            switch (e.Severity)
            {
                case LogSeverity.Error: color = ConsoleColor.Red; break;
                case LogSeverity.Warning: color = ConsoleColor.Yellow; break;
                case LogSeverity.Info: color = ConsoleColor.White; break;
                case LogSeverity.Verbose: color = ConsoleColor.Gray; break;
                case LogSeverity.Debug: default: color = ConsoleColor.DarkGray; break;
            }

            //Exception
            string exMessage;
            Exception ex = e.Exception;
            if (ex != null)
            {
                while (ex is AggregateException && ex.InnerException != null)
                    ex = ex.InnerException;
                exMessage = ex.Message;
            }
            else
                exMessage = null;

            //Source
            string sourceName = e.Source;

            //Text
            string text;
            if (e.Message == null)
            {
                text = exMessage ?? "";
                exMessage = null;
            }
            else
                text = e.Message;

            //Build message
            StringBuilder builder = new StringBuilder(text.Length + (sourceName?.Length ?? 0) + (exMessage?.Length ?? 0) + 5);
            if (sourceName != null)
            {
                builder.Append('[');
                builder.Append(sourceName);
                builder.Append("] ");
            }

            for (int i = 0; i < text.Length; i++)
            {
                //Strip control chars
                char c = text[i];
                if (!char.IsControl(c))
                    builder.Append(c);
            }
            if (exMessage != null)
            {
                builder.Append(": ");
                builder.Append(exMessage);
            }

            text = builder.ToString();
            //if (e.Severity <= LogSeverity.Info)
            //{
            Console.ForegroundColor = color;
            Console.WriteLine(text);
        }

        static async void ChannelSweeper()
        {
            // Get the list of all channels on the server
            var channelList = _client.GetServer(104803223163899904).AllChannels;

            // The channels that you do not want to be deleted even if empty
            var whitelist = new List<ulong>
            {
                104803223231008768, //Lobby
                150945131460165632, //Gaming Channel 1
                150945184618643456, //Gaming Channel 2
                150945221398495232, //Gaming Channel 3
                150945252411179010, //Gaming Channel 4
                150945295729950721, //Gaming Channel 5
                144179373111508992, //Staff
                150733011896238080, //----------------------
                150731579101020160, //Titanfall
                144179526597869568, //Eve Online
                144179560504623105, //Freeworlds Cantina
                144179611285192704, //Hermit Central
                150733093987287040, //Potato Whiskey Tavern
                150733368416403456, //Shifty Channel
                144179688133099521, //E10
                144179712342753280, //LLJK
                144202944336625664, //🎶 Tunes 🎶
                144177109521137664, //💤 AFK
                150733314813067264  //-----Custom-----
            };

            foreach (var channel in channelList)
            {
                if (channel.Type == ChannelType.Voice)
                {
                    if (!whitelist.Contains(channel.Id))
                    {
                        if (!channel.Users.Any())
                        {
                            Console.WriteLine("Empty channel found, deleting");
                            await channel.Delete();
                        }
                    }   
                }
            }
        }
    }
}
