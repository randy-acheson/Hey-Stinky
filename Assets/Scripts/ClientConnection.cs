using UnityEngine;
using System.Net.Sockets;
using System;
using System.Net;
using System.Text;
using System.Threading;
using System.ServiceModel;
using System.Collections.Generic;


public class UdpState {
    public UdpClient client;
    public IPEndPoint ip;
}


public class ClientConnection : MonoBehaviour
{

    public Dictionary<String, GameObject> player_holder = new Dictionary<String, GameObject>();

    public GameObject playerPrefabNoCodeReal;
    UdpClient senderClient;
    UdpClient receiveClient;
    DateTime next_update = DateTime.Now;
    const int opsToPreAlloc = 2;
    const int bufferSize = 1024;
    bool messageReceived = true;
    GameObject parent_guy;
    PlayerController parent_guy_script;

    IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

    public void Start() {
        // Debug.Log("construct");
        // client = new UdpClient(5006);

        receiveClient = new UdpClient(5005);
        senderClient = new UdpClient(5006);

        IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5006); // endpoint where server is listening
        senderClient.Connect(ep);

        parent_guy = GameObject.Find("playerPrefab");
        Debug.Log(parent_guy);
        parent_guy_script = parent_guy.GetComponent<PlayerController>();
        Debug.Log(parent_guy_script);
        parent_guy_script = GameObject.FindObjectOfType<PlayerController>();
        Debug.Log(parent_guy_script);
    }

    public void Update() {
        if (next_update < DateTime.Now)
        {
            // send player stuff
            String ok = parent_guy_script.getPositionDict();
            sendMessege(ok);
            next_update = DateTime.Now + TimeSpan.FromSeconds(.1);

            // recieve player stuff
            if (messageReceived == true) {
                UdpState state = new UdpState();
                state.ip = RemoteIpEndPoint;
                state.client = receiveClient;
                receiveClient.BeginReceive(new AsyncCallback(ReceiveCallback), state);
                messageReceived = false;
                // next_update = DateTime.Now + TimeSpan.FromSeconds(.1);
            }
        }
    }

    
    public void ReceiveCallback(IAsyncResult ar) {
        UdpClient u = ((UdpState)(ar.AsyncState)).client;
        IPEndPoint e = ((UdpState)(ar.AsyncState)).ip;

        byte[] receiveBytes = u.EndReceive(ar, ref e);
        string receiveString = Encoding.ASCII.GetString(receiveBytes);

        // Debug.Log($"I GOT SOMETHING: {receiveString}");


        lock (parent_guy_script.__lockObj) {
            parent_guy_script.to_add.Add(receiveString);
        }
        messageReceived = true;
    }



    public void sendMessege(String message) {
        try {
            Byte[] sendBytes = Encoding.ASCII.GetBytes(message);
            // Debug.Log("sending update " + message);
            senderClient.Send(sendBytes, sendBytes.Length);
            // Debug.Log("sendt update " + message);

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
