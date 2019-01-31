using System;
using System.Collections.Generic;
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
                apiKey = "your apikey if used",
				apiSecret = "your api secret if used"
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
            _scc = new PureSocketClusterSocket("wss://yoursocketclusterserver.com/socketcluster/", opts);
            // set the channels we want to subscribe to
            var cn = _scc.CreateChannel("TRADE-GDAX--BTC--USD").Subscribe();
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
            await _scc.ConnectAsync();

            Console.ReadLine();
        }

        private static void _scc_OnFatality(string reason)
        {
			// fatality is as bad as it gets and you should probably quit here
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"Fatality: {reason}");
            Console.ResetColor();
            Console.WriteLine("");
        }

        private static void _scc_OnData(byte[] data)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Binary: {data}");
            Console.ResetColor();
            Console.WriteLine("");
        }

        private static void _scc_OnClosed(WebSocketCloseStatus reason)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"Socket Closed: {reason}");
            Console.ResetColor();
            Console.WriteLine("");
        }

        private static void _scc_OnError(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"Error: {ex}");
            Console.ResetColor();
            Console.WriteLine("");
        }

        private static void _scc_OnSendFailed(string data, Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"send failed: {data} Ex: {ex}");
            Console.ResetColor();
            Console.WriteLine("");
        }

        private static void _scc_OnStateChanged(WebSocketState newState, WebSocketState prevState)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"State changed from {prevState} to {newState}");
            Console.ResetColor();
            Console.WriteLine("");
        }

        private static void TradeData(string name, object data)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(name + ": " + data);
            Console.ResetColor();
            Console.WriteLine("");
        }

        private static void _scc_OnMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(message);
            Console.ResetColor();
            Console.WriteLine("");
        }

        private static void Scc_OnOpened()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Opened");
            Console.ResetColor();
            Console.WriteLine("");
        }
    }
}