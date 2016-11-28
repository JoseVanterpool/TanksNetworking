using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class Client : MonoBehaviour {

	public static IPEndPoint Probe() {
		var Client = new UdpClient();
		var RequestData = Encoding.ASCII.GetBytes("SomeRequestData");
		var ServerEp = new IPEndPoint(IPAddress.Any, 0);

		Client.EnableBroadcast = true;
		Client.Send(RequestData, RequestData.Length, new IPEndPoint(IPAddress.Broadcast, 8888));

		var ServerResponseData = Client.Receive(ref ServerEp);
		var ServerResponse = Encoding.ASCII.GetString(ServerResponseData);
		Debug.Log(String.Format("Recived {0} from {1}", ServerResponse, ServerEp.Address.ToString()));

		Client.Close();
		return ServerEp;
	}

	public static void StartClient() {
		// Data buffer for incoming data.
		byte[] bytes = new byte[1024];

		// Connect to a remote device.
		try {
			// Establish the remote endpoint for the socket.
			// This example uses port 11000 on the local computer.
			var remoteEP = Probe();
			remoteEP.Port = 11000;
//			IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
//			IPAddress ipAddress = ipHostInfo.AddressList[0];
//			IPEndPoint remoteEP = new IPEndPoint(ipAddress,11000);

			// Create a TCP/IP  socket.
			Socket sender = new Socket(AddressFamily.InterNetwork, 
				SocketType.Stream, ProtocolType.Tcp );



			// Connect the socket to the remote endpoint. Catch any errors.
			try {
				sender.Connect(remoteEP);

				Debug.Log(String.Format("Socket connected to {0}",
					sender.RemoteEndPoint.ToString()));

				// Encode the data string into a byte array.
				byte[] msg = Encoding.ASCII.GetBytes("This is a test<EOF>");

				// Send the data through the socket.
				int bytesSent = sender.Send(msg);

				// Receive the response from the remote device.
				//int bytesRec = sender.Receive(bytes);
				//Debug.Log(String.Format("Echoed test = {0}",
				//	Encoding.ASCII.GetString(bytes,0,bytesRec)));

				Debug.Log("Before receive");
				using (var stream = new NetworkStream(sender))
				using (var output =	File.Create("result.dat"))
				{
					Debug.Log("Receiving file");

					byte[] buffer = new byte[1024];
					int bytesRead;
					while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0) {
						output.Write(buffer, 0, bytesRead);
					}
				}

				// Release the socket. Do we need this now that we have a NetworkStream?
				sender.Shutdown(SocketShutdown.Both);
				sender.Close();

			} catch (ArgumentNullException ane) {
				Debug.Log(String.Format("ArgumentNullException : {0}",ane.ToString()));
			} catch (SocketException se) {
				Debug.Log(String.Format("SocketException : {0}",se.ToString()));
			} catch (Exception e) {
				Debug.Log(String.Format("Unexpected exception : {0}", e.ToString()));
			}

		} catch (Exception e) {
			Debug.Log( e.ToString());
		}
	}

	// Use this for initialization
	void Start () {
		StartClient();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

