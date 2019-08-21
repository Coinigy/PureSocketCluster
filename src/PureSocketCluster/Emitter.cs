using System.Collections.Generic;

namespace PureSocketCluster
{
    public class Emitter
    {
        private readonly Dictionary<string, Listener> _singleCallbacks = new Dictionary<string, Listener>();
        private readonly Dictionary<string, AckListener> _singleAckCallbacks = new Dictionary<string, AckListener>();
        private readonly Dictionary<string, Listener> _publishCallbacks = new Dictionary<string, Listener>();

        public Emitter On(string Event, Listener fn)
        {
            if (_singleCallbacks.ContainsKey(Event))
            {
                _singleCallbacks.Remove(Event);
            }

            _singleCallbacks.Add(Event, fn);
            return this;
        }

        public Emitter OnSubscribe(string Event, Listener fn)
        {
            if (_publishCallbacks.ContainsKey(Event))
            {
                _publishCallbacks.Remove(Event);
            }

            _publishCallbacks.Add(Event, fn);
            return this;
        }

        public Emitter On(string Event, AckListener fn)
        {
            if (_singleAckCallbacks.ContainsKey(Event))
            {
                _singleAckCallbacks.Remove(Event);
            }

            _singleAckCallbacks.Add(Event, fn);
            return this;
        }

        public Emitter HandleEmit(object sender, string Event, object Object)
        {
            if (!_singleCallbacks.ContainsKey(Event))
            {
                return this;
            }

            var listener = _singleCallbacks[Event];
            listener(sender, Event, Object);
            return this;
        }

        public Emitter HandlePublish(object sender, string Event, object Object)
        {
            if (!_publishCallbacks.ContainsKey(Event))
            {
                return this;
            }

            var listener = _publishCallbacks[Event];
            listener(sender, Event, Object);
            return this;
        }

        public bool HasEventAck(string Event) => _singleAckCallbacks.ContainsKey(Event);

        public Emitter HandleEmitAck(object sender, string Event, object Object, AckCall ack)
        {
            if (!_singleAckCallbacks.ContainsKey(Event))
            {
                return this;
            }

            var listener = _singleAckCallbacks[Event];
            listener(sender, Event, Object, ack);
            return this;
        }
    }
}