using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class Client : MonoBehaviour {

	public static void StartClient(IPEndPoint remoteEP) {
		// Data buffer for incoming data.
		byte[] bytes = new byte[1024];

		// Connect to the server
		try {
			remoteEP.Port = 11000;

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

	private IEnumerator ProbeCoroutine(System.Action<IPEndPoint> callBack) {
		//Set up new UDP Client
		var Client = new UdpClient();
		var RequestData = Encoding.ASCII.GetBytes("SomeRequestData");
		var ServerEp = new IPEndPoint(IPAddress.Any, 0);

		//Low timeouts to minimise lag
		Client.Client.SendTimeout = 100;
		Client.Client.ReceiveTimeout = 100;

		Client.EnableBroadcast = true;

		bool tryAgain = true;
		while (tryAgain) {
			try {
				Client.Send(RequestData, RequestData.Length, new IPEndPoint(IPAddress.Broadcast, 8888));

				var ServerResponseData = Client.Receive(ref ServerEp);
				var ServerResponse = Encoding.ASCII.GetString(ServerResponseData);
				Debug.Log(String.Format("Recived {0} from {1}", ServerResponse, ServerEp.Address.ToString()));
				tryAgain = false;
			} catch (SocketException e) {
				Debug.Log(String.Format("Server not found: {0}", e.ToString()));
			}
			yield return 0;
		}
		Client.Close();
		callBack (ServerEp);
	}
	// Use this for initialization
	void Start () {
//		IPEndPoint remoteEp = null;
		StartCoroutine (ProbeCoroutine ((IPEndPoint p) => {
			StartClient (p);
		})
		);

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

