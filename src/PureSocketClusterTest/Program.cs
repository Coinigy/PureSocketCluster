using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using PureSocketCluster;
using PureWebSockets;

namespace PureSocketClusterTest
{
    public class Program
    {
        private static PureSocketClusterSocket _scc;

        public static async Task Main(string[] args)
        {
            // input credentials if used, different systems use different auth systems this however is the most common (passing 'auth' event with your credentials)
            var creds = new Creds
            {
                ApiKey = "your apikey if used",
                ApiSecret = "your api secret if used"
            };

            // setup our options
            var opts = new PureSocketClusterOptions
            {
                Creds = creds, // set our credentials
                MyReconnectStrategy = new ReconnectStrategy(4000, 60000), // how to handle reconnects
                //Serializer = new NewtonsoftSerializer(), // the default serializer is UTF8JSON, if you have issues or want to use your own you can do so
                DebugMode = true // turn on debug mode to see detailed output
            };

            // initialize the client
            _scc = new PureSocketClusterSocket("wss://sc-02.coinigy.com/socketcluster/", opts, "MyOptionsInstanceName1");

            // set the channels we want to subscribe to
            var cn = await _scc.CreateChannel("TRADE-GDAX--BTC--USD").SubscribeAsync();
            cn.OnMessage(TradeData);

            // hook up to some events
            _scc.OnOpened += Scc_OnOpened;
            _scc.OnMessage += _scc_OnMessage;
            _scc.OnStateChanged += _scc_OnStateChanged;
            _scc.OnSendFailed += _scc_OnSendFailed;
            _scc.OnError += _scc_OnError;
            _scc.OnClosed += _scc_OnClosed;
            _scc.OnData += _scc_OnData;
            _scc.OnFatality += _scc_OnFatality;

            // connect to the server
            await _scc.ConnectAsync();

            Console.ReadLine();
        }

        private static void _scc_OnFatality(object sender, string reason)
        {
            // fatality is as bad as it gets and you should probably quit here
            OutputConsole.WriteLine($"{((PureSocketClusterSocket)sender).InstanceName} Fatality: {reason} \r\n", ConsoleColor.Magenta);
        }

        private static void _scc_OnData(object sender, byte[] data)
        {
            OutputConsole.WriteLine($"{((PureSocketClusterSocket)sender).InstanceName} Binary: {data} \r\n", ConsoleColor.White);
        }

        private static void _scc_OnClosed(object sender, WebSocketCloseStatus reason)
        {
            OutputConsole.WriteLine($"{((PureSocketClusterSocket)sender).InstanceName} Socket Closed: {reason} \r\n", ConsoleColor.DarkRed);
        }

        private static void _scc_OnError(object sender, Exception ex)
        {
            OutputConsole.WriteLine($"{((PureSocketClusterSocket)sender).InstanceName} Error: {ex} \r\n", ConsoleColor.Gray);
        }

        private static void _scc_OnSendFailed(object sender, string data, Exception ex)
        {
            OutputConsole.WriteLine($"{((PureSocketClusterSocket)sender).InstanceName} Send failed: {data} Ex: {ex} \r\n", ConsoleColor.Magenta);
        }

        private static void _scc_OnStateChanged(object sender, WebSocketState newState, WebSocketState prevState)
        {
            OutputConsole.WriteLine($"{((PureSocketClusterSocket)sender).InstanceName} State changed from {prevState} to {newState} \r\n", ConsoleColor.Yellow);
        }

        private static void TradeData(object sender, string name, object data)
        {
            OutputConsole.WriteLine($"{((PureSocketClusterSocket)sender).InstanceName} {name} : {data} \r\n", ConsoleColor.Green);
        }

        private static void _scc_OnMessage(object sender, string message)
        {
            OutputConsole.WriteLine($"{((PureSocketClusterSocket)sender).InstanceName} Message : {message} \r\n", ConsoleColor.Blue);
        }

        private static void Scc_OnOpened(object sender)
        {
            OutputConsole.WriteLine($"{((PureSocketClusterSocket)sender).InstanceName} Connection Opened \r\n", ConsoleColor.Yellow);
        }
    }
}