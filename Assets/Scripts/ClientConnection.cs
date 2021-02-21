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

    public GameObject playerPrefabNoCodeReal;
    public GameObject crawler;
    UdpClient senderClient;
    UdpClient receiveClient;
    DateTime next_update = DateTime.Now;
    const int bufferSize = 1024;
    //bool messageReceived = true;
    GameObject parent_guy;
    CreatureBase parent_guy_script;

    IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);


    public object udp_lock = new object();
    public object tcp_lock = new object();
    public List<String> udp_strings_to_process = new List<String>();
    public List<String> tcp_strings_to_process = new List<String>();

    public Dictionary<String, Tuple<GameObject, DateTime>> player_holder = new Dictionary<String, Tuple<GameObject, DateTime>>();


    void Start() {
        Debug.Log("Started Client Code");
        AsyncTCPClient.StartClient();
        receiveClient = new UdpClient(5005);
        senderClient = new UdpClient(5006);

        IPEndPoint ep = new IPEndPoint(IPAddress.Parse("192.168.86.61"), 5006); // endpoint where server is listening
        senderClient.Connect(ep);

        parent_guy = GameObject.Find("playerPrefab");

        try {
            parent_guy_script = GameObject.FindObjectOfType<PlayerController>();
        }
        catch (Exception e) {
            parent_guy_script = GameObject.FindObjectOfType<MonsterController>();
        }

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

    public GameObject GetPlayer(String username, String prefabname="default_lmao") {
        if (!player_holder.ContainsKey(username)) {
            Debug.Log("instantiating multiplayer entity named: " + username);
            GameObject new_guy = null;
            try {
                if (prefabname == "playerPrefab") {
                    new_guy = Instantiate(playerPrefabNoCodeReal, new Vector3(0, 0, 0), Quaternion.identity);
                }
                else if (prefabname == "crawler") {
                    new_guy = Instantiate(crawler, new Vector3(0, 0, 0), Quaternion.identity);
                }
                else {
                    Debug.Log("WARNING COULD NOT FIND THE PLAYERPREFAB: " + prefabname);
                    new_guy = Instantiate(playerPrefabNoCodeReal, new Vector3(0, 0, 0), Quaternion.identity);
                }
                Debug.Log("instantiated player: " + username);
                player_holder[username] = new Tuple<GameObject, DateTime>(new_guy, DateTime.Now + TimeSpan.FromSeconds(3));
                Debug.Log(player_holder[username].Item1);
                return new_guy;            
            }
            catch (Exception e) {
                Debug.Log(e);
                Application.Quit();
            }
            return null;
        }
        else {
            // Debug.Log("found player");
            player_holder[username] = new Tuple<GameObject, DateTime>(player_holder[username].Item1, DateTime.Now + TimeSpan.FromSeconds(3));
            return player_holder[username].Item1;
        }
    }

    void FixedUpdate() {
        lock (udp_lock) {
            foreach (var msg in udp_strings_to_process) {
                process_udp_messege(msg);
            }
            udp_strings_to_process = new List<String>();

            List<String> to_die = new List<String>();
            foreach(KeyValuePair<String, Tuple<GameObject, DateTime>> entry in player_holder) {
                if (entry.Value.Item2 < DateTime.Now) {
                    to_die.Add(entry.Key);
                }
            }

            foreach (var name in to_die) {
                Debug.Log("REMOVING: " + name);
                Destroy(player_holder[name].Item1);
                player_holder.Remove(name);
            }
        }

        lock (tcp_lock) {
            foreach (var msg in tcp_strings_to_process) {
                process_tcp_messege(msg);
            }
            tcp_strings_to_process = new List<String>();
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


        lock (udp_lock) {
            udp_strings_to_process.Add(receiveString);
        }

        UdpState state = new UdpState();
        state.ip = RemoteIpEndPoint;
        state.client = receiveClient;
        receiveClient.BeginReceive(new AsyncCallback(ReceiveCallback), state);
    }

    private void process_tcp_messege(String msg) {
        try {
            Dictionary<string, string> argDict = stringmuncher(msg);
            DictCommandEvaluator dcm = new DictCommandEvaluator();
            dcm.eval(argDict["function"], new object[] {argDict});
        }
        catch (Exception e) {
            Debug.Log(e);
        }
    }

    public Dictionary<String, String> stringmuncher(String string_to_munch) {
        Dictionary<String, String> dict = new Dictionary<String, String>();
        
        List<String> stuff2 = new List<String>(string_to_munch.Split(','));
        foreach (var something in stuff2) {
            Tuple<String, String> KeyValPair = GetKeyVal(something);
            String key = KeyValPair.Item1;
            String val = KeyValPair.Item2;

            dict[key] = val;
        }
        return dict;
    }

    private Tuple<String, String> GetKeyVal(String something) {
        List<String> stuff3 = new List<String>(something.Split(':'));
        String val;
        String key;
        key = stuff3[0].Trim();
        val = stuff3[1].Trim();
        var temp = new Tuple<String, String>(key, val);
        return temp;
    }


    private void process_udp_messege(String msg) {
        try {
            GameObject remotePlayer = null;
            Dictionary<String, String> all_dict = stringmuncher(msg);

            if (all_dict["player_hash"] == parent_guy_script.get_player_hash()) {
                return;
            }
            // Debug.Log("getting palyer");

            if (all_dict.ContainsKey("prefab_name")) {
                remotePlayer = GetPlayer(all_dict["player_hash"], all_dict["prefab_name"]);
            }
            else {
                remotePlayer = GetPlayer(all_dict["player_hash"]);
            }

            if (remotePlayer != null) {
                remotePlayer.transform.position = new Vector3(float.Parse(all_dict["body_posX"]), float.Parse(all_dict["body_posY"]), float.Parse(all_dict["body_posZ"]));
                remotePlayer.transform.rotation = Quaternion.Euler(0, float.Parse(all_dict["body_rotY"]), 0);
                remotePlayer.transform.GetChild(0).localRotation = Quaternion.Euler(float.Parse(all_dict["head_rotX"]), 0, 0);
            }
        }
        catch (Exception e) {
            Debug.Log(e);
        }
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
    private static ClientConnection client_connection_script = GameObject.FindObjectOfType<ClientConnection>();

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
        
        Debug.Log("okay" + data);

        state.workSocket.BeginSend(byteData, 0, byteData.Length, SocketFlags.None,  
            new AsyncCallback(SendCallback), state.workSocket);
    }

    private static void SendCallback(IAsyncResult ar) {  
        Socket s_tcp = (Socket) ar.AsyncState;  
        Debug.Log("DONEZO");

        int bytesSent = s_tcp.EndSend(ar);
    }

    private static void ReceiveCallback( IAsyncResult ar ) {
        StateObject state = (StateObject) ar.AsyncState;
        Socket s_tcp = state.workSocket;

        int bytesRead = s_tcp.EndReceive(ar);

        if (bytesRead > 0) {
            string incoming_data = Encoding.ASCII.GetString(state.buffer,0,bytesRead).ToString();
            Debug.Log($"Received From Server: {incoming_data}");
            lock (client_connection_script.tcp_lock) {
                client_connection_script.tcp_strings_to_process.Add(incoming_data);
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