using UnityEngine;
// using UnityEngine.CoreModule;
using System.Net.Sockets;
using System;
using System.Net;
using System.Text;
using System.Threading;
using System.ServiceModel;
// using System.ServiceModel.Channels;


public class ClientConnection : MonoBehaviour
{
    ClientConnection() {
        // Debug.Log("construct");
        // client = new UdpClient(5006);
    }


    UdpClient senderClient;
    UdpClient receiveClient;
    int times_happened = 0;
    DateTime next_update = DateTime.Now;
    // BufferManager m_bufferManager;
    const int opsToPreAlloc = 2;
    const int bufferSize = 1024;
    bool messageReceived = true;

    IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
    IPEndPoint ep;


    void Start() {
        receiveClient = new UdpClient(5005);
        senderClient = new UdpClient(5006);

        ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5006); // endpoint where server is listening
        senderClient.Connect(ep);
        // receiveClient.Connect(RemoteIpEndPoint);
    }

    void Update() {
        UpdateServerWithInfo("john", new Vector3(0, 0, 0));
    }

    
    public void ReceiveCallback(IAsyncResult ar)
    {
        UdpClient u = ((UdpState)(ar.AsyncState)).client;
        IPEndPoint e = ((UdpState)(ar.AsyncState)).ip;

        byte[] receiveBytes = u.EndReceive(ar, ref e);
        string receiveString = Encoding.ASCII.GetString(receiveBytes);

        Debug.Log($"Received: {receiveString}");
        messageReceived = true;
    }

    public class UdpState {
        public UdpClient client;
        public IPEndPoint ip;
    }

    void UpdateServerWithInfo(String playerName, Vector3 position) {
        if (next_update < DateTime.Now)
        {
            // send player stuff
            String the_str = playerName + ':' + position.ToString();
            sendMessege(the_str);
            next_update = DateTime.Now + TimeSpan.FromSeconds(.1);
            times_happened++;

            // recieve player stuff

            if (messageReceived == true)
            {
                Debug.Log("starting listening");
                UdpState state = new UdpState();
                state.ip = RemoteIpEndPoint;
                state.client = receiveClient;
                receiveClient.BeginReceive(new AsyncCallback(ReceiveCallback), state);
                messageReceived = false;
            }
        }
    }

    void sendMessege(String message) {
        try {
            Byte[] sendBytes = Encoding.ASCII.GetBytes(message);
            Debug.Log("sending update " + message);
            senderClient.Send(sendBytes, sendBytes.Length);

            Debug.Log("sendt update " + message);

            // Sends a message to a different host using optional hostname and port parameters.
            // UdpClient udpClientB = new UdpClient();
            // udpClientB.Send(sendBytes, sendBytes.Length, "AlternateHostMachineName", 5005);

            // IPEndPoint object will allow us to read datagrams sent from any source.

            // Blocks until a message returns on this socket from a remote host.
            // string returnData = Encoding.ASCII.GetString(receiveBytes);

            // Uses the IPEndPoint object to determine which of these two hosts responded.
            // Console.WriteLine("This is the message you received " +
                                        // returnData.ToString());
            // Console.WriteLine("This message was sent from " +
                                        // RemoteIpEndPoint.Address.ToString() +
                                        // " on their port number " +
                                        // RemoteIpEndPoint.Port.ToString());

            // client.Close();
            // udpClientB.Close();
        }

        catch (Exception e ) {
                    Console.WriteLine(e.ToString());
        }
    }
}
