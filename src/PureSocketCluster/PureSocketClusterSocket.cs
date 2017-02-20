/*
 * Author: ByronP
 * Date: 1/15/2017
 * Coinigy Inc. Coinigy.com
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PureWebSockets;

namespace PureSocketCluster
{
    public class PureSocketClusterSocket : Emitter, IDisposable
    {
        public string Id;
        public JsonSerializerSettings SerializerSettings { get; set; } = new JsonSerializerSettings();

        private readonly PureWebSocket _socket;
        private long _counter;
        private string _authToken;
        internal List<Channel> Channels;
        private readonly Dictionary<long?, object[]> _acks;
        private readonly Creds _creds;

        public event Closed OnClosed;
        public event Data OnData;
        public event Error OnError;
        public event Fatality OnFatality;
        public event Message OnMessage;
        public event Opened OnOpened;
        public event SendFailed OnSendFailed;
        public event StateChanged OnStateChanged;

        public int SocketSendQueueLength => _socket?.SendQueueLength ?? 0;

        public ushort SocketSendDelay
        {
            get { return _socket?.SendDelay ?? 0; }
            set
            {
                if (_socket != null)
                    _socket.SendDelay = value;
            }
        }

        public PureSocketClusterSocket(string url)
        {
            _socket = new PureWebSocket(url);
            _counter = 0;
            Channels = new List<Channel>();
            _acks = new Dictionary<long?, object[]>();


            SetupEvents();
        }

        public PureSocketClusterSocket(string url, string authToken)
        {
            _authToken = authToken;
            _socket = new PureWebSocket(url);
            _counter = 0;
            Channels = new List<Channel>();
            _acks = new Dictionary<long?, object[]>();


            SetupEvents();
        }

        public PureSocketClusterSocket(string url, Creds creds)
        {
            _creds = creds;
            _socket = new PureWebSocket(url);
            _counter = 0;
            Channels = new List<Channel>();
            _acks = new Dictionary<long?, object[]>();


            SetupEvents();
        }

        public PureSocketClusterSocket(string url, ReconnectStrategy reconnectStrategy)
        {
            _socket = new PureWebSocket(url, reconnectStrategy);
            _counter = 0;
            Channels = new List<Channel>();
            _acks = new Dictionary<long?, object[]>();

            SetupEvents();
        }

        public PureSocketClusterSocket(string url, ReconnectStrategy reconnectStrategy, string authToken)
        {
            _authToken = authToken;
            _socket = new PureWebSocket(url, reconnectStrategy);
            _counter = 0;
            Channels = new List<Channel>();
            _acks = new Dictionary<long?, object[]>();

            SetupEvents();
        }

        public PureSocketClusterSocket(string url, ReconnectStrategy reconnectStrategy, Creds creds)
        {
            _creds = creds;
            _socket = new PureWebSocket(url, reconnectStrategy);
            _counter = 0;
            Channels = new List<Channel>();
            _acks = new Dictionary<long?, object[]>();

            SetupEvents();
        }

        private void SetupEvents()
        {
            _socket.OnOpened += socket_OnOpened;
            _socket.OnError += socket_OnError;
            _socket.OnClosed += socket_OnClosed;
            _socket.OnMessage += socket_OnMessage;
            _socket.OnData += socket_OnData;
            _socket.OnFatality += socket_OnFatality;
            _socket.OnSendFailed += socket_OnSendFailed;
            _socket.OnStateChanged += socket_OnStateChanged;
        }

        private void socket_OnStateChanged(WebSocketState newState, WebSocketState prevState)
        {
            OnStateChanged?.Invoke(newState, prevState);
        }

        private void socket_OnSendFailed(string data, Exception ex)
        {
            OnSendFailed?.Invoke(data, ex);
        }

        private void socket_OnFatality(string reason)
        {
            OnFatality?.Invoke(reason);
        }

        private void socket_OnData(byte[] data)
        {
            OnData?.Invoke(data);
        }

        private void socket_OnMessage(string message)
        {
            OnMessage?.Invoke(message);
            //TODO: this can be optimized more
            if (message == "#1")
            {
                _socket.Send("#2");
                return;
            }
            if (message == "1")
            {
                _socket.Send("2");
                return;
            }
            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(message);

            object dataobject;
            if (!dict.TryGetValue("data", out dataobject)) return;
            object trid = null;
            dict.TryGetValue("rid", out trid);
            object tcid = null;
            dict.TryGetValue("cid", out tcid);
            object tstrEvent = "";
            dict.TryGetValue("event", out tstrEvent);

            var rid = (long?) trid;
            var cid = (long?) tcid;
            var strEvent = (string) tstrEvent;

            switch (Parser.Parse(dataobject, rid, cid, strEvent))
            {
                case Parser.ParseResult.ISAUTHENTICATED:
                    Id = (string) ((JObject) dataobject).GetValue("id");
                    //_listener.OnAuthentication(this, (bool)((JObject)dataobject).GetValue("isAuthenticated"));
                    SubscribeChannels();
                    break;
                case Parser.ParseResult.PUBLISH:
                    HandlePublish((string) ((JObject) dataobject).GetValue("channel"),
                        ((JObject) dataobject).GetValue("data"));
                    break;
                case Parser.ParseResult.REMOVETOKEN:
                    SetAuthToken(null);
                    break;
                case Parser.ParseResult.SETTOKEN:
                    SetAuthToken((string) ((JObject) dataobject).GetValue("token"));
                    break;
                case Parser.ParseResult.EVENT:

                    if (HasEventAck(strEvent))
                        HandleEmitAck(strEvent, dataobject, Ack(cid));
                    else
                        HandleEmit(strEvent, dataobject);
                    break;
                case Parser.ParseResult.ACKRECEIVE:
                    if (_acks.ContainsKey(rid))
                    {
                        var Object = _acks[rid];
                        _acks.Remove(rid);
                        if (Object != null)
                        {
                            var fn = (Ackcall) Object[1];
                            if (fn != null)
                            {
                                object err;
                                dict.TryGetValue("error", out err);
                                object dat;
                                dict.TryGetValue("data", out dat);

                                fn((string) Object[0], err, dat);
                            }
                            else
                            {
                                Console.WriteLine("Ack function is null");
                            }
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void socket_OnClosed(WebSocketCloseStatus reason)
        {
            OnClosed?.Invoke(reason);
        }

        private void socket_OnError(Exception ex)
        {
            OnError?.Invoke(ex);
        }

        private void socket_OnOpened()
        {
            _counter = 0;
            var authobject = new Dictionary<string, object>
            {
                {"event", "#handshake"},
                {"data", new Dictionary<string, object> {{"authToken", _authToken}}},
                {"cid", Interlocked.Increment(ref _counter)}
            };
            var json = JsonConvert.SerializeObject(authobject, Formatting.Indented, SerializerSettings);

            _socket.Send(json);

            if (_creds != null)
            {
                Emit("auth", _creds);
                Task.Delay(500).Wait();
            }

            OnOpened?.Invoke();
        }

        // TODO: invalid ssl bypass
        //public void SetSslCertVerification(bool value)
        //{
        //    //_socket.AllowUnstrustedCertificate = value;
        //}

        public Channel CreateChannel(string name)
        {
            var channel = new Channel(this, name);
            Channels.Add(channel);
            return channel;
        }

        public List<Channel> GetChannels()
        {
            return Channels;
        }

        public Channel GetChannelByName(string name)
        {
            return Channels.FirstOrDefault(channel => channel.GetChannelName().Equals(name));
        }

        private void SubscribeChannels()
        {
            foreach (var channel in Channels)
                channel.Subscribe();
        }

        public void SetAuthToken(string token)
        {
            _authToken = token;
        }

        public void Connect()
        {
            try
            {
                _socket.Connect();
            }
            catch (Exception ex)
            {
                socket_OnError(ex);
                throw;
            }
        }

        public void Disconnect()
        {
            _socket.Disconnect();
        }

        public Ackcall Ack(long? cid)
        {
            return (name, error, data) =>
            {
                var dataObject = new Dictionary<string, object> {{"error", error}, {"data", data}, {"rid", cid}};
                var json = JsonConvert.SerializeObject(dataObject, Formatting.Indented, SerializerSettings);
                _socket.Send(json);
            };
        }

        public bool Emit(string Event, object Object)
        {
            var eventObject = new Dictionary<string, object> {{"event", Event}, {"data", Object}};
            var json = JsonConvert.SerializeObject(eventObject, Formatting.Indented, SerializerSettings);
            return _socket.Send(json);
        }

        public bool Emit(string Event, object Object, Ackcall ack)
        {
            var count = Interlocked.Increment(ref _counter);
            var eventObject = new Dictionary<string, object> {{"event", Event}, {"data", Object}, {"cid", count}};
            _acks.Add(count, GetAckObject(Event, ack));
            var json = JsonConvert.SerializeObject(eventObject, Formatting.Indented, SerializerSettings);
            return _socket.Send(json);
        }

        public bool Subscribe(string channel)
        {
            var subscribeObject = new Dictionary<string, object>
            {
                {"event", "#subscribe"},
                {"data", new Dictionary<string, string> {{"channel", channel}}},
                {"cid", Interlocked.Increment(ref _counter)}
            };
            var json = JsonConvert.SerializeObject(subscribeObject, Formatting.Indented, SerializerSettings);
            return _socket.Send(json);
        }

        public bool Subscribe(string channel, Ackcall ack)
        {
            var count = Interlocked.Increment(ref _counter);
            var subscribeObject = new Dictionary<string, object>
            {
                {"event", "#subscribe"},
                {"data", new Dictionary<string, string> {{"channel", channel}}},
                {"cid", count}
            };
            _acks.Add(count, GetAckObject(channel, ack));
            var json = JsonConvert.SerializeObject(subscribeObject, Formatting.Indented, SerializerSettings);
            return _socket.Send(json);
        }

        public bool Unsubscribe(string channel)
        {
            var subscribeObject = new Dictionary<string, object>
            {
                {"event", "#unsubscribe"},
                {"data", channel},
                {"cid", Interlocked.Increment(ref _counter)}
            };
            var json = JsonConvert.SerializeObject(subscribeObject, Formatting.Indented, SerializerSettings);
            return _socket.Send(json);
        }

        public bool Unsubscribe(string channel, Ackcall ack)
        {
            var count = Interlocked.Increment(ref _counter);
            var subscribeObject = new Dictionary<string, object>
            {
                {"event", "#unsubscribe"},
                {"data", channel},
                {"cid", count}
            };
            _acks.Add(count, GetAckObject(channel, ack));
            var json = JsonConvert.SerializeObject(subscribeObject, Formatting.Indented, SerializerSettings);
            return _socket.Send(json);
        }

        public bool Publish(string channel, object data)
        {
            var publishObject = new Dictionary<string, object>
            {
                {"event", "#publish"},
                {"data", new Dictionary<string, object> {{"channel", channel}, {"data", data}}},
                {"cid", Interlocked.Increment(ref _counter)}
            };
            var json = JsonConvert.SerializeObject(publishObject, Formatting.Indented, SerializerSettings);
            return _socket.Send(json);
        }

        public bool Publish(string channel, object data, Ackcall ack)
        {
            var count = Interlocked.Increment(ref _counter);
            var publishObject = new Dictionary<string, object>
            {
                {"event", "#publish"},
                {"data", new Dictionary<string, object> {{"channel", channel}, {"data", data}}},
                {"cid", count}
            };
            _acks.Add(count, GetAckObject(channel, ack));
            var json = JsonConvert.SerializeObject(publishObject, Formatting.Indented, SerializerSettings);
            return _socket.Send(json);
        }

        private static object[] GetAckObject(string Event, Ackcall ack)
        {
            object[] Object = {Event, ack};
            return Object;
        }

        #region IDisposable Support

        private bool disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _socket.Disconnect();
                    Task.Delay(1000).Wait();
                    _socket.Dispose();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        #endregion
    }
}