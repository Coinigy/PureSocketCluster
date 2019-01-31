namespace PureSocketCluster
{
    public class Channel
    {
        private readonly string _channelName;
        private readonly PureSocketClusterSocket _socket;

        public Channel(PureSocketClusterSocket socket, string channelName)
        {
            _socket = socket;
            _channelName = channelName;
        }

        public Channel Subscribe()
        {
            _socket.Subscribe(_channelName);
            return this;
        }

        public Channel Subscribe(AckCall ack)
        {
            _socket.Subscribe(_channelName, ack);
            return this;
        }

        public void OnMessage(Listener listener) => _socket.OnSubscribe(_channelName, listener);

        public void Publish(object data) => _socket.Publish(_channelName, data);

        public void Publish(object data, AckCall ack) => _socket.Publish(_channelName, data, ack);

        public void Unsubscribe()
        {
            _socket.Unsubscribe(_channelName);
            _socket.Channels.Remove(this);
        }

        public void Unsubscribe(AckCall ack)
        {
            _socket.Unsubscribe(_channelName, ack);
            _socket.Channels.Remove(this);
        }

        public string GetChannelName() => _channelName;
    }
}