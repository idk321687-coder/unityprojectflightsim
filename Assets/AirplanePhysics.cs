using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class SimpleFlightControls : MonoBehaviour
{
    [Header("Engine")]
    public float maxSpeed = 120f;
    public float throttleChangeSpeed = 0.5f;

    [Range(0f, 1f)]
    public float throttle = 1f;

    [Header("Flight Controls")]
    public float pitchPower = 5f;
    public float rollPower = 12f;
    public float yawPower = 2.5f;

    [Header("Turning")]
    public float bankTurnPower = 8f;

    [Header("Stability")]
    public float pitchStability = 2f;
    public float rollStability = 3f;
    public float yawStability = 1f;

    [Header("Crash Physics")]
    public float landingSpeedThreshold = 15f;
    public float crashSpeedThreshold = 40f;

    public float bounceStrength = 0.8f;
    public float tumbleStrength = 80f;
    public float crashRecoveryTime = 3f;

    private Rigidbody rb;

    private float pitchInput;
    private float rollInput;
    private float yawInput;

    private bool recoveringFromCrash;
    private float recoveryTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = true;
        rb.linearDamping = 0.1f;
        rb.angularDamping = 2f;
        rb.maxAngularVelocity = 10f;
    }

    void Update()
    {
        ReadInput();
    }

    void FixedUpdate()
    {
        if (recoveringFromCrash)
        {
            recoveryTimer -= Time.fixedDeltaTime;

            if (recoveryTimer <= 0f)
            {
                recoveringFromCrash = false;
            }
        }

        Fly();
        Stabilize();
    }

    void ReadInput()
    {
        pitchInput = 0f;
        rollInput = 0f;
        yawInput = 0f;

        if (Keyboard.current != null)
        {
            // Pitch
            if (Keyboard.current.wKey.isPressed ||
                Keyboard.current.upArrowKey.isPressed)
            {
                pitchInput = 1f;
            }

            if (Keyboard.current.sKey.isPressed ||
                Keyboard.current.downArrowKey.isPressed)
            {
                pitchInput = -1f;
            }

            // Roll
            if (Keyboard.current.aKey.isPressed ||
                Keyboard.current.leftArrowKey.isPressed)
            {
                rollInput = -1f;
            }

            if (Keyboard.current.dKey.isPressed ||
                Keyboard.current.rightArrowKey.isPressed)
            {
                rollInput = 1f;
            }

            // Yaw
            if (Keyboard.current.qKey.isPressed)
                yawInput = -1f;

            if (Keyboard.current.eKey.isPressed)
                yawInput = 1f;

            // Throttle Up
            if (Keyboard.current.leftShiftKey.isPressed)
            {
                throttle += throttleChangeSpeed * Time.deltaTime;
            }

            // Throttle Down
            if (Keyboard.current.leftCtrlKey.isPressed)
            {
                throttle -= throttleChangeSpeed * Time.deltaTime;
            }
        }

        throttle = Mathf.Clamp01(throttle);

        if (Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.leftStick.ReadValue();

            if (stick.magnitude > 0.1f)
            {
                pitchInput = -stick.y;
                rollInput = stick.x;
            }
        }
    }

    void Fly()
    {
        if (!recoveringFromCrash)
        {
            float currentSpeed = maxSpeed * throttle;

            rb.linearVelocity =
                transform.forward * currentSpeed;
        }

        rb.AddTorque(
            transform.right *
            -pitchInput *
            pitchPower,
            ForceMode.Force
        );

        rb.AddTorque(
            transform.forward *
            -rollInput *
            rollPower,
            ForceMode.Force
        );

        rb.AddTorque(
            transform.up *
            yawInput *
            yawPower,
            ForceMode.Force
        );

        float bank = Vector3.Dot(transform.right, Vector3.up);
        float turnFromBank = -bank;

        rb.AddTorque(
            transform.up *
            turnFromBank *
            bankTurnPower,
            ForceMode.Force
        );
    }

    void Stabilize()
    {
        if (recoveringFromCrash)
            return;

        Vector3 localAngularVelocity =
            transform.InverseTransformDirection(
                rb.angularVelocity
            );

        rb.AddTorque(
            transform.right *
            -localAngularVelocity.x *
            pitchStability,
            ForceMode.Force
        );

        rb.AddTorque(
            transform.forward *
            -localAngularVelocity.z *
            rollStability,
            ForceMode.Force
        );

        rb.AddTorque(
            transform.up *
            -localAngularVelocity.y *
            yawStability,
            ForceMode.Force
        );
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.contactCount == 0)
            return;

        float impactSpeed =
            collision.relativeVelocity.magnitude;

        // Safe landing
        if (impactSpeed < landingSpeedThreshold)
            return;

        ContactPoint contact =
            collision.contacts[0];

        Vector3 bounceVelocity =
            Vector3.Reflect(
                rb.linearVelocity,
                contact.normal
            );

        // Medium impact
        if (impactSpeed < crashSpeedThreshold)
        {
            rb.linearVelocity =
                bounceVelocity * bounceStrength;

            recoveringFromCrash = true;
            recoveryTimer = 1.5f;

            return;
        }

        // Hard crash
        rb.linearVelocity =
            bounceVelocity * bounceStrength;

        rb.AddTorque(
            Random.onUnitSphere *
            tumbleStrength,
            ForceMode.Impulse
        );

        recoveringFromCrash = true;
        recoveryTimer = crashRecoveryTime;
    }
}