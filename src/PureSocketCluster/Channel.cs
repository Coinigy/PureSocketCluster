using System.Threading.Tasks;

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

        public async Task<Channel> SubscribeAsync()
        {
            await _socket.SubscribeAsync(_channelName);
            return this;
        }

        public Channel Subscribe(AckCall ack)
        {
            _socket.Subscribe(_channelName, ack);
            return this;
        }

        public async Task<Channel> SubscribeAsync(AckCall ack)
        {
            await _socket.SubscribeAsync(_channelName, ack);
            return this;
        }

        public void OnMessage(Listener listener) => _socket.OnSubscribe(_channelName, listener);

        public void Publish(object data) => _socket.Publish(_channelName, data);

        public Task PublishAsync(object data) => _socket.PublishAsync(_channelName, data);

        public void Publish(object data, AckCall ack) => _socket.Publish(_channelName, data, ack);

        public Task PublishAsync(object data, AckCall ack) => _socket.PublishAsync(_channelName, data, ack);

        public void Unsubscribe()
        {
            _socket.Unsubscribe(_channelName);
            _socket.Channels.Remove(this);
        }

        public async Task UnsubscribeAsync()
        {
            await _socket.UnsubscribeAsync(_channelName);
            _socket.Channels.Remove(this);
        }

        public void Unsubscribe(AckCall ack)
        {
            _socket.Unsubscribe(_channelName, ack);
            _socket.Channels.Remove(this);
        }

        public async Task UnsubscribeAsync(AckCall ack)
        {
            await _socket.UnsubscribeAsync(_channelName, ack);
            _socket.Channels.Remove(this);
        }

        public string GetChannelName() => _channelName;
    }
}