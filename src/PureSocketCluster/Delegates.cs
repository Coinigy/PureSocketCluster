namespace PureSocketCluster
{
    public delegate void Listener(string name, object data);

    public delegate void Ackcall(string name, object error, object data);

    public delegate void AckListener(string name, object data, Ackcall ack);
}