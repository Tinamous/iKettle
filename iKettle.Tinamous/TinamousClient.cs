using System;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using NLog;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace iKettle.Tinamous
{
    public class TinamousClient : ITinamousClient
    {
        private const string TimelinePostTopic = "/Tinamous/V1/Status";
        private const string TimelineWatchTopicTemplate = "/Tinamous/V1/Status.To/{0}";

        public event EventHandler<StatusPostReceivedEventArgs> PostReceived;

        private readonly string _url;
        private readonly string _username;
        private readonly string _password;
        private MqttClient _client;
        private readonly string _clientId = Guid.NewGuid().ToString("N");
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public TinamousClient()
        {
            _url = ConfigurationManager.AppSettings["Tinamous.Url"];
            _username = ConfigurationManager.AppSettings["Tinamous.Username"];
            _password = ConfigurationManager.AppSettings["Tinamous.Password"];
        }

        public void Connect()
        {
            if (!_username.Contains("."))
            {
                throw new Exception("Username must include account as Username.AccountName to connect to MQTT");
            }

            _client = new MqttClient(_url);
            _logger.Log(LogLevel.Info, "MQTT Server: " + _url);

            _client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
            _client.ProtocolVersion = MqttProtocolVersion.Version_3_1;

            _client.Connect(_clientId, _username, _password);

            if (!_client.IsConnected)
            {
                _logger.Log(LogLevel.Error, "Failed to connect to MQTT Server: " + _url);
                throw new Exception("Client failed to connect to MQTT server.");
            }

            _logger.Log(LogLevel.Info, "Connected to MQTT Server: " + _url);
        }

        public void Disconnect()
        {
            _client.Disconnect();
        }

        public void SubscribeToPostsToUser()
        {
            string[] splitUsername = _username.Split('.');
            string topicToSubscribeTo = string.Format(TimelineWatchTopicTemplate, splitUsername[0]);
            _logger.Log(LogLevel.Info, "Subscribing to : " + topicToSubscribeTo);
            _client.Subscribe(new[] { topicToSubscribeTo }, new[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
        }

        public void PublishStatus(string message)
        {
            _logger.Log(LogLevel.Info, "Publishing to topic: " + TimelinePostTopic + ", Message:" + message);
            byte[] byteValue = Encoding.UTF8.GetBytes(message);
            _client.Publish(TimelinePostTopic, byteValue, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        }


        private void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string message = Encoding.UTF8.GetString(e.Message);
            _logger.Log(LogLevel.Info, "Mqtt Message Received: " + message);
            OnPostReceived(new StatusPostReceivedEventArgs(message));
        }

        public void Dispose()
        {
            if (_client != null)
            {
                _client.Disconnect();
                _client = null;
            }
        }

        protected virtual void OnPostReceived(StatusPostReceivedEventArgs e)
        {
            var handler = PostReceived;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
