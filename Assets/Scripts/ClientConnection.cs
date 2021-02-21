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
    const int bufferSize = 1024;
    bool messageReceived = true;
    GameObject parent_guy;
    PlayerController parent_guy_script;

    IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

    void Start() {
        Debug.Log("Started Client Code");
        AsyncTCPClient.StartClient();
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

        UdpState state = new UdpState();
        state.ip = RemoteIpEndPoint;
        state.client = receiveClient;
        receiveClient.BeginReceive(new AsyncCallback(ReceiveCallback), state);
        Debug.Log("started first listen");
    }

    public static String dictmuncher(Dictionary<String, String> dict) {
        List<String> to_join = new List<String>(); 
        foreach(KeyValuePair<String, String> entry in dict) {
            to_join.Add(entry.Key + ":" + entry.Value);
        }
        return String.Join(",", to_join);
    }

    void Update() {
        if (next_update < DateTime.Now)
        {
            // send player stuff
            Dictionary<String, String> ok = parent_guy_script.getPositionDict();
            sendMessege(dictmuncher(ok));
            next_update = DateTime.Now + TimeSpan.FromSeconds(.01);
        }
    }

    public DateTime nextPrint = DateTime.Now; 
    
    void ReceiveCallback(IAsyncResult ar) {
        UdpClient u = ((UdpState)(ar.AsyncState)).client;
        IPEndPoint e = ((UdpState)(ar.AsyncState)).ip;

        byte[] receiveBytes = u.EndReceive(ar, ref e);
        string receiveString = Encoding.ASCII.GetString(receiveBytes);

        if (nextPrint < DateTime.Now) {
            Debug.Log($"I GOT SOMETHING: {receiveString}");
            nextPrint = DateTime.Now + TimeSpan.FromSeconds(10);
        }


        lock (parent_guy_script.udp_lock) {
            parent_guy_script.udp_strings_to_process.Add(receiveString);
        }

        UdpState state = new UdpState();
        state.ip = RemoteIpEndPoint;
        state.client = receiveClient;
        receiveClient.BeginReceive(new AsyncCallback(ReceiveCallback), state);
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
    private static ManualResetEvent stateObjectBuilt = 
        new ManualResetEvent(false);
    private static ManualResetEvent receiveDone = 
        new ManualResetEvent(false);

    public class StateObject {  
        public Socket workSocket = null;  
        public const int BufferSize = 256;  
        public byte[] buffer = new byte[BufferSize];  
        public StringBuilder sb = new StringBuilder();  
    }

    private static StateObject state = new StateObject();

    private static PlayerController parent_guy_script = GameObject.FindObjectOfType<PlayerController>();

    public static void StartClient() {
        Socket s_tcp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint serverEP = new IPEndPoint(IPAddress.Parse(SERVER_ADDR), PORT);

        s_tcp.BeginConnect(serverEP, new AsyncCallback(ConnectCallback), s_tcp);
        connectDone.WaitOne();

        state.workSocket = s_tcp;
        stateObjectBuilt.Set();
        s_tcp.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
    }

    private static void ConnectCallback(IAsyncResult ar) {
        Socket s_tcp = (Socket) ar.AsyncState;
        s_tcp.EndConnect(ar);
        connectDone.Set();
    }

    public static void Send(String data) {
        stateObjectBuilt.WaitOne();
        byte[] byteData = Encoding.ASCII.GetBytes(data);  
        
        state.workSocket.BeginSend(byteData, 0, byteData.Length, SocketFlags.None,  
            new AsyncCallback(SendCallback), state.workSocket);
    }

    private static void SendCallback(IAsyncResult ar) {  
        Socket s_tcp = (Socket) ar.AsyncState;  

        int bytesSent = s_tcp.EndSend(ar);
    }

    private static void ReceiveCallback( IAsyncResult ar ) {
        StateObject state = (StateObject) ar.AsyncState;
        Socket s_tcp = state.workSocket;

        int bytesRead = s_tcp.EndReceive(ar);

        if (bytesRead > 0) {
            string incoming_data = Encoding.ASCII.GetString(state.buffer,0,bytesRead).ToString();
            Debug.Log($"Received From Server: {incoming_data}");
            lock (parent_guy_script.tcp_lock) {
                parent_guy_script.tcp_strings_to_process.Add(incoming_data);
            }

            s_tcp.BeginReceive(state.buffer,0,StateObject.BufferSize,0,
                new AsyncCallback(ReceiveCallback), state);
        } else {
            receiveDone.Set();
        }
    }

    private static void Close () {
        stateObjectBuilt.WaitOne();
        state.workSocket.Shutdown(SocketShutdown.Both);
        state.workSocket.Close();
    }
}