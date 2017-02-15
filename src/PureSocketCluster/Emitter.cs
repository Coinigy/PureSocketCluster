using System.Collections.Generic;

namespace PureSocketCluster
{
    public class Emitter
    {
        private readonly Dictionary<string, Listener> _singlecallbacks = new Dictionary<string, Listener>();
        private readonly Dictionary<string, AckListener> _singleackcallbacks = new Dictionary<string, AckListener>();
        private readonly Dictionary<string, Listener> _publishcallbacks = new Dictionary<string, Listener>();

        public Emitter On(string Event, Listener fn)
        {
            if (_singlecallbacks.ContainsKey(Event))
                _singlecallbacks.Remove(Event);

            _singlecallbacks.Add(Event, fn);

            return this;
        }

        public Emitter OnSubscribe(string Event, Listener fn)
        {
            if (_publishcallbacks.ContainsKey(Event))
                _publishcallbacks.Remove(Event);
            _publishcallbacks.Add(Event, fn);
            return this;
        }

        public Emitter On(string Event, AckListener fn)
        {
            if (_singleackcallbacks.ContainsKey(Event))
                _singleackcallbacks.Remove(Event);
            _singleackcallbacks.Add(Event, fn);
            return this;
        }

        public Emitter HandleEmit(string Event, object Object)
        {
            if (!_singlecallbacks.ContainsKey(Event)) return this;
            var listener = _singlecallbacks[Event];
            listener(Event, Object);
            return this;
        }

        public Emitter HandlePublish(string Event, object Object)
        {
            if (!_publishcallbacks.ContainsKey(Event)) return this;
            var listener = _publishcallbacks[Event];
            listener(Event, Object);
            return this;
        }

        public bool HasEventAck(string Event) => _singleackcallbacks.ContainsKey(Event);

        public Emitter HandleEmitAck(string Event, object Object, Ackcall ack)
        {
            if (!_singleackcallbacks.ContainsKey(Event)) return this;
            var listener = _singleackcallbacks[Event];
            listener(Event, Object, ack);
            return this;
        }
    }
}