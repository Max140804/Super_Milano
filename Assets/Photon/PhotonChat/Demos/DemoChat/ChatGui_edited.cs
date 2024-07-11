// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Exit Games GmbH"/>
// <summary>Demo code for Photon Chat in Unity.</summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Photon.Chat;
using Photon.Realtime;
using AuthenticationValues = Photon.Chat.AuthenticationValues;
#if PHOTON_UNITY_NETWORKING
using Photon.Pun;
#endif


namespace Photon.Chat.Demo
{
    /// <summary>
    /// This simple Chat UI demonstrate basics usages of the Chat Api
    /// </summary>
    /// <remarks>
    /// The ChatClient basically lets you create any number of channels.
    ///
    /// some friends are already set in the Chat demo "DemoChat-Scene", 'Joe', 'Jane' and 'Bob', simply log with them so that you can see the status changes in the Interface
    ///
    /// Workflow:
    /// Create ChatClient, Connect to a server with your AppID, Authenticate the user (apply a unique name,)
    /// and subscribe to some channels.
    /// Subscribe a channel before you publish to that channel!
    ///
    ///
    /// Note:
    /// Don't forget to call ChatClient.Service() on Update to keep the Chatclient operational.
    /// </remarks>
    public class ChatGui_edited : MonoBehaviour, IChatClientListener
    {
        public string UserName { get; set; }

        public ChatClient chatClient;

        #if !PHOTON_UNITY_NETWORKING
        public ChatAppSettings ChatAppSettings
        {
            get { return this.chatAppSettings; }
        }

        [SerializeField]
        #endif
        protected internal ChatAppSettings chatAppSettings;

        public InputField InputFieldChat;   // set in inspector


        private readonly Dictionary<string, Toggle> channelToggles = new Dictionary<string, Toggle>();

        public Text CurrentChannelText;     // set in inspector


        public void Start()
        {
            if (string.IsNullOrEmpty(this.UserName))
            {
                this.UserName = PhotonNetwork.NickName;
            }

            #if PHOTON_UNITY_NETWORKING
            this.chatAppSettings = PhotonNetwork.PhotonServerSettings.AppSettings.GetChatSettings();
            #endif

            bool appIdPresent = !string.IsNullOrEmpty(this.chatAppSettings.AppIdChat);

            if (!appIdPresent)
            {
                Debug.LogError("You need to set the chat app ID in the PhotonServerSettings file in order to continue.");
            }
            Connect();

        }

        public void Connect()
        {
            this.chatClient = new ChatClient(this);
            #if !UNITY_WEBGL
            this.chatClient.UseBackgroundWorkerForSending = true;
            #endif
            this.chatClient.AuthValues = new AuthenticationValues(this.UserName);
            this.chatClient.ConnectUsingSettings(this.chatAppSettings);
        }

        /// <summary>To avoid that the Editor becomes unresponsive, disconnect all Photon connections in OnDestroy.</summary>
        public void OnDestroy()
        {
            if (this.chatClient != null)
            {
                this.chatClient.Disconnect();
            }
        }

        /// <summary>To avoid that the Editor becomes unresponsive, disconnect all Photon connections in OnApplicationQuit.</summary>
        public void OnApplicationQuit()
        {
            if (this.chatClient != null)
            {
                this.chatClient.Disconnect();
            }
        }

        public void Update()
        {
            if (this.chatClient != null)
            {
                this.chatClient.Service(); // make sure to call this regularly! it limits effort internally, so calling often is ok!
            }
        }


        public void OnEnterSend()
        {
            if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter))
            {
                SendChatMessage(InputFieldChat.text);
                InputFieldChat.text = "";
            }
        }

        public void OnClickSend()
        {
            if (this.InputFieldChat != null)
            {
                this.SendChatMessage(this.InputFieldChat.text);
                this.InputFieldChat.text = "";
            }
        }


        public int TestLength = 2048;
        private byte[] testBytes = new byte[2048];

        private void SendChatMessage(string inputLine)
        {
            if (string.IsNullOrEmpty(inputLine))
            {
                return;
            }


            chatClient.PublishMessage(PhotonNetwork.CurrentRoom.Name, inputLine);
                
        }


        public void DebugReturn(ExitGames.Client.Photon.DebugLevel level, string message)
        {
            if (level == ExitGames.Client.Photon.DebugLevel.ERROR)
            {
                Debug.LogError(message);
            }
            else if (level == ExitGames.Client.Photon.DebugLevel.WARNING)
            {
                Debug.LogWarning(message);
            }
            else
            {
                Debug.Log(message);
            }
        }

        public void OnConnected()
        {
                chatClient.Subscribe(PhotonNetwork.CurrentRoom.Name);
        }

        public void OnDisconnected()
        {
            Debug.Log("OnDisconnected()");
        }

        public void OnChatStateChange(ChatState state)
        {
        }

        public void OnSubscribed(string[] channels, bool[] results)
        {
            Debug.Log("OnSubscribed: " + string.Join(", ", channels));
        }

    
        public void OnGetMessages(string channelName, string[] senders, object[] messages)
        {
            ShowChannel(PhotonNetwork.CurrentRoom.Name);

        }

        public void OnPrivateMessage(string sender, object message, string channelName)
        {

        }

        public void ShowChannel(string channelName)
        {
            if (string.IsNullOrEmpty(channelName))
            {
                return;
            }

            ChatChannel channel = null;
            bool found = this.chatClient.TryGetChannel(channelName, out channel);
            if (!found)
            {
                Debug.Log("ShowChannel failed to find channel: " + channelName);
                return;
            }

            this.CurrentChannelText.text = channel.ToStringMessages();

            foreach (KeyValuePair<string, Toggle> pair in this.channelToggles)
            {
                pair.Value.isOn = pair.Key == channelName ? true : false;
            }
        }

        /// <summary>
        /// New status of another user (you get updates for users set in your friends list).
        /// </summary>
        /// <param name="user">Name of the user.</param>
        /// <param name="status">New status of that user.</param>
        /// <param name="gotMessage">True if the status contains a message you should cache locally. False: This status update does not include a
        /// message (keep any you have).</param>
        /// <param name="message">Message that user set.</param>
        public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
        {

        }

        public void OnUserSubscribed(string channel, string user)
        {
            Debug.LogFormat("OnUserSubscribed: channel=\"{0}\" userId=\"{1}\"", channel, user);
        }

        public void OnUserUnsubscribed(string channel, string user)
        {
            Debug.LogFormat("OnUserUnsubscribed: channel=\"{0}\" userId=\"{1}\"", channel, user);
        }

        /// <inheritdoc />
        public void OnChannelPropertiesChanged(string channel, string userId, Dictionary<object, object> properties)
        {
            Debug.LogFormat("OnChannelPropertiesChanged: {0} by {1}. Props: {2}.", channel, userId, Extensions.ToStringFull(properties));
        }

        public void OnUserPropertiesChanged(string channel, string targetUserId, string senderUserId, Dictionary<object, object> properties)
        {
            Debug.LogFormat("OnUserPropertiesChanged: (channel:{0} user:{1}) by {2}. Props: {3}.", channel, targetUserId, senderUserId, Extensions.ToStringFull(properties));
        }

        /// <inheritdoc />
        public void OnErrorInfo(string channel, string error, object data)
        {
            Debug.LogFormat("OnErrorInfo for channel {0}. Error: {1} Data: {2}", channel, error, data);
        }

        public void AddMessageToSelectedChannel(string msg)
        {
            ChatChannel channel = null;
            bool found = this.chatClient.TryGetChannel(PhotonNetwork.CurrentRoom.Name, out channel);
            if (!found)
            {
                Debug.Log("AddMessageToSelectedChannel failed to find channel: " + PhotonNetwork.CurrentRoom.Name);
                return;
            }

            if (channel != null)
            {
                channel.Add("Bot", msg,0); //TODO: how to use msgID?
            }
        }


        public void OnUnsubscribed(string[] channels)
        {
            throw new NotImplementedException();
        }
    }
}