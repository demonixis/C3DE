namespace C3DE.Net
{
    public struct HostData
    {
        public string Comment { get; internal set; }
        public int ConnectedPlayers { get; internal set; }
        public string GameName { get; internal set; }
        public string GUID { get; internal set; }
        public string IpAdress { get; internal set; }
        public int Port { get; internal set; }
        public int PlayerLimit { get; internal set; }
    }
}
