using System.Runtime.Serialization;

namespace PureSocketCluster
{
    public class Creds
    {
        [DataMember(Name = "apiKey")]
        public string ApiKey { get; set; }
        [DataMember(Name = "apiSecret")]
        public string ApiSecret { get; set; }
    }
}