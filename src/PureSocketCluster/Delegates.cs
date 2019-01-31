namespace PureSocketCluster
{
    public delegate void Listener(string name, object data);

    public delegate void AckCall(string name, object error, object data);

    public delegate void AckListener(string name, object data, AckCall ack);
}