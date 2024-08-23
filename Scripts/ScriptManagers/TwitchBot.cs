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
        [Signal]
        public delegate void TwitchClientSuccessfullyConnectedEventHandler();
        private const string TWITCH_CHANNEL_NAME = "praepollens_sub_caelum";
        private const string TWITCH_ACCESS_TOKEN = "knljbxajr6dcbk4b4jq5zzcyjepsjs";
        private bool isConnected = false;
        private bool DEBUG = false;
        TwitchClient client;
        SettingsManager settingsManager;
        public bool botConnected = false;

        public static TwitchBot Instance { get; private set;}

        public override void _Ready()
        {
            settingsManager = GetNode<SettingsManager>("/root/SettingsManager");

            if (Instance == null)
            {
                Instance = this;

                if (DEBUG)
                {
                    Instance.ConnectDebug(false);
                }
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

        internal bool Connect(bool isLogging)
        {
            ConnectionCredentials creds = new ConnectionCredentials(settingsManager.GetTwitchUserName(), settingsManager.GetTwitchAccessToken());
            client.Initialize(creds, settingsManager.GetTwitchUserName());

            GD.Print("[Bot]: Connecting...");

            if (isLogging)
                client.OnLog += Client_OnLog;

            client.OnError += Client_OnError;
            client.OnMessageReceived += Client_OnMessageReceived;
            client.OnChatCommandReceived += Client_OnChatCommandReceived;
            client.OnConnected += client_OnConnected;

            client.Connect();

            return client.IsConnected;
        }

        internal bool ConnectDebug(bool isLogging)
        {
            ConnectionCredentials creds = new ConnectionCredentials(TWITCH_CHANNEL_NAME, TWITCH_ACCESS_TOKEN);
            client.Initialize(creds, TWITCH_CHANNEL_NAME);

            GD.Print("[Bot]: Connecting...");

            if (isLogging)
                client.OnLog += Client_OnLog;

            client.OnError += Client_OnError;
            client.OnMessageReceived += Client_OnMessageReceived;
            client.OnChatCommandReceived += Client_OnChatCommandReceived;
            client.OnConnected += client_OnConnected;

            client.Connect();

            return isConnected;
        }

        internal bool ConnectFailSafe(bool isLogging)
        {
            // Dispose of the old client if necessary
            if (client != null)
            {
                client.OnLog -= Client_OnLog;
                client.OnError -= Client_OnError;
                client.OnMessageReceived -= Client_OnMessageReceived;
                client.OnConnected -= client_OnConnected;
            }

            // Create a new client instance
            client = new TwitchClient();
            ConnectionCredentials creds = new ConnectionCredentials(settingsManager.GetTwitchUserName(), settingsManager.GetTwitchAccessToken());
            client.Initialize(creds, settingsManager.GetTwitchUserName());

            GD.Print("[Bot]: Connecting...");

            if (isLogging)
                client.OnLog += Client_OnLog;

            client.OnError += Client_OnError;
            client.OnMessageReceived += Client_OnMessageReceived;
            client.OnChatCommandReceived += Client_OnChatCommandReceived;
            client.OnConnected += client_OnConnected;

            client.Connect();
            return true;
        }

        private void client_OnConnected(object sender, OnConnectedArgs e)
        {
            CallDeferred(nameof(OnTwitchClientSuccessDeffered));
            botConnected = true;
            GD.Print("[Bot]: Connected");
        }

        // Command starts with !
        private void Client_OnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {
            string command = e.Command.CommandText.ToLower();
            switch (command)
            {
                case "controls":
                    client.SendMessage(settingsManager.GetTwitchUserName(), 
                        "tldr: https://imgur.com/a/controls-TMPwPqw " +
                        "<command> <degrees> <power>, if power is ommited it will be defaulted as 100%.\n " +
                        "Degrees are from 0 - 90 using 'l' & 'r' or -90 to 90 using 'j', power is 0 to 100. Values will be clamped to follow this rule.\n" +
                        "Example commands:\n" +
                        "Jump straight up: 'u', 'j0', 'l0', 'r0'\n" +
                        "45 degrees to the left: 'l45', 'j-45'\n" +
                        "45 degress to the right: 'r45', 'j45'");
                    break;
                case "teams":
                    client.SendMessage(settingsManager.GetTwitchUserName(), "T4T, GGEL, CHD, BIS, ICW, HVC, AST, FF, CB");
                    break;
                case "help":
                    client.SendMessage(settingsManager.GetTwitchUserName(), "Commands: !controls, !teams");
                    break;
            }
            // only works for the bot owner
            if (e.Command.ChatMessage.DisplayName == settingsManager.GetTwitchUserName())
            {
                switch (command)
                {
                    case "hi":
                        client.SendMessage(settingsManager.GetTwitchUserName(), "Hi Boss");
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

            var (isValid, targetTeam) = MessageParser.ParseTeamJoin(messageInfo[2], settingsManager.UNLTeams);
            if (isValid)
            {
                GameManager.Instance.HandleJoinRequest(messageInfo, targetTeam);
            }
        }

        private void OnMessagedReceivedDeffered(string[] messageInfo)
        {
            EmitSignal(SignalName.MessageReceived, messageInfo);
        }         

        private void OnTwitchClientSuccessDeffered()
        {
            EmitSignal(SignalName.TwitchClientSuccessfullyConnected);
        }

        private void Client_OnError(object sender, OnErrorEventArgs e)
        {
            GD.Print(e);
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