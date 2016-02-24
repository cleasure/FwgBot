using System;
using Discord;
using Discord.Commands;
using Discord.Modules;

namespace C3PO.Modules
{
    class ChannelCreation : IModule
    {
        public void Install(ModuleManager manager)
        {
            //Begin creating commands with the modual manager
            manager.CreateCommands("", builder =>
            {
                // Ping
                builder.CreateCommand("ping")
                .Description("A simple command to test bot functionality")
                .Do(async e =>
                {
                    // Reply to the ping command by saying pong
                    await e.Channel.SendMessage("Pong");
                });

                // New Channel
                builder.CreateCommand("newchannel")
                .Description("Creates a new channel labeled <Your Name>'s Channel.  \r\n" +
                    "You may specify a different channel name by typing @C-3PO newchannel MyChannel.  \r\n" +
                    "If you are currently in a voice channel you will be moved into the channel once it is created.")
                .Parameter("name", ParameterType.Optional)
                .Do(async e =>
                {
                    // Determine if the channel will be named after the user or a custom title from the argument
                    var channelName = string.IsNullOrWhiteSpace(e.GetArg("name")) ? $"{e.User.Name}'s Channel" : e.GetArg("name");

                    // Create a new channel permission override that allows them to edit the channel
                    var channelPermissions = new ChannelPermissionOverrides(null, PermValue.Allow, 
                        null, null, null, null, null,
                        null, null, null, null, null,
                        null, null, null, null, null);

                    // Create the new channel
                    var newChannel = await e.Server.CreateChannel(channelName, ChannelType.Voice);

                    // Add the channel creator as an admin on the channel
                    await newChannel.AddPermissionsRule(e.User, channelPermissions);

                    // Move the channel creator to the channel if they're in any voip channel
                    await e.User.Edit(null, null, newChannel);
                });
            });
        }
    }
}
