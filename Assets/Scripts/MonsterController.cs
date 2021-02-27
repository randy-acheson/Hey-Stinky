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
    public float mouseSensitivity = 400f;

    private Transform headBone;
    
    private float maxSpeed;

    private Transform head;

    private CharacterController controller;
    //private Light flashlight;
    private float velY = 0f;
    private float rotX = 0;
    private float movement = 0f;
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
        controller = GetComponent<CharacterController>();
        headBone = transform.Find("Body/Armature/Bone/Bone.003/Bone.004/Bone.005/Bone.006/Bone.007");
        head = transform.Find("Head");
        player_hash = generatePlayerHash();
        uiText = GetComponentInChildren<Text>();

        Cursor.lockState = CursorLockMode.Locked;

        animator = transform.Find("Body").GetComponent<Animator>();
        maxSpeed = speed;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //Debug.Log("hit: " + hit.normal);
        if(!isWalled && hit.normal.y > 0.1){
            // Debug.Log("hitting wrong thing");
            gravity = -9.81f;
        }
    }

    private void FixedUpdate()
    {
        string newUIText = "";
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(head.transform.position, head.transform.forward, out hit, Mathf.Infinity))
        {
            //Debug.Log(hit.collider.gameObject.GetComponent<CrystalController>());
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
            else if (hit.transform.gameObject.layer == 8 && isClicking && hit.distance < 2)
            {
                //Debug.Log("Walling");
                wallClicked = true;
                wallNormal = hit.normal;
                hitVector = hit.point;
                Debug.Log("hit: " + hit.point);
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
            if (!Physics.Raycast(transform.position, -1f * transform.up, out hit2, 0.5f))
            {
                //wallLeft = true;
            }
        }

        if (isAttacking)
        {
            animator.SetTrigger("attack");
            string playerHit = "";
            RaycastHit hit3;

            // Does the ray intersect any objects excluding the player layer
            if (Physics.SphereCast(head.transform.position, 0.5f, head.transform.forward, out hit3, 1f))
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

        float posY = -0.02f;

        if((controller.collisionFlags & CollisionFlags.Below) != 0){
            if(Input.GetKeyDown(KeyCode.Space)){
                velY = 4;
                posY = velY * Time.deltaTime;
                
                animator.SetTrigger("jump");
                Dictionary<string, string> tcpJumpCommand = new Dictionary<string, string>();
                tcpJumpCommand["function"] = "monsterAction";
                tcpJumpCommand["action"] = "jump";
                tcpJumpCommand["playerHash"] = player_hash;
                tcpJumpCommand["playerHit"] = "";
                AsyncTCPClient.Send(ClientConnection.dictmuncher(tcpJumpCommand));
                
            }else{
                velY = 0;
            }
        }else if ((controller.collisionFlags & CollisionFlags.Above) != 0){
            velY = 0;
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
            posY = velY * Time.deltaTime;
        }

        if (wallClicked)
        {
            isWalled = true;

            //Vector3 target = transform.up - Vector3.Dot(transform.up, wallNormal) * wallNormal;
            //Debug.Log("wall normal: " + wallNormal);
            Vector3 target = head.transform.forward - Vector3.Dot(head.transform.forward, wallNormal) * wallNormal;
            //Debug.Log("head: " + head.transform.forward + ", target: " + target + ", wall: " + wallNormal);
            if(target.magnitude == 0){
                target = transform.up;
            }
            transform.rotation = Quaternion.LookRotation(target.normalized, wallNormal);
            //Debug.Log("final: " + transform.forward + ", finalcam: " + head.transform.forward);

            //transform.position = hitVector;
            controller.Move(hitVector - transform.position);
            Debug.Log("pos: " + transform.position);
            rotX = 0;

            gravity = 0;

            wallClicked = false;
        }

        float posX = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        float posZ = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        
        controller.Move(transform.forward * posZ + transform.right * posX + transform.up * posY);

        int newAnimatorState = 0;
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
        
        //animator.SetInteger("movementState", newAnimatorState);
        if (animator.GetInteger("movementState") != newAnimatorState)
        {
            animator.SetInteger("movementState", newAnimatorState);
            
            Dictionary<string, string> tcpMoveCommand = new Dictionary<string, string>();
            tcpMoveCommand["function"] = "monsterAction";
            tcpMoveCommand["movementState"] = newAnimatorState.ToString();
            tcpMoveCommand["playerHash"] = player_hash;
            tcpMoveCommand["playerHit"] = "";
            AsyncTCPClient.Send(ClientConnection.dictmuncher(tcpMoveCommand));
            
        }

        /////////////////////////////////

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = -1f * Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        //Debug.Log("MouseX: " + Input.GetAxis("Mouse X") + ", sens: " + mouseSensitivity + ", time: " + Time.deltaTime + ", total: " + mouseX);
        rotX += mouseY;
        rotX = Mathf.Clamp(rotX, -85f, 85f);

        head.localRotation = Quaternion.Euler(rotX, 0f, 0f);
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
                other.gameObject.GetComponent<CrystalController>()
                    .SetTransformParent(gameObject.transform);
                gameObject.transform.position = new Vector3(10, 10, 10);
            }
        }
        else if (other.gameObject.CompareTag("Receptacle") && crystal != null)
        {
            crystal.GetComponent<CrystalController>()
                .SetTransformParent(other.gameObject.transform);
            crystal.GetComponent<CrystalController>().isDeposited = true;
            crystal = null;
        }
    }

    public String get_player_hash() {
        return player_hash;
    }

    public Dictionary<String, String> getPositionDict()
    {
        Vector3 player_xyz_pos = transform.position;
        Vector3 player_xyz_rot = transform.eulerAngles;
        float head_x_rot = head.eulerAngles.x;

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
