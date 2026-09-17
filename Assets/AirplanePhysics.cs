using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class SimpleFlightControls : MonoBehaviour
{
    [Header("Speed")]
    public float maxSpeed = 120f;

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

    private Rigidbody rb;

    private float pitchInput;
    private float rollInput;
    private float yawInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = true;

        // Keep physics stable.
        rb.linearDamping = 0.1f;
        rb.angularDamping = 2f;
        rb.maxAngularVelocity = 5f;
    }

    void Update()
    {
        ReadInput();
    }

    void FixedUpdate()
    {
        Fly();
        Stabilize();
    }

    void ReadInput()
    {
        pitchInput = 0f;
        rollInput = 0f;
        yawInput = 0f;

        // Keyboard
        if (Keyboard.current != null)
        {
            // W = nose down
            if (Keyboard.current.wKey.isPressed ||
                Keyboard.current.upArrowKey.isPressed)
            {
                pitchInput = 1f;
            }

            // S = nose up
            if (Keyboard.current.sKey.isPressed ||
                Keyboard.current.downArrowKey.isPressed)
            {
                pitchInput = -1f;
            }

            // A = roll left
            if (Keyboard.current.aKey.isPressed ||
                Keyboard.current.leftArrowKey.isPressed)
            {
                rollInput = -1f;
            }

            // D = roll right
            if (Keyboard.current.dKey.isPressed ||
                Keyboard.current.rightArrowKey.isPressed)
            {
                rollInput = 1f;
            }

            // Q/E = yaw
            if (Keyboard.current.qKey.isPressed)
            {
                yawInput = -1f;
            }

            if (Keyboard.current.eKey.isPressed)
            {
                yawInput = 1f;
            }
        }

        // Controller
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
        // Keep the plane moving slowly forward.
        Vector3 forwardVelocity =
            transform.forward * maxSpeed;

        rb.linearVelocity = forwardVelocity;

        // Pitch
        rb.AddTorque(
            transform.right *
            -pitchInput *
            pitchPower,
            ForceMode.Force
        );

        // Roll
        rb.AddTorque(
            transform.forward *
            -rollInput *
            rollPower,
            ForceMode.Force
        );

        // Yaw
        rb.AddTorque(
            transform.up *
            yawInput *
            yawPower,
            ForceMode.Force
        );

        // Coordinated turn: banking should also turn the plane.
        // bank = 1 when wings level, ~0 when banked 90 degrees.
        float bank = Vector3.Dot(transform.right, Vector3.up);
        float turnFromBank = -bank; // flip sign if it turns the wrong way

        rb.AddTorque(
            transform.up *
            turnFromBank *
            bankTurnPower,
            ForceMode.Force
        );
    }

    void Stabilize()
    {
        // Convert angular velocity into local space.
        Vector3 localAngularVelocity =
            transform.InverseTransformDirection(
                rb.angularVelocity
            );

        // Stop excessive pitch rotation.
        rb.AddTorque(
            transform.right *
            -localAngularVelocity.x *
            pitchStability,
            ForceMode.Force
        );

        // Stop excessive rolling.
        rb.AddTorque(
            transform.forward *
            -localAngularVelocity.z *
            rollStability,
            ForceMode.Force
        );

        // Stop excessive yawing.
        rb.AddTorque(
            transform.up *
            -localAngularVelocity.y *
            yawStability,
            ForceMode.Force
        );
    }
}