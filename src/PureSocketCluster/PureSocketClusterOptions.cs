using PureWebSockets;

namespace PureSocketCluster
{
    public class PureSocketClusterOptions : PureWebSocketOptions
    {
        public Creds Creds { get; set; }

        public ISerializer Serializer { get; set; }
    }
}
