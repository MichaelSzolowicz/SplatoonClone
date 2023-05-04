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
public class PlayerController : MonoBehaviour
{
    protected const float GRAVITY_CONSTANT = 9.8f;
    protected const float STOP_MIN_THRESHOLD = .2f;

    public Rigidbody rigibody;
    public GameObject mesh;
    public ThirPersonCamera thirdPersonCamera;

    protected PlayerControls playerControls;

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
    [SerializeField]
    protected float maxAcceleration = 1.0f;
    [SerializeField]
    protected float maxHorizontalSpeed = 5.0f;
    [SerializeField, Tooltip("Magnitude of force to apply on input.")]
    protected float inputStrength = 1.0f;
    [SerializeField, Tooltip("Constant force applied opposing input velocity.")]
    protected float braking = 0.0f;

    protected void Awake()
    {
        Cursor.visible = false;
        playerControls = new PlayerControls();
        playerControls.Enable();
    }

    protected void Update()
    {
        Vector2 mouseDelta = playerControls.Walking.Camera.ReadValue<Vector2>();
        thirdPersonCamera.CameraUpdate(mouseDelta.x, mouseDelta.y);
        mesh.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, thirdPersonCamera.yRotation, transform.rotation.eulerAngles.z);
    }

    protected void FixedUpdate()
    {
        UpdatePhysics();
    }

    private Vector2 GetInput() 
    {
        pendingInput = new Vector3(playerControls.Walking.MovementInput.ReadValue<Vector2>().x, 0, playerControls.Walking.MovementInput.ReadValue<Vector2>().y);
        Vector3 worldSpaceInput = Quaternion.LookRotation(-transform.up, mesh.transform.forward) * Quaternion.Euler(-90f, 0, 0) * pendingInput;
        pendingInput = worldSpaceInput;

        pendingInput *= inputStrength;

        Debug.DrawLine(transform.position, transform.position + pendingInput * 5, Color.red, 1.0f);

        return pendingInput;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="inputVector"> 2D input vector, assumed to be normalized. </param>
    /// <param name="strength"> Multiplier for input vector. IE magnitude of force. </param>
    private void AddInput(Vector2 inputVector, float strength) 
    {
        pendingForce += pendingInput;
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
}   
