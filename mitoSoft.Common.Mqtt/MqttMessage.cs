namespace mitoSoft.homeNet.Core.Mqtt
{
    public class MqttMessage
    {
        public MqttMessage(string topic, string message)
        {
            this.Topic = topic;
            this.Message = message;
        }

        public string Topic { get; set; }

        public string Message { get; set; }

        public override string ToString()
        {
            return this.Topic + ">" + this.Message;
        }
    }
}