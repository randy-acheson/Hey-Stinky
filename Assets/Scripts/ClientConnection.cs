using UnityEngine;
using System.Net.Sockets;
using System;
using System.Net;
using System.Text;


namespace Backrooms
{
    public class ClientConnection : MonoBehaviour
    {
        // UdpClient client;
        ClientConnection() {
            Debug.Log("construct");
            // client = new UdpClient(5006);
        }

        void Start() {
            Debug.Log("Start");
            sendMessege();
        }

        void sendMessege() {
            UdpClient client = new UdpClient(5006);
            try {
                client.Connect("127.0.0.1", 5006);

                // Sends a message to the host to which you have connected.
                Byte[] sendBytes = Encoding.ASCII.GetBytes("Is anybody there?");

                client.Send(sendBytes, sendBytes.Length);

                // Sends a message to a different host using optional hostname and port parameters.
                // UdpClient udpClientB = new UdpClient();
                // udpClientB.Send(sendBytes, sendBytes.Length, "AlternateHostMachineName", 5005);

                // IPEndPoint object will allow us to read datagrams sent from any source.
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                // Blocks until a message returns on this socket from a remote host.
                // Byte[] receiveBytes = client.Receive(ref RemoteIpEndPoint);
                // string returnData = Encoding.ASCII.GetString(receiveBytes);

                // Uses the IPEndPoint object to determine which of these two hosts responded.
                // Console.WriteLine("This is the message you received " +
                                            // returnData.ToString());
                // Console.WriteLine("This message was sent from " +
                                            // RemoteIpEndPoint.Address.ToString() +
                                            // " on their port number " +
                                            // RemoteIpEndPoint.Port.ToString());

                client.Close();
                // udpClientB.Close();
            }

            catch (Exception e ) {
                        Console.WriteLine(e.ToString());
            }
        }
    }
}