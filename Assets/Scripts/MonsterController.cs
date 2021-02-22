using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

public class MonsterController : MonoBehaviour, CreatureBase
{
    public float speed = 4f;
    public float gravity = -9.81f;
    public float bounce = 0.04f;
    public float mouseSensitivity = 0.1f;

    public Transform headBone;
    
    private float maxSpeed;

    private Transform body;
    private Transform camera;
    //private Transform hand;
    private CharacterController controller;
    //private Light flashlight;
    private float velY = 0f;
    private float rotX = 0;
    private float movement = 0f;
    private bool isGrounded = true;
    private bool isWalled = false;
    private GameObject crystal;

    private Animator animator;

    private Text uiText;
    private bool isClicking = false;
    private bool isAttacking = false;
    private bool wallClicked = false;
    private bool wallLeft = false;
    private Vector3 wallNormal;
    private Vector3 hitVector;

    private String player_hash;

    private float[] sendPacket = new float[6];
    

    void Start()
    {
        controller = gameObject.GetComponent<CharacterController>();
        headBone = gameObject.transform.Find("Armature/Bone/Bone.003/Bone.004/Bone.005/Bone.006/Bone.007").gameObject.transform;
        camera = transform.Find("HeadObject");
        body = transform.Find("crawler_low");
        //hand = camera.GetChild(0);
        player_hash = generatePlayerHash();
        uiText = GetComponentInChildren<Text>();

        Cursor.lockState = CursorLockMode.Locked;

        //headBone = gameObject.transform.Find("Armature/Bone/Bone.003/Bone.004/Bone.005/Bone.006/HeadObject").gameObject;

        animator = GetComponent<Animator>();
        maxSpeed = speed;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //Debug.Log("hit: " + hit.normal);
        if(!isWalled && hit.normal.y > 0.9){
            // Debug.Log("hitting wrong thing");

            isGrounded = true;
            isWalled = false;
            gravity = -9.81f;
        }
        else if(hit.normal.y < -0.9 && hit.moveDirection.y > 0 && velY > 0){
            velY = 0;
        }
    }

    private void FixedUpdate()
    {
        string newUIText = "";
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        //if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
        if (Physics.Raycast(camera.transform.position, camera.transform.up, out hit, Mathf.Infinity))
        {
            //Debug.DrawRay(camera.transform.position, camera.transform.up, Color.yellow, 1f, false);
            //Debug.Log(hit.collider.gameObject.GetComponent<CrystalController>());
            var obj = hit.collider.gameObject.GetComponent<InteractiveObject>();
//            InteractiveObject obj = hit.transform.GetComponent<InteractiveObject>();
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
            else if (hit.transform.gameObject.layer == 8 && isClicking && hit.distance < 2)
            {
                //Debug.Log("Walling");
                wallClicked = true;
                wallNormal = hit.normal;
                hitVector = hit.point;
            }
        }
        
        isClicking = false;
        if (uiText != null)
        {
            uiText.text = newUIText;
        }

        RaycastHit hit2;
        int layerMask = 1 << 8;
        layerMask = ~layerMask;
        if (isWalled)
        {
           // Debug.DrawRay(transform.position, -1f * transform.up, Color.blue, 0.5f);
            if (!Physics.Raycast(transform.position, -1f * transform.up, out hit2, 0.5f))
            {
                wallLeft = true;
            }
        }

        if (isAttacking)
        {
            animator.SetTrigger("attack");
            string playerHit = "";
            RaycastHit hit3;

            //Debug.DrawRay(camera.transform.position, camera.transform.up, Color.green, 1.5f);
            //Debug.DrawRay(camera.transform.position, camera.transform.forward, Color.blue, 1.5f);
            Debug.DrawRay(camera.transform.position, -1 * camera.transform.right, Color.red, 1.5f);
            // Does the ray intersect any objects excluding the player layer
            //if (Physics.Raycast(camera.transform.position, -1 * camera.transform.right, out hit3, Mathf.Infinity))
            if (Physics.SphereCast(camera.transform.position, 0.5f, -1 * camera.transform.right, out hit3, 1f))
            {

                Debug.Log("Did Hit");

                //PlayerController obj = hit.transform.GetComponent<PlayerController>();
                if (hit3.transform.Find("Head/Hand/Flashlight") != null)
                //if (obj != null)
                {

                    ClientConnection client = GameObject.FindObjectOfType<ClientConnection>();
                    foreach (string key in client.player_holder.Keys)
                    {
                        if (client.player_holder[key].Item1 == hit3.transform.gameObject)
                        {
                            playerHit = key;
                            break;
                        }
                    }
                    Debug.Log("killed " + playerHit);
                }
            }

            Dictionary<string, string> tcpAttackCommand = new Dictionary<string, string>();
            tcpAttackCommand["function"] = "monsterAction";
            tcpAttackCommand["action"] = "attack";
            tcpAttackCommand["playerHash"] = player_hash;
            tcpAttackCommand["playerHit"] = playerHit;
            AsyncTCPClient.Send(ClientConnection.dictmuncher(tcpAttackCommand));

            isAttacking = false;
        }
    }

    public GameObject getGameObject() {
        return gameObject;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            UnityEditor.EditorApplication.isPlaying = false; 
        }

        if(!Physics.CheckSphere(transform.position, 0.1f, 1<<8)){
            isGrounded = false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            isClicking = true;
        }

        if(Input.GetKeyDown(KeyCode.E))
        {
            isAttacking = true;
        }

        if (Input.GetKey(KeyCode.LeftControl)){
        speed = maxSpeed;
        }else{
            speed = maxSpeed/2;
        }

        if(Input.GetKey(KeyCode.LeftShift)){
            speed = maxSpeed;
        }else{
            speed = maxSpeed/2;
        }

        if(isGrounded){
            if(Input.GetKeyDown(KeyCode.Space)){
                velY = 4;
                isGrounded = false;
                animator.SetTrigger("jump");
                Dictionary<string, string> tcpJumpCommand = new Dictionary<string, string>();
                tcpJumpCommand["function"] = "monsterAction";
                tcpJumpCommand["action"] = "jump";
                tcpJumpCommand["playerHash"] = player_hash;
                tcpJumpCommand["playerHit"] = "";
                AsyncTCPClient.Send(ClientConnection.dictmuncher(tcpJumpCommand));
            }
            else{
                velY = 0;
            }
        }else{
            if (Input.GetKeyDown(KeyCode.Space) || wallLeft)
            {
                //Debug.Log("jumping off wall");
                gravity = -9.81f;
                isWalled = false;
                Vector3 newForward = transform.position;
                newForward.y = 0;
                if (newForward.x == 0 && newForward.z == 0)
                {
                    newForward = Vector3.forward;
                }
                transform.rotation = Quaternion.LookRotation(newForward.normalized, Vector3.up);
                wallLeft = false;
            }
            velY += gravity * Time.deltaTime;
        }
        //Debug.Log("grounded: " + isGrounded);

        if (wallClicked)
        {
            isWalled = true;
            isGrounded = false;

            Vector3 target = transform.forward - Vector3.Dot(transform.forward, wallNormal) * wallNormal;
            transform.rotation = Quaternion.LookRotation(target.normalized, wallNormal);
            transform.position = hitVector + .5f * wallNormal;

            gravity = 0;

            wallClicked = false;
        }

        float posX = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        float posZ = Input.GetAxis("Vertical") * speed * Time.deltaTime;

        int newAnimatorState;
        if (Mathf.Abs(controller.velocity.x) > 0 || Mathf.Abs(controller.velocity.z) > 0)
        {
            if (speed > 2.5) {
                newAnimatorState = 2;
            }
            else
            {
                newAnimatorState = 1;
            }
        }
        else
        {
            newAnimatorState = 0;
        }
        animator.SetInteger("movementState", newAnimatorState);
        /*if (animator.GetInteger("movementState") != newAnimatorState)
        {
            animator.SetInteger("movementState", newAnimatorState);
            Dictionary<string, string> tcpMoveCommand = new Dictionary<string, string>();
            tcpMoveCommand["function"] = "monsterAction";
            tcpMoveCommand["movementState"] = newAnimatorState.ToString();
            tcpMoveCommand["playerHash"] = player_hash;
            tcpMoveCommand["playerHit"] = "";
            AsyncTCPClient.Send(ClientConnection.dictmuncher(tcpMoveCommand));
        }*/

        if (isGrounded && (posX !=0 || posZ != 0))
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
        
        controller.Move(transform.forward * posX - transform.right * posZ + transform.up * posY);

        /////////////////////////////////

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = -1f * Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        //Debug.Log("MouseX: " + Input.GetAxis("Mouse X") + ", sens: " + mouseSensitivity + ", time: " + Time.deltaTime + ", total: " + mouseX);
        rotX -= mouseY;
        rotX = Mathf.Clamp(rotX, -85f, 85f);

        camera.localRotation = Quaternion.Euler(0f, 0f, -1f * rotX);
        //headBone.localRotation = Quaternion.Euler(rotX, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        /////////////////////////////////
    


        ////////////////////////////////



        float noise = Mathf.PerlinNoise(0, 10f*Time.time);

    }

    private void OnMouseDown()
    {
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
        {
            //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
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
        System.Random r = new System.Random();
        Int32 seed = (Int32) r.Next();
        Dictionary<string, string> seedRngArgs = new Dictionary<string, string>();
        seedRngArgs["function"] = "seedRng";
        seedRngArgs["seed"] = seed.ToString();
        AsyncTCPClient.Send(ClientConnection.dictmuncher(seedRngArgs));
        
    }

    public String get_player_hash() {
        return player_hash;
    }

    public Dictionary<String, String> getPositionDict()
    {
        Vector3 player_xyz_pos = gameObject.transform.position;
        Vector3 player_xyz_rot = gameObject.transform.eulerAngles;
        float head_x_rot = camera.eulerAngles.x;

        Dictionary<String, String> dict = new Dictionary<String, String> {
            {"player_hash", player_hash},
            {"body_posX", player_xyz_pos.x.ToString()},
            {"body_posY", player_xyz_pos.y.ToString()},
            {"body_posZ", player_xyz_pos.z.ToString()},
            {"head_rotX", head_x_rot.ToString()},
            {"body_rotX", player_xyz_rot.x.ToString()},
            {"body_rotY", player_xyz_rot.y.ToString()},
            {"body_rotZ", player_xyz_rot.z.ToString()},
            {"prefab_name", "crawler"},
        };

        return dict;
    }

    public void hideInCloset(GameObject closet)
    {
        transform.position = closet.transform.position;
    }
}
