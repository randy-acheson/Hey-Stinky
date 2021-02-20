using UnityEngine;


namespace Backrooms
{
    public class ClientConnection : MonoBehavior
    {
        UdpClient client;
        ClientConnection() {
            client = new UdpClient(5005);
        }

        void sendMessege() {
            try {
                client.Connect("127.0.0.1", 5005);

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