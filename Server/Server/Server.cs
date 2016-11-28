using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Threading;

public class SynchronousSocketListener {

	// Incoming data from the client.
	public static string data = null;
	private static Thread thread;

	public static void Discover() {
		var Server = new UdpClient(8888);
		var ResponseData = Encoding.ASCII.GetBytes("SomeResponseData");

		while (true)
		{
			var ClientEp = new IPEndPoint(IPAddress.Any, 0);
			var ClientRequestData = Server.Receive(ref ClientEp);
			var ClientRequest = Encoding.ASCII.GetString(ClientRequestData);

			Console.WriteLine("Recived {0} from {1}, sending response", ClientRequest, ClientEp.Address.ToString());
			Server.Send(ResponseData, ResponseData.Length, ClientEp);
		}
	}

	public static void StartListening() {
		// Data buffer for incoming data.
		byte[] bytes = new Byte[1024];

		// Establish the local endpoint for the socket.
		// Dns.GetHostName returns the name of the 
		// host running the application.
		thread = new Thread(Discover) { IsBackground = true };
		thread.Start();
		IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
		IPAddress ipAddress = ipHostInfo.AddressList[0];
		IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

		// Create a TCP/IP socket.
		Socket listener = new Socket(AddressFamily.InterNetwork,
			SocketType.Stream, ProtocolType.Tcp );

		//String path = Directory.GetCurrentDirectory();
		//Console.WriteLine (path);
		String fileName = "../../fortress.ply";

		// Bind the socket to the local endpoint and 
		// listen for incoming connections.
		try {
			listener.Bind(localEndPoint);
			listener.Listen(10);

			// Start listening for connections.
			while (true) {
				Console.WriteLine("Waiting for a connection...");
				// Program is suspended while waiting for an incoming connection.
				Socket handler = listener.Accept();
				data = null;

				// An incoming connection needs to be processed.
				while (true) {
					bytes = new byte[1024];
					int bytesRec = handler.Receive(bytes);
					data += Encoding.ASCII.GetString(bytes,0,bytesRec);
					if (data.IndexOf("<EOF>") > -1) {
						break;
					}
				}

				// Show the data on the console.
				Console.WriteLine( "Text received : {0}", data);

				// Echo the data back to the client.
				//byte[] msg = Encoding.ASCII.GetBytes(data);

				//handler.Send(msg);
				handler.SendFile(fileName);
				handler.Shutdown(SocketShutdown.Both);
				handler.Close();
			}

		} catch (Exception e) {
			Console.WriteLine(e.ToString());
		}

		Console.WriteLine("\nPress ENTER to continue...");
		Console.Read();

	}

	public static int Main(String[] args) {
		StartListening();
		return 0;
	}
}