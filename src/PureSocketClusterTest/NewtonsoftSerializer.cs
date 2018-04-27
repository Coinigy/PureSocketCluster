using System.Text;
using Newtonsoft.Json;
using PureSocketCluster;

namespace PureSocketClusterTest
{
    public class NewtonsoftSerializer : ISerializer
    {
	    public T Deserialize<T>(string json)
	    {
		    return JsonConvert.DeserializeObject<T>(json);
	    }

	    public byte[] Serialize(object obj)
	    {
		    // newtonsoft only gives us a string so we need to get the UTF8 bytes
			// JSON always uses UTF8
			// this is very slow and thus by default we use a different library that does not require this conversion that newtonsoft does (bytes -> string -> bytes)
			return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj));
	    }
    }
}
