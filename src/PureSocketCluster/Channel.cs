namespace PureSocketCluster
{
    public class Channel
    {
        private readonly string _channelname;
        private readonly PureSocketClusterSocket _socket;

        public Channel(PureSocketClusterSocket socket, string channelName)
        {
            _socket = socket;
            _channelname = channelName;
        }

        public Channel Subscribe()
        {
            _socket.Subscribe(_channelname);
            return this;
        }

        public Channel Subscribe(Ackcall ack)
        {
            _socket.Subscribe(_channelname, ack);
            return this;
        }

        public void OnMessage(Listener listener) => _socket.OnSubscribe(_channelname, listener);

        public void Publish(object data) => _socket.Publish(_channelname, data);

        public void Publish(object data, Ackcall ack) => _socket.Publish(_channelname, data, ack);

        public void Unsubscribe()
        {
            _socket.Unsubscribe(_channelname);
            _socket.Channels.Remove(this);
        }

        public void Unsubscribe(Ackcall ack)
        {
            _socket.Unsubscribe(_channelname, ack);
            _socket.Channels.Remove(this);
        }

        public string GetChannelName() => _channelname;
    }
}