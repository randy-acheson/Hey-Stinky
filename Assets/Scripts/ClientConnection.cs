using UnityEngine;
using System.Net.Sockets;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.ServiceModel;
using System.Collections.Generic;


public class UdpState {
    public UdpClient client;
    public IPEndPoint ip;
}


public class ClientConnection : MonoBehaviour {

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

    void Start() {
        Debug.Log("Started Client Code");
        // AsyncTCPClient.StartClient();
        receiveClient = new UdpClient(5005);
        senderClient = new UdpClient(5006);

        IPEndPoint ep = new IPEndPoint(IPAddress.Parse("192.168.86.61"), 5006); // endpoint where server is listening
        senderClient.Connect(ep);

        parent_guy = GameObject.Find("playerPrefab");
        Debug.Log(parent_guy);
        parent_guy_script = parent_guy.GetComponent<PlayerController>();
        Debug.Log(parent_guy_script);
        parent_guy_script = GameObject.FindObjectOfType<PlayerController>();
        Debug.Log(parent_guy_script);
    }

    void Update() {
        if (next_update < DateTime.Now)
        {
            // send player stuff
            String ok = parent_guy_script.getPositionDict();
            sendMessege(ok);
            next_update = DateTime.Now + TimeSpan.FromSeconds(.2);

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

    
    void ReceiveCallback(IAsyncResult ar) {
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



    void sendMessege(String message) {
        try {
            Byte[] sendBytes = Encoding.ASCII.GetBytes(message);
            // Debug.Log("sending update " + message);
            senderClient.Send(sendBytes, sendBytes.Length);
        }

        catch (Exception e ) {
                    Console.WriteLine(e.ToString());
        }
    }
}

public class AsyncTCPClient {
    private const string SERVER_ADDR = "192.168.86.46";
    private const int PORT = 7777;

    private static ManualResetEvent connectDone = 
        new ManualResetEvent(false);
    private static ManualResetEvent sendDone = 
        new ManualResetEvent(false);
    private static ManualResetEvent receiveDone = 
        new ManualResetEvent(false);

    public static void StartClient() {
        using (StreamWriter sw = File.AppendText(@"C:\Programming\hey-stinky\Assets\Scripts\debug.txt")){sw.WriteLine("Started\n");}
        Socket s_tcp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint serverEP = new IPEndPoint(IPAddress.Parse(SERVER_ADDR), PORT);

        s_tcp.BeginConnect(serverEP, new AsyncCallback(ConnectCallback), s_tcp);
        connectDone.WaitOne();
        using (StreamWriter sw = File.AppendText(@"C:\Programming\hey-stinky\Assets\Scripts\debug.txt")){sw.WriteLine("Connected\n");}

        Send(s_tcp,"This is a test<EOF>");
        sendDone.WaitOne();
        using (StreamWriter sw = File.AppendText(@"C:\Programming\hey-stinky\Assets\Scripts\debug.txt")){sw.WriteLine("Sent\n");}

        Receive(s_tcp);
        // receiveDone.WaitOne();
        using (StreamWriter sw = File.AppendText(@"C:\Programming\hey-stinky\Assets\Scripts\debug.txt")){sw.WriteLine("Recieved\n");}

        // s_tcp.Shutdown(SocketShutdown.Both);
        // s_tcp.Close();
        using (StreamWriter sw = File.AppendText(@"C:\Programming\hey-stinky\Assets\Scripts\debug.txt")){sw.WriteLine("Done\n");}
    }

    private static void ConnectCallback(IAsyncResult ar) {
        Socket s_tcp = (Socket) ar.AsyncState;
        s_tcp.EndConnect(ar);
        connectDone.Set();
    }

    public static void Send(Socket s_tcp, String data) {  
        byte[] byteData = Encoding.ASCII.GetBytes(data);  
        
        s_tcp.BeginSend(byteData, 0, byteData.Length, SocketFlags.None,  
            new AsyncCallback(SendCallback), s_tcp);
    }

    private static void SendCallback(IAsyncResult ar) {  
        Socket s_tcp = (Socket) ar.AsyncState;  

        int bytesSent = s_tcp.EndSend(ar);  

        sendDone.Set();  
    }

    public class StateObject {  
        public Socket workSocket = null;  
        public const int BufferSize = 256;  
        public byte[] buffer = new byte[BufferSize];  
        public StringBuilder sb = new StringBuilder();  
    }

    private static void Receive(Socket s_tcp) {
        StateObject state = new StateObject();
        state.workSocket = s_tcp;

        s_tcp.BeginReceive( state.buffer, 0, StateObject.BufferSize, 0,
            new AsyncCallback(ReceiveCallback), state);
    }

    private static void ReceiveCallback( IAsyncResult ar ) {
        StateObject state = (StateObject) ar.AsyncState;
        Socket s_tcp = state.workSocket;

        int bytesRead = s_tcp.EndReceive(ar);

        if (bytesRead > 0) {
            Debug.Log($"Received From Server: {Encoding.ASCII.GetString(state.buffer,0,bytesRead).ToString()}");

            s_tcp.BeginReceive(state.buffer,0,StateObject.BufferSize,0,
                new AsyncCallback(ReceiveCallback), state);
        } else {
            receiveDone.Set();
        }
    }
}