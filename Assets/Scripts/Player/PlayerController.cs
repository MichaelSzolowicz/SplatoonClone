using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// [Player Controller]
/// [05-01-23]
/// [Szolowicz, Michael]
/// Defines actions a default player character is capable of. 
/// </summary>
/// 


public enum MovementState
{
    Walking,
    Swimming,
    SwimmingWall,
    EnemyInk
}


public class PlayerController : MonoBehaviour
{
    protected const float GRAVITY_CONSTANT = 9.8f;
    protected const float STOP_MIN_THRESHOLD = .2f;

    public Rigidbody rigibody;
    public GameObject mesh;
    public ThirPersonCamera thirdPersonCamera;

    protected PlayerControls playerControls;
    protected SplatmapReader splatmapReader;

    public MovementState currentMovementState;       /** FIXME public for testing purpoises only **/
    protected delegate void UpdateMovementStateDelgate();
    UpdateMovementStateDelgate updateMovementStateDelgate;
    public bool isSquid;
    public CapsuleCollider capsule;
    public Vector4 team;

    protected Vector3 pendingInput;
    protected Vector3 pendingForce;
    protected Vector3 inputVelocity;
    protected Vector3 gravityVelocity;
    protected Vector3 groundNormal;

    [SerializeField, Tooltip("True if the character is colliding.")]
    protected bool grounded;
    [SerializeField]
    protected float minSlopeGradation = .0f;
    [SerializeField]
    protected float slopeCheckTolerance = .01f;
    [SerializeField]
    protected float gravityScale = 1.0f;
    protected float defaultGravityScale;
    [SerializeField]
    protected float maxAcceleration = 1.0f;
    [SerializeField]
    protected float maxHorizontalSpeed = 5.0f;
    protected float baseMaxHorizontalSpeed;
    [SerializeField, Tooltip("Magnitude of force to apply on input.")]
    protected float inputStrength = 1.0f;
    [SerializeField, Tooltip("Constant force applied opposing input velocity.")]
    protected float braking = 0.0f;

    [SerializeField]
    protected float updateMovementStateDelay = 0.032f;

    protected void Awake()
    {
        //Cursor.visible = false;
        playerControls = new PlayerControls();
        playerControls.Enable();
        playerControls.Walking.Squid.performed += EnterSquid;
        playerControls.Walking.Squid.canceled += ExitSquid;

        currentMovementState = MovementState.Walking;
        isSquid = false;
        defaultGravityScale = gravityScale;

        splatmapReader = new SplatmapReader();

        baseMaxHorizontalSpeed = maxHorizontalSpeed;

        Invoke("UpdateMovementState", .032f);
    }

    protected void Update()
    {
        // Camera rotation
        Vector2 mouseDelta = playerControls.Walking.Camera.ReadValue<Vector2>();
        thirdPersonCamera.CameraUpdate(mouseDelta.x, mouseDelta.y);
        mesh.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, thirdPersonCamera.yRotation, transform.rotation.eulerAngles.z);
    }

    protected void FixedUpdate()
    {
        //UpdateMovementState();
        UpdatePhysics();
    }

    protected void UpdateMovementState()
    {
        // Downward probe.
        RaycastHit hit;
        Ray ray = new Ray(transform.position, Vector3.down);
        bool isValidHit = Physics.Raycast(ray, out hit, (capsule.height + .1f) / 2);

        /** TESTONLY **/
        Debug.DrawLine(transform.position, transform.position - Vector3.up * (capsule.height + .1f) / 2, Color.red, .1f);

        if (!isValidHit)
        {
            FinishUpdateMovementSate(Color.clear);
            return;
        }

        SplatableObject splatObj = hit.collider.GetComponent<SplatableObject>();

        if (splatObj)
        {
            splatmapReader.ReadPixel(splatObj.Splatmap, hit.textureCoord, FinishUpdateMovementSate);
        }
        else FinishUpdateMovementSate(Color.clear);

        /**
        Color color = new Color();
        RaycastHit hit;
        SplatableObject splatObj;

        // Horizontal probe.
        Ray ray = new Ray(transform.position, mesh.transform.forward);
        bool isValidHit = Physics.Raycast(ray, out hit, capsule.radius + .1f);

        /** TESTONLY 
        Debug.DrawRay(ray.origin, ray.direction, Color.blue, .1f);

        if(isValidHit && isSquid)
        {
            
            splatObj = hit.collider.GetComponent<SplatableObject>();

            if (splatObj)
            {
                
                color = splatmapReader.ReadPixel(splatObj.Splatmap, hit.textureCoord, FinishUpdateMovementSate);
                print(color);

                if (color.r > .5f)
                {
                    
                    print("SwimmingWall");
                }
            }
        }
        */
    }

    protected void FinishUpdateMovementSate(Color color)
    {
        // Enemy ink
        if (color.g > .5f && currentMovementState != MovementState.EnemyInk)
        {
            print("EnemyInk");
            currentMovementState = MovementState.EnemyInk;
            maxHorizontalSpeed = baseMaxHorizontalSpeed * .5f;
            isSquid = false;

        }
        // Swimming
        else if (color.r > .5f && isSquid && currentMovementState != MovementState.Swimming)
        {
            print("Swimming");
            currentMovementState = MovementState.Swimming;
            maxHorizontalSpeed = baseMaxHorizontalSpeed * 2f;
            if (playerControls.Walking.Squid.IsPressed() && !isSquid) EnterSquid(new InputAction.CallbackContext());
        }
        // Default case
        else if (color.g < .5f && color.r < .5 && currentMovementState != MovementState.Walking)
        {
            print("Walking");
            currentMovementState = MovementState.Walking;
            maxHorizontalSpeed = baseMaxHorizontalSpeed;
            if (playerControls.Walking.Squid.IsPressed() && !isSquid) EnterSquid(new InputAction.CallbackContext());
        }

        print("Finished Update Movement State at "+ Time.time);

        Invoke("UpdateMovementState", updateMovementStateDelay);
    }

    private Vector2 GetInput() 
    {
        pendingInput = new Vector3(playerControls.Walking.MovementInput.ReadValue<Vector2>().x, 0, playerControls.Walking.MovementInput.ReadValue<Vector2>().y);


        if (false && isSquid && currentMovementState == MovementState.SwimmingWall)
        {
            //moveMode = MovementMode.SwimmingWall;
            //gravityScale = 0f;
            //gravityVelocity = Vector3.zero;
            //grounded = false;
            //pendingInput = Quaternion.LookRotation(-hit.normal, transform.up) * Quaternion.Euler(-90f, 0, 0) * pendingInput;
        }
        else
        {
            pendingInput = Quaternion.LookRotation(-transform.up, mesh.transform.forward) * Quaternion.Euler(-90f, 0, 0) * pendingInput;
            //moveMode = MovementMode.Walking;
            gravityScale = defaultGravityScale;
        }

        Debug.DrawLine(transform.position, transform.position + pendingInput * 5, Color.red, .1f); 

        return pendingInput;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="inputVector"> 2D input vector, assumed to be normalized. </param>
    /// <param name="strength"> Multiplier for input vector. IE magnitude of force. </param>
    private void AddInput(Vector2 inputVector, float strength) 
    {
        pendingForce += pendingInput * inputStrength;
    }

    protected void UpdatePhysics() 
    {
        AddInput(GetInput(), inputStrength);
        AddFriction();

        // Calc acceleration, Ignoring mass.
        Vector3 a = pendingForce;
        if (a.magnitude >= maxAcceleration) a = a.normalized * maxAcceleration;

        // Lock max & min speed.
        inputVelocity += a * Time.fixedDeltaTime;
        if (inputVelocity.magnitude >= maxHorizontalSpeed) inputVelocity = inputVelocity.normalized * maxHorizontalSpeed;
        if (inputVelocity.magnitude <= STOP_MIN_THRESHOLD) inputVelocity = Vector3.zero;

        Vector3 delta = inputVelocity * Time.fixedDeltaTime;

        // Maintain velocity parallel to floor
        if (Vector3.Dot(groundNormal, Vector3.up) - slopeCheckTolerance > minSlopeGradation && grounded)
        {
            float magnitude = delta.magnitude;
            delta = Vector3.ProjectOnPlane(delta, groundNormal);
            delta = delta.normalized * magnitude;
        }

        // Keep gravity calculation seperate. If they get normalized when the character goes overspeed, horizontally moving charcters will fall more slowly.
        if (!grounded)
        {
            gravityVelocity += Vector3.down * GRAVITY_CONSTANT * gravityScale * Time.fixedDeltaTime;
            delta += gravityVelocity * Time.fixedDeltaTime;
        }



        transform.position += delta;
        pendingForce = Vector3.zero;
    }

    protected void OnCollisionExit(Collision collision)
    {
        grounded = false;
        groundNormal = Vector3.zero;
    }

    protected void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision);
    }

    protected void OnCollisionStay(Collision collision)
    {
        HandleCollision(collision);
    }

    protected void HandleCollision(Collision collision)
    {
        // Check if the slope is walkable
        if (Vector3.Dot(collision.GetContact(0).normal, Vector3.up) - slopeCheckTolerance > minSlopeGradation)
        {
            grounded = true;
            gravityVelocity.y = 0f;
        }

        groundNormal = collision.GetContact(0).normal;

        // Correct our position wo the capsule doesn't clip inside other geometry.
        Vector3 correction;
        float distance;
        Physics.ComputePenetration(GetComponent<CapsuleCollider>(), transform.position, transform.rotation, collision.collider, collision.transform.position, collision.transform.rotation, out correction, out distance);

        if (distance > 0)
        {
            // Only apply the correction if it is valid.
            transform.Translate(correction * distance);
        }
    }

    public void AddForce(Vector3 force) 
    {
        pendingForce += force;
    }

    /// <summary>
    /// Going to keep this as a simple braking factor.
    /// Strong enough braking also keeps us from maintaining inputVelocity while running into walls.
    /// </summary>
    /// <param name="collision"></param>
    protected void AddFriction()
    {
        pendingForce += braking * -inputVelocity.normalized;
    }

    protected void EnterSquid(InputAction.CallbackContext context)
    {
        print("Enter Squid");

        // Can't swim in enemy ink
        if (currentMovementState == MovementState.EnemyInk) return;

        isSquid = true;
    }

    protected void ExitSquid(InputAction.CallbackContext context)
    {
        print("Exit Squid");

        isSquid = false;
        print("Walking");
        currentMovementState = MovementState.Walking;
        maxHorizontalSpeed = baseMaxHorizontalSpeed;
    }
}   
