using UnityEngine;
// using UnityEngine.CoreModule;
using System.Net.Sockets;
using System;
using System.Net;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.ServiceModel;

// using System.ServiceModel.Channels;


public class MulticastConnection : MonoBehaviour
{

    DateTime next_update = DateTime.Now;
    // BufferManager m_bufferManager;
    const int opsToPreAlloc = 2;
    const int bufferSize = 1024;
    bool messageReceived = true;

    IPEndPoint RemoteIpEndPoint;
    IPEndPoint ep;

    private static IPAddress mcastAddress;
    private static int mcastPort;
    private static Socket mcastSocket;

    public GameObject playerPrefab;

    byte[] playerTransformMessage;

    Dictionary<byte[], GameObject> players = new Dictionary<byte[], GameObject>();

    void Start()
    {

        System.Random rnd = new System.Random();
        playerTransformMessage = new Byte[28];
        rnd.NextBytes(playerTransformMessage);

        mcastAddress = IPAddress.Parse("224.168.100.2");
        mcastPort = 11000;

        // Join the listener multicast group.
        JoinMulticastGroup();

        Thread t = new Thread(new ThreadStart(ReceiveBroadcastMessages));
        t.Start();
    }

    void Update()
    {
        Array.Copy(BitConverter.GetBytes(gameObject.transform.position.x), 0, playerTransformMessage, 8, 4);
        Array.Copy(BitConverter.GetBytes(gameObject.transform.position.y), 0, playerTransformMessage, 12, 4);
        Array.Copy(BitConverter.GetBytes(gameObject.transform.position.z), 0, playerTransformMessage, 16, 4);
        //playerTransformMessage[8] = gameObject.transform.position.x[0];
        // Broadcast the message to the listener.
        BroadcastMessage(playerTransformMessage);
    }


    static void JoinMulticastGroup()
    {
        try
        {
            // Create a multicast socket.
            mcastSocket = new Socket(AddressFamily.InterNetwork,
                                     SocketType.Dgram,
                                     ProtocolType.Udp);

            // Get the local IP address used by the listener and the sender to
            IPAddress localIPAddr = IPAddress.Parse("192.168.86.39");

            // Create an IPEndPoint object.
            IPEndPoint IPlocal = new IPEndPoint(localIPAddr, 0);

            // Bind this endpoint to the multicast socket.
            mcastSocket.Bind(IPlocal);

            // Define a MulticastOption object specifying the multicast group
            // address and the local IP address.
            // The multicast group address is the same as the address used by the listener.
            MulticastOption mcastOption = new MulticastOption(mcastAddress, localIPAddr);

            mcastSocket.SetSocketOption(SocketOptionLevel.IP,
                                        SocketOptionName.AddMembership,
                                        mcastOption);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    private void ReceiveBroadcastMessages()
    {
        bool done = false;
        byte[] bytes = new Byte[28];
        IPEndPoint groupEP = new IPEndPoint(mcastAddress, mcastPort);
        EndPoint remoteEP = (EndPoint)new IPEndPoint(IPAddress.Any, 0);

        try
        {
            while (!done)
            {
                Debug.Log("Waiting for multicast packets.......");

                mcastSocket.ReceiveFrom(bytes, ref remoteEP);

                HandlePlayerTransform(bytes);

                Debug.Log("Received broadcast");
            }

            mcastSocket.Close();
        }

        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    private void BroadcastMessage(byte[] message)
    {
        IPEndPoint endPoint;

        try
        {
            endPoint = new IPEndPoint(mcastAddress, mcastPort);
            mcastSocket.SendTo(message, endPoint);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }




    //////////////////////////////////////////////////////////////////////////////////////////////

    public void HandlePlayerTransform(byte[] message)
    {
        byte[] id = new byte[8];
        byte[] x = new byte[4];
        byte[] y = new byte[4];
        byte[] z = new byte[4];
        Array.Copy(message, 0, id, 0, 8);
        Array.Copy(message, 8, x, 0, 4);
        Array.Copy(message, 12, y, 0, 4);
        Array.Copy(message, 16, z, 0, 4);
        
        if(!players.ContainsKey(id)){
            players.Add(id, Instantiate(playerPrefab, new Vector3(0, 0, 0), Quaternion.identity));
        }
        players[id].transform.position = new Vector3(System.BitConverter.ToSingle(x, 0), System.BitConverter.ToSingle(y, 0), System.BitConverter.ToSingle(z, 0));
    }

    public void HandlePlayerInteract(String username, String Object, byte action)
    {
        GameObject player = GameObject.Find(username);
        if (player == null)
        {
            //Instantiate(playerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        }
        gameObject.GetComponent<InteractiveObject>().OnPlayerInteract(gameObject, action);
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////

}
