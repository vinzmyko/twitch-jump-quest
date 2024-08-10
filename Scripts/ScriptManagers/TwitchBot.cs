using System;
using Godot;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;

namespace UNLTeamJumpQuest.TwitchIntegration
{
    public partial class TwitchBot : Node
    {
        [Signal]
        public delegate void MessageReceivedEventHandler(string[] messageInfo);

        public int num = 5;
        private const string TWITCH_CHANNEL_NAME = "praepollens_sub_caelum";
        // WILL NEED TO HIDE THIS LATER
        private const string TWITCH_ACCESS_TOKEN = "t6ggeo9evwsv8x8u7cxy4tef35kxv4";
        TwitchClient client;

        public static TwitchBot Instance { get; private set;}

        public override void _Ready()
        {
            if (Instance == null)
            {
                Instance = this;
                //Maybe connect when the Connect button is pressed.

                Instance.Connect(false);
            }
            else
            {
                QueueFree();
                return;
            }
        }
        public TwitchBot()
        {
            client = new TwitchClient();
        }

        internal void Connect(bool isLogging)
        {
            ConnectionCredentials creds = new ConnectionCredentials(TWITCH_CHANNEL_NAME, TWITCH_ACCESS_TOKEN);
            client.Initialize(creds, TWITCH_CHANNEL_NAME);

            GD.Print("[Bot]: Connecting...");

            if (isLogging)
                client.OnLog += Client_OnLog;

            // client.X is the event and the Client_X is the event handler method that you are subscribing to the event. += is used to attach event handler to event.
            client.OnError += Client_OnError;
            client.OnMessageReceived += Client_OnMessageReceived;
            client.OnChatCommandReceived += Client_OnChatCommandReceived;
            
            client.Connect();
            client.OnConnected += client_OnConnected;
        }

        private void client_OnConnected(object sender, OnConnectedArgs e)
        {
            GD.Print("[Bot]: Connected");
        }

        // Command starts with !
        private void Client_OnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {

            string command = e.Command.CommandText.ToLower();
            switch (command)
            {
                case "test":
                    client.SendMessage(TWITCH_CHANNEL_NAME, "wassup");
                    break;
            }

            // only works for the bot owner
            if (e.Command.ChatMessage.DisplayName == TWITCH_CHANNEL_NAME)
            {
                switch (command)
                {
                    case "hi":
                        client.SendMessage(TWITCH_CHANNEL_NAME, "Hi Boss");
                        break;
                }
            }
        }

        private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            string[] messageInfo = new string[3];
            messageInfo[0] = e.ChatMessage.DisplayName;
            messageInfo[1] = e.ChatMessage.UserId;
            messageInfo[2] = e.ChatMessage.Message;
            CallDeferred(nameof(OnMessagedReceivedDeffered), messageInfo);

            if (messageInfo[2] == "join" || messageInfo[2] == "!join")
            {
                GameManager.Instance.HandleJoinRequest(messageInfo);
            }
        }

        private void OnMessagedReceivedDeffered(string[] messageInfo)
        {
            EmitSignal(SignalName.MessageReceived, messageInfo);
        }         

        private void Client_OnError(object sender, OnErrorEventArgs e)
        {
            throw new NotImplementedException();
        }

        // Whenever a log write happens
        private void Client_OnLog(object sender, OnLogArgs e)
        {
            GD.Print(e.Data);
        }

        internal void Disconnect()
        {
            client.Disconnect();
        }

    }
}