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
    WallSwimming,
    EnemyInk
}


public class PlayerController : MonoBehaviour
{
    protected const float GRAVITY_CONSTANT = 9.8f;
    protected const float STOP_MIN_THRESHOLD = .2f;

    [Header("==== Child object references ====")]
    /** Child obj references **/
    public GameObject mesh;
    public ThirdPersonCamera cameraControls;
    public CapsuleCollider capsule;
    public Shooter shooter;
    public Animator animator;

    /** Internal objects **/
    protected PlayerControls playerControls;
    protected SplatmapReader splatmapReader;

    /** Movement state control **/
    [SerializeField]
    protected Vector4 team;
    [Header("==== TESTONLY exposd for testing purposes, do not edit. ====")]
    [SerializeField]
    protected MovementState currentMovementState;
    [SerializeField]
    protected bool isSquid;
    protected RaycastHit surfaceProbeHit;

    /** Input variables **/
    [Tooltip("Variables with Input in their name are clamped to max speed accel & speed in UpdatePhysics. Generally used for horizontal movement.")]
    protected Vector3 pendingInput, pendingInputForce, inputVelocity, pendingVerticalInput;
    [Tooltip("Vertical input variables are added after input has been clamped, and are not clamped themselves. Keeps gravity consistent even when over the max speed.")]
    protected Vector3 verticalVelocity;
    [Tooltip("Normal of the most recently handled collision. Zeroed when not colliding.")]
    protected Vector3 groundNormal;
    [SerializeField, Tooltip("True if the character is colliding.")]
    protected bool grounded;

    [Header("==== Movement paramters ====")]
    /** Adjustable properties **/
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
    protected float inkAlphaMinThreshold;
    [SerializeField]
    protected float updateMovementStateDelay = 0.032f;
    [SerializeField]
    protected float enemyInkSlowingFactor;

    public float testGravMult;
    public float testBrakeMult;

    protected void Awake()
    {
        //Controls setup
        playerControls = new PlayerControls();
        playerControls.Enable();
        playerControls.Walking.Squid.performed += EnterSquid;
        playerControls.Walking.Squid.canceled += ExitSquid;
        playerControls.Walking.Shoot.performed += OnPressedShoot;
        playerControls.Walking.Shoot.canceled += OnReleaseShoot;

        //Default values
        defaultGravityScale = gravityScale;
        baseMaxHorizontalSpeed = maxHorizontalSpeed;
        currentMovementState = MovementState.Walking;
        isSquid = false;

        // Object initialization
        splatmapReader = new SplatmapReader();

        // Important: Start movement state update loop.
        Invoke("UpdateMovementState", .032f);
    }

    protected void Update()
    {
        // Camera rotation
        Vector2 mouseDelta = playerControls.Walking.Camera.ReadValue<Vector2>();
        cameraControls.CameraUpdate(mouseDelta.x, mouseDelta.y);
        mesh.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, cameraControls.yRotation, transform.rotation.eulerAngles.z);
        shooter.transform.rotation = Quaternion.LookRotation(cameraControls.cameraTransform.forward);

        UpdateAnimation();
    }

    protected void FixedUpdate()
    {
        UpdatePhysics();
        
    }

    /// <summary>
    /// Probes the surface, looking for a SplatableObject to read ink values from.
    /// This function gets the values and calls ReadPixel() on SplatmapReader, which in turn calls 
    /// FinishUpdateMovementState() once it finished reading the pixel value.
    /// </summary>
    protected void UpdateMovementState()
    {
        SplatableObject splatObj;

        // Forward probe
        RaycastHit hit;
        Vector3 origin = capsule.transform.TransformPoint(capsule.center);
        //origin.y -= (capsule.radius);
        Ray ray = new Ray(origin, mesh.transform.forward);
        bool isValidHit = Physics.Raycast(ray, out hit, capsule.radius + .1f);
        surfaceProbeHit = hit;

        /** TESTONLY **/ 
        Debug.DrawRay(ray.origin, ray.direction, Color.blue, .1f);
        /** ENDTEST **/

        if(isValidHit && currentMovementState != MovementState.EnemyInk)
        {
            //print(hit.collider.gameObject.name);

            
            splatObj = hit.collider.GetComponent<SplatableObject>();
            if(splatObj)
            {
                splatmapReader.ReadPixel(splatObj.Splatmap, hit.textureCoord, FinishUpdateMovementSate);
                return;
            }
        }

        // Downward probe.
        hit = new RaycastHit();
        origin = capsule.transform.TransformPoint(capsule.center);
        ray = new Ray(origin, Vector3.down);
        isValidHit = Physics.Raycast(ray, out hit, capsule.height + capsule.radius + .1f);
        surfaceProbeHit = hit;

        /** TESTONLY **/
        Debug.DrawRay(ray.origin, ray.direction * (capsule.height + capsule.radius + .1f), Color.red, .1f);
        /** ENDTEST **/

        if (!isValidHit)
        {
            FinishUpdateMovementSate(Color.clear);
            return;
        }

        splatObj = hit.collider.GetComponent<SplatableObject>();

        if (splatObj)
        {
            splatmapReader.ReadPixel(splatObj.Splatmap, hit.textureCoord, FinishUpdateMovementSate);
        }
        else FinishUpdateMovementSate(Color.clear);
    }

    /// <summary>
    /// Updates the movement state using color as the ink color and surfaceProbeHit as the surface.
    /// Invokes UpdateMovementState() after finishing, to maintain the update movement state loop. To
    /// cancel the update movement state loop, call CancelInvoke().
    /// </summary>
    /// <param name="color"></param>
    protected void FinishUpdateMovementSate(Color color)
    {
        // This function can be called late by SplatmapReader, so make sure this still exists before doing anything.
        if (!this) return;

        //print(color);

        if(surfaceProbeHit.collider && Vector3.Dot(surfaceProbeHit.normal, Vector3.up) - slopeCheckTolerance < minSlopeGradation)
        {

            if (color.a > inkAlphaMinThreshold && color.r > color.g &&  isSquid)
            {
                //print("WallSwimming");
                currentMovementState = MovementState.WallSwimming;
                //inputVelocity = Vector3.zero;
                maxHorizontalSpeed = Mathf.SmoothStep(maxHorizontalSpeed, baseMaxHorizontalSpeed * 1f, .1f);
                grounded = false;
                Invoke("UpdateMovementState", updateMovementStateDelay);
                return;
            }
        }

        // Enemy ink
        if (color.a > inkAlphaMinThreshold && color.g > color.r)
        {
            if (currentMovementState == MovementState.EnemyInk)
            {
                Invoke("UpdateMovementState", updateMovementStateDelay);
                return;
            }
            print("EnemyInk");
            currentMovementState = MovementState.EnemyInk;
            maxHorizontalSpeed = baseMaxHorizontalSpeed * enemyInkSlowingFactor;
            isSquid = false;
            Invoke("UpdateMovementState", updateMovementStateDelay);
            return;
        }

        // Swimming
        if (color.a > inkAlphaMinThreshold && color.r > color.g && isSquid && grounded)
        {
            if(currentMovementState == MovementState.Swimming)
            {
                Invoke("UpdateMovementState", updateMovementStateDelay);
                return;
            }
            print("Swimming");
            currentMovementState = MovementState.Swimming;
            maxHorizontalSpeed = baseMaxHorizontalSpeed * 2f;
            if (playerControls.Walking.Squid.IsPressed() && !isSquid) EnterSquid(new InputAction.CallbackContext());
            Invoke("UpdateMovementState", updateMovementStateDelay);
            return;
        }

        // Default case
        print("Walking");
        currentMovementState = MovementState.Walking;
        maxHorizontalSpeed = Mathf.SmoothStep(maxHorizontalSpeed, baseMaxHorizontalSpeed, .1f);  //baseMaxHorizontalSpeed;
        if (playerControls.Walking.Squid.IsPressed() && !isSquid) EnterSquid(new InputAction.CallbackContext());
 
        

        //print("Finished Update Movement State at "+ Time.time);

        Invoke("UpdateMovementState", updateMovementStateDelay);
        return;
    }

    /// <summary>
    /// Update the value of pending input, accounting for current movement state.
    /// </summary>
    /// <returns></returns>
    private void GetInput() 
    {
        pendingInput = new Vector3(playerControls.Walking.MovementInput.ReadValue<Vector2>().x, 0, playerControls.Walking.MovementInput.ReadValue<Vector2>().y);
        
        //print(inputVelocity.y + verticalVelocity.y);

        if (isSquid && currentMovementState == MovementState.WallSwimming  /*Vector3.Dot(surfaceProbeHit.normal, Vector3.up) - slopeCheckTolerance < minSlopeGradation && surfaceProbeHit.collider*/)
        {
            //print("Wall Swimmming");

            grounded = false;
            //gravityScale = 0;

            pendingVerticalInput = Quaternion.LookRotation(-surfaceProbeHit.normal, transform.up) * Quaternion.Euler(-90f, 0, 0) * pendingInput;
            pendingVerticalInput.x = 0; pendingVerticalInput.z = 0;
            pendingInput = Quaternion.LookRotation(-surfaceProbeHit.normal, transform.up) * Quaternion.Euler(-90f, 0, 0) * pendingInput;
            pendingInput.y = 0;

            //maxHorizontalSpeed = baseMaxHorizontalSpeed;

            //print("pen inp" + pendingInput);
        }
        else if(isSquid && !surfaceProbeHit.collider && verticalVelocity.y < 0f)
        {
            maxAcceleration = 999f;
            print("peak");
            pendingInput = Quaternion.LookRotation(mesh.transform.forward, Vector3.up) * pendingInput;
        }
        else if(isSquid && !surfaceProbeHit.collider && !grounded)
        {
            grounded = false;
            print("Floating");
            pendingInput = Quaternion.LookRotation(mesh.transform.forward, Vector3.up) * pendingInput;

            maxAcceleration = 10f;

            gravityScale = defaultGravityScale;

            //pendingInput = Quaternion.LookRotation(mesh.transform.forward, Vector3.up) * pendingInput;
        }
        else
        {
            //print("else");
            braking = 20;
            maxAcceleration = 999f;
            gravityScale = defaultGravityScale;
            //maxHorizontalSpeed = baseMaxHorizontalSpeed;
            pendingInput = Quaternion.LookRotation(mesh.transform.forward, Vector3.up) * pendingInput;
        }


        //pendingInput = Vector3.zero;
        Debug.DrawLine(transform.position, transform.position + pendingInput * 5, Color.red, .1f); 
    }

    /// <summary>
    /// Updates physics variables and move character.
    /// </summary>
    protected void UpdatePhysics() 
    {

        GetInput();
        AddInputForce(pendingInput * inputStrength);
        AddFriction();

        // Calc acceleration, Ignoring mass.
        Vector3 a = pendingInputForce;

        if (a.magnitude >= maxAcceleration) a = a.normalized * maxAcceleration;
        if ((inputVelocity + a * Time.deltaTime).magnitude > maxHorizontalSpeed)
        {     
            a = ((a.normalized * maxHorizontalSpeed - inputVelocity) / Time.deltaTime);
        }
      
        inputVelocity += a * Time.fixedDeltaTime;

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
            verticalVelocity += Vector3.down * GRAVITY_CONSTANT * gravityScale * Time.fixedDeltaTime;

            //if (verticalVelocity.y > 10f) verticalVelocity.y = 10f;

            //print(verticalVelocity);

            delta += verticalVelocity * Time.fixedDeltaTime;
        }

        //print("Delta: "+delta);

        transform.position += delta;
        pendingInputForce = Vector3.zero;
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

    /// <summary>
    /// Update collision related globals and correct our position.
    /// NOTE velocity is not negated when hitting a wall, meaning in certain cases the player can appear to get
    /// "stuck" to a wall as they overcome their opposing velocity. This project has high braking and acceleration
    /// values, meaning this isn't going to crop up at this time.
    /// </summary>
    /// <param name="collision"></param>
    protected void HandleCollision(Collision collision)
    {
        // Check if the slope is walkable
        if (Vector3.Dot(collision.GetContact(0).normal, Vector3.up) - slopeCheckTolerance > minSlopeGradation)
        {
            grounded = true;
            verticalVelocity.y = 0f;
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

    public void AddInputForce(Vector3 force) 
    {

        verticalVelocity += pendingVerticalInput * Time.fixedDeltaTime * testGravMult;

        pendingVerticalInput = Vector3.zero;

        pendingInputForce += force; 
    }

    /// <summary>
    /// Going to keep this as a simple braking factor.
    /// Strong enough braking also keeps us from maintaining inputVelocity while running into walls.
    /// </summary>
    /// <param name="collision"></param>
    protected void AddFriction()
    {
        pendingInputForce += braking * -inputVelocity.normalized;
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
        //print("Walking");
        currentMovementState = MovementState.Walking;
        maxHorizontalSpeed = baseMaxHorizontalSpeed;
    }

    protected void OnPressedShoot(InputAction.CallbackContext context)
    {
        print("start shooting");
        shooter.StartShooting();
    }

    protected void OnReleaseShoot(InputAction.CallbackContext context)
    {
        print("stop shooting");
        shooter.StopShooting();
    }

    protected void UpdateAnimation()
    {

        if (currentMovementState == MovementState.EnemyInk) animator.SetBool("EnemyInk", true);
        else animator.SetBool("EnemyInk", false);

        if (grounded) animator.SetFloat("InputSpeed", inputVelocity.magnitude);
        else animator.SetFloat("InputSpeed", 0f);

        if (isSquid)
        {
            animator.SetTrigger("EnterSquid");
            animator.ResetTrigger("ExitSquid");
        }
        else
        {
            animator.SetTrigger("ExitSquid");
            animator.ResetTrigger("EnterSquid");
        }

        if (currentMovementState == MovementState.WallSwimming || currentMovementState == MovementState.Swimming)
        {
            mesh.GetComponent<MeshRenderer>().enabled = false;
            //mesh.gameObject.SetActive(false);
        }
        else
        {
            mesh.GetComponent<MeshRenderer>().enabled = true;
            //mesh.gameObject.SetActive(true);
        }
    }
}   
