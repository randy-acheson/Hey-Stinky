using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using System.Net.Sockets;
using System;
using System.Net;
using System.Threading;
using System.ServiceModel;

public class PlayerController : MonoBehaviour
{
    public GameObject playerPrefabNoCodeReal;
    public AudioSource audioData;

    public float speed = 8f;
    public float gravity = -9.81f;
    public float bounce = 0.04f;
    public float mouseSensitivity = 0.1f;
    
    private Transform body;
    private Transform camera;
    private Transform hand;
    private CharacterController controller;
    private Light flashlight;
    private float velY = 0f;
    private float rotX = 0;
    private float movement = 0f;
    private bool isGrounded = true;
    private GameObject crystal;

    private DateTime next_update = DateTime.Now;

    private Text uiText;
    private bool isClicking = false;

    public string player_hash;

    private float[] sendPacket = new float[6];

    public object udp_lock = new object();
    public object tcp_lock = new object();
    public List<String> udp_strings_to_process = new List<String>();
    public List<String> tcp_strings_to_process = new List<String>();

    public Dictionary<String, Tuple<GameObject, DateTime>> player_holder = new Dictionary<String, Tuple<GameObject, DateTime>>();
    
    void Start()
    {
        controller = gameObject.GetComponent<CharacterController>();
        camera = transform.GetChild(0);
        body = transform.GetChild(1);
        hand = camera.GetChild(0);
        flashlight = hand.GetChild(0).GetComponent<Light>();
        player_hash = generatePlayerHash();
        uiText = GetComponentInChildren<Text>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.normal.y > 0.5) {
            isGrounded = true;
        } else if(hit.normal.y < -0.9 && hit.moveDirection.y > 0 && velY > 0){
            velY = 0;
        }
    }

    //////////////////////////////////////////////////////////////////////////////////////////////

    public GameObject GetPlayer(String username) {
        if (!player_holder.ContainsKey(username)) {
            Debug.Log("instantiating player named: " + username);
            GameObject new_guy = null;
            try {
                new_guy = Instantiate(playerPrefabNoCodeReal, new Vector3(0, 0, 0), Quaternion.identity);
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

    public void HandlePlayerInteract(String username, String Object, byte action){
        GameObject player = GameObject.Find(username);
        if (player == null){
            Instantiate(playerPrefabNoCodeReal, new Vector3(0, 0, 0), Quaternion.identity);
        }
        GameObject.Find(Object).GetComponent<InteractiveObject>().OnPlayerInteract(player, action);
    }
    //////////////////

    private Tuple<String, String> GetKeyVal(String something) {
        List<String> stuff3 = new List<String>(something.Split(':'));
        String val;
        String key;
        key = stuff3[0].Trim();
        val = stuff3[1].Trim();
        return new Tuple<String, String>(key, val);
    }

    private void process_tcp_messege(String msg) {
        try {
            
        }
        catch (Exception e) {
            Debug.Log(e);
        }
    }

    private void process_udp_messege(String msg) {
        try {
            GameObject remotePlayer = null;
            List<String> stuff2 = new List<String>(msg.Split(','));

            Dictionary<String, String> all_dict = new Dictionary<String, String>();
            foreach (var something in stuff2) {
                Tuple<String, String> KeyValPair = GetKeyVal(something);
                String key = KeyValPair.Item1;
                String val = KeyValPair.Item2;

                all_dict[key] = val;
            }

            if (all_dict["player_hash"] == player_hash) {
                return;
            }
            // Debug.Log("getting palyer");
            remotePlayer = GetPlayer(all_dict["player_hash"]);

            if (remotePlayer != null) {
                remotePlayer.transform.position = new Vector3(float.Parse(all_dict["body_posX"]), float.Parse(all_dict["body_posY"]), float.Parse(all_dict["body_posZ"]));
                remotePlayer.transform.rotation = Quaternion.Euler(remotePlayer.transform.rotation.x, float.Parse(all_dict["body_rotY"]), remotePlayer.transform.rotation.z);
                remotePlayer.transform.GetChild(0).rotation = Quaternion.Euler(float.Parse(all_dict["head_rotX"]), remotePlayer.transform.rotation.y, remotePlayer.transform.rotation.z);
            }
        }
        catch (Exception e) {
            Debug.Log(e);
        }
    }

    private void FixedUpdate()
    {
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

        Debug.DrawRay(camera.transform.position, camera.transform.forward, Color.white, 5f, false);
        RaycastHit hit;
        string newUIText = "";
        // Does the ray intersect any objects excluding the player layer
        //if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
        if (Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, Mathf.Infinity))
        {
            Debug.DrawRay(camera.transform.position, camera.transform.forward, Color.yellow, 5f, false);
            var obj = hit.collider.gameObject.GetComponent<InteractiveObject>();
            if (obj != null)
            {
                //Debug.Log("hit "+hit.transform.name);
                if (isClicking)
                {
                    obj.OnPlayerInteract(gameObject, 0);
                }
                else
                {
                    newUIText = obj.getHoverMessage();
                }
            }
        }
        else
        {
            Debug.DrawRay(camera.transform.position, camera.transform.forward, Color.white, 5f, false);
            //Debug.Log("Did not Hit");
        }
        isClicking = false;
        if (uiText != null)
        {
            uiText.text = newUIText;
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            UnityEditor.EditorApplication.isPlaying = false; 
        }

        if(!Physics.CheckSphere(transform.position, 0.1f)){
            isGrounded = false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            isClicking = true;
        }

        if (Input.GetKey(KeyCode.LeftControl)){
        speed = 8;
        }else{
            speed = 4;
        }

        if(Input.GetKey(KeyCode.LeftShift)){
            speed = 8;
        }else{
            speed = 4;
        }

        if(isGrounded){
            if(Input.GetKeyDown(KeyCode.Space)){
                velY = 4;
                isGrounded = false;
            }else{
                velY = 0;
            }
        }else{
            velY += gravity * Time.deltaTime;
        }

        float posX = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        float posZ = Input.GetAxis("Vertical") * speed * Time.deltaTime;

        if(isGrounded && (posX != 0 || posZ != 0))
        {
            movement = (movement + 1.5f * Mathf.Max(Mathf.Abs(posX), Mathf.Abs(posZ))) % (Mathf.PI*2f);
        }
        else if(movement < Mathf.PI*0.5f)
        {
            movement = Mathf.Max(movement - 5f*Time.deltaTime, 0f);
        }
        else if(movement >= Mathf.PI*0.5f && movement < Mathf.PI)
        {
            movement = Mathf.Min(movement + 5f*Time.deltaTime, Mathf.PI);
        }
        else if(movement >= Mathf.PI && movement < Mathf.PI*1.5f)
        {
            movement = Mathf.Max(movement - 5f*Time.deltaTime, Mathf.PI);
        }
        else if(movement >= Mathf.PI*1.5f)
        {
            movement = Mathf.Min(movement + 5f*Time.deltaTime, Mathf.PI*2f);
        }

        float vertBob = Mathf.Abs(Mathf.Sin(movement + Mathf.PI*0.5f));
        float horiBob = Mathf.Sin(movement);

        float posY = velY * Time.deltaTime;

        body.localPosition = new Vector3(horiBob*bounce*0.5f, 0.9f+vertBob*bounce, 0f);
        //body.transform.localRotation = Quaternion.Euler(0f, 0f, horiBob*-0.0244f);
        camera.localPosition = new Vector3(horiBob*bounce*0.5f, 1.68f+vertBob*bounce, 0f);

        controller.Move(transform.right*posX + transform.forward*posZ + Vector3.up*posY);

        sendPacket[0] = transform.position.x;
        sendPacket[1] = transform.position.y;
        sendPacket[2] = transform.position.z;

        /////////////////////////////////

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        //Debug.Log("MouseX: " + Input.GetAxis("Mouse X") + ", sens: " + mouseSensitivity + ", time: " + Time.deltaTime + ", total: " + mouseX);
        rotX -= mouseY;
        rotX = Mathf.Clamp(rotX, -85f, 85f);

        camera.localRotation = Quaternion.Euler(rotX, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        sendPacket[3] = camera.localRotation.x;
        sendPacket[4] = transform.rotation.y;

        /////////////////////////////////
    
        hand.localRotation = Quaternion.Euler(
            90f + 1.5f*(Mathf.PerlinNoise(0, 1f*Time.time)-0.5f), 
            1.5f*(Mathf.PerlinNoise(1f*Time.time, 0)-0.5f), 
            0f);

        ////////////////////////////////

        if(Input.GetKeyDown(KeyCode.E)){
            flashlight.enabled = !flashlight.enabled;
            if(flashlight.enabled){
                sendPacket[5] = 1f;
            }else{
                sendPacket[5] = 0f;
            }
            hand.gameObject.SetActive(!hand.gameObject.activeSelf);
        }

        float noise = Mathf.PerlinNoise(0, 10f*Time.time);

        flashlight.intensity = Mathf.Min(40f + noise*160f, 80f);
    }

    private void OnMouseDown()
    {
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            //Debug.Log("Did Hit");
            if (hit.transform.tag == "Interactive")
            {
                InteractiveObject obj = hit.transform.GetComponent<InteractiveObject>();
                obj.OnPlayerInteract(gameObject, 0);
            }
        }
    }

    string generatePlayerHash() {
        byte[] byte_hash;
        using (HashAlgorithm algorithm = SHA256.Create()) {
            byte_hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(System.DateTime.Now.ToString()+System.Environment.MachineName));
        }
        
        StringBuilder sb = new StringBuilder();
        foreach (byte b in byte_hash) {
            sb.Append(b.ToString("X2"));
        }
        return sb.ToString();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Crystal") && crystal == null)
        {
            if (!other.gameObject.GetComponent<CrystalController>().isDeposited)
            {
                crystal = other.gameObject;
                crystal.GetComponent<CrystalController>()
                    .SetTransformParent(gameObject.transform);
                crystal.transform.localPosition = new Vector3(0,0.5f,0.5f);
            }
        }
        else if (other.gameObject.CompareTag("Receptacle") && crystal != null)
        {
            other.gameObject.GetComponent<ReceptacleScript>().AddCrystal(crystal);
            crystal = null;
        }
        else if (other.gameObject.CompareTag("Goal"))
        {
            Debug.Log("You win!");
            audioData.Play(0);
        }
    }

    public string getPositionDict() {
        Vector3 player_xyz_pos = gameObject.transform.position;
        Vector3 player_xyz_rot = gameObject.transform.eulerAngles;
        float head_x_rot = gameObject.transform.GetChild(0).eulerAngles.x;
        return $"player_hash: {player_hash}, body_posX: {player_xyz_pos.x}, body_posY: {player_xyz_pos.y}, " + 
                $"body_posZ: {player_xyz_pos.z}, head_rotX: {head_x_rot}, body_rotY: {player_xyz_rot.y}, body_rotZ: {player_xyz_rot.z}";
    }

    public void hideInCloset(GameObject closet)
    {
        transform.position = closet.transform.position;
    }
}
