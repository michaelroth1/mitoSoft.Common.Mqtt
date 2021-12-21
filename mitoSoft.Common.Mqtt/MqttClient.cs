using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace mitoSoft.Common.Mqtt
{
    public class MqttClient
    {
        public event Action<MqttMessage> MessageReceived;

        private string _clientId;

        private uPLibrary.Networking.M2Mqtt.MqttClient _client;

        public MqttClient(string hostName)
        {
            this.HostName = hostName;
        }

        public string HostName { get; set; }

        public List<string> Topics { get; set; }

        public bool IsConnected => _client?.IsConnected ?? false;

        public bool Connect(string clientId)
        {
            try
            {
                _clientId = clientId;
                _client = new uPLibrary.Networking.M2Mqtt.MqttClient(this.HostName);

                if (this.Topics != null && this.Topics.Count > 0)
                {
                    var qos = new List<byte>();

                    foreach (var topic in this.Topics)
                    {
                        qos.Add(2);
                    }

                    _client.Subscribe(this.Topics.ToArray(), qos.ToArray());

                    _client.MqttMsgPublishReceived += this.OnMessageReceived;
                }

                _client.ConnectionClosed += this.OnConnectionClosed;

                _client.Connect(_clientId);

                return true;
            }
            catch (System.AggregateException)
            {
                throw;
            }
            catch
            {
                this.Disconnect();

                Task.Run(() => this.Reconnect());

                return false;
            }
        }

        public void Disconnect()
        {
            if (_client == null)
            {
                return;
            }

            this.UnregisterEvents();

            if (!this.IsConnected)
            {
                return;
            }

            this._client.Disconnect();
        }

        private void UnregisterEvents()
        {
            try
            {
                _client.MqttMsgPublishReceived -= this.OnMessageReceived;
            }
            catch { }
            try
            {
                _client.ConnectionClosed -= this.OnConnectionClosed;
            }
            catch { }
        }

        private void OnMessageReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        {
            var message = System.Text.Encoding.ASCII.GetString(e.Message);

            MessageReceived?.Invoke(new MqttMessage(e.Topic, message));
        }

        public void Publish(string topic, string message, int qosLevel, bool retain)
        {
            if (this.IsConnected)
            {
                this._client.Publish(topic, System.Text.Encoding.ASCII.GetBytes(message), byte.Parse(qosLevel.ToString()), retain);
            }
        }

        private void OnConnectionClosed(object sender, System.EventArgs e)
        {
            this.Disconnect();

            Task.Run(() => this.Reconnect());
        }

        private void Reconnect()
        {
            Task.Delay(5000);

            if (!_client.IsConnected)
            {
                this.Connect(_clientId);
            }
        }
    }
}