namespace PureSocketCluster
{
    public delegate void Listener(object sender, string name, object data);

    public delegate void AckCall(object sender, string name, object error, object data);

    public delegate void AckListener(object sender, string name, object data, AckCall ack);
}