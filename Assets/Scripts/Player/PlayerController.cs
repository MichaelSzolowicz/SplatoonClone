using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// [Player Controller]
/// [05-01-23]
/// [Szolowicz, Michael]
/// Defines actions a default player characteris capable of. 
/// </summary>
public class PlayerController : MonoBehaviour
{
    public Rigidbody rigibody;

    protected const float GRAVITY_CONSTANT = 9.8f;

    protected PlayerControls playerControls;

    protected Vector3 pendingInput;
    protected Vector3 pendingForce;
    protected Vector3 velocity;
    protected Vector3 groundNormal;

    protected Vector3 gravVel;

    [SerializeField]
    protected bool grounded;

    [SerializeField]
    protected float gravityScale = 1.0f;
    [SerializeField]
    protected float maxAcceleration = 1.0f;
    [SerializeField]
    protected float maxSpeed = 5.0f;
    /// <summary>
    /// Magnitude of force to apply on input.
    /// </summary>
    [SerializeField]
    protected float inputStrength = 1.0f;
    [SerializeField]
    protected float braking = 0.0f;
    [SerializeField]
    protected float frictionCoefficient = 100.0f;

    protected void Awake()
    {
        playerControls = new PlayerControls();
        playerControls.Enable();
    }

    protected void FixedUpdate()
    {

        UpdatePhysics();
    }

    private Vector2 GetInput() 
    {
        pendingInput = new Vector3(playerControls.Walking.MovementInput.ReadValue<Vector2>().x, 0, playerControls.Walking.MovementInput.ReadValue<Vector2>().y) * inputStrength;

        // is actually ground collision. Need better checking system later
        /*
        if(Vector3.Dot(groundNormal, Vector3.up) > 0f)
        {
            pendingInput = Vector3.ProjectOnPlane(pendingInput, groundNormal);
        } */
        

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
        Collision delteMe = new Collision();
        AddFriction(delteMe);

        //print("Vel: "+velocity);
        //print("Inp: " + pendingInput);

        //AddGravity();

        // Ignoring mass.
        Vector3 a = pendingForce;
        if (a.magnitude >= maxAcceleration) a = a.normalized * maxAcceleration;

        velocity += a * Time.fixedDeltaTime;
        if (velocity.magnitude >= maxSpeed) velocity = velocity.normalized * maxSpeed;

        Vector3 delta = velocity * Time.fixedDeltaTime;

        /* FIXME: Anyhwere I cehck the normal / up dot, I should use a threshold instead of a finite check. */
        if (Vector3.Dot(groundNormal, Vector3.up) > .0f && grounded)
        {
            // Maintain velocity parallel to floor
            float magnitude = delta.magnitude;
            delta = Vector3.ProjectOnPlane(delta, groundNormal);
            delta = delta.normalized * magnitude;
        }

        // Keep gravity calculation seperate. If they get normalized when the character goes overspeed, horizontally moving charcters will fall more slowly.
        if (!grounded)
        {
            print("apply grav");
            gravVel += Vector3.down * GRAVITY_CONSTANT * gravityScale * Time.fixedDeltaTime;
            if (gravVel.magnitude >= maxSpeed) gravVel = gravVel.normalized * maxSpeed;
            delta += gravVel * Time.fixedDeltaTime;
        } 

       // print(gravVel);

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
        /* FIXME: Anyhwere I cehck the normal / up dot, I should use a threshold instead of a finite check. */
        if (Vector3.Dot(collision.GetContact(0).normal, Vector3.up) > .0f)
        {
            grounded = true;
            print(Vector3.Dot(collision.GetContact(0).normal, Vector3.up));
            gravVel.y = 0f;
        }
            
        
        groundNormal = collision.GetContact(0).normal;
        HandleCollision(collision);
    }

    protected void OnCollisionStay(Collision collision)
    {
        /* FIXME: Anyhwere I cehck the normal / up dot, I should use a threshold instead of a finite check. */
        if (Vector3.Dot(collision.GetContact(0).normal, Vector3.up) > .0f)
        {
            grounded = true;
            //print(Vector3.Dot(collision.GetContact(0).normal, Vector3.up));
        }

        HandleCollision(collision);
    }

    protected void HandleCollision(Collision collision)
    {
        groundNormal = collision.GetContact(0).normal;

        Vector3 correction;
        float distance;
        Physics.ComputePenetration(GetComponent<CapsuleCollider>(), transform.position, transform.rotation, collision.collider, collision.transform.position, collision.transform.rotation, out correction, out distance);

        // print(correction + " : " + distance);
        if (distance > 0) { }
            transform.Translate(correction * distance);

    }

    public void AddForce(Vector3 force) 
    {
        pendingForce += force;
    }

    public void AddFriction(Collision collision)
    {
        pendingForce += frictionCoefficient * -velocity;

        //braking
        if(playerControls.Walking.MovementInput.ReadValue<Vector2>().magnitude == 0.0f)
        {
            //pendingForce += braking * -velocity.normalized;
            //velocity *= braking;
        }
        
        
        //if (pendingForce.magnitude <= .02f) pendingForce = Vector3.zero;
    }

    public void AddGravity() 
    {

        pendingForce += Vector3.down * GRAVITY_CONSTANT * gravityScale;
    }

    public void AddGroundFriction()
    {

    }
}
