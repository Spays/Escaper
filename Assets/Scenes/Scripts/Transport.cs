using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Transport : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField] private float minSpeed = 8f;
    [SerializeField] private float maxSpeed = 40f;
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private float braking = 18f;

    [Header("Flight")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxPitch = 80f;
    [SerializeField] private float rollSpeed = 100f;
    [SerializeField] private float rollReturnSpeed = 120f;

    private Rigidbody body;

    private Vector2 pendingMouseInput;
    private float rollInput;
    private float throttleInput;
    private float currentRoll;

    private float yaw;
    private float pitch;
    
    public bool CanMove { get; private set; } = true;

    public float CurrentSpeed { get; private set; }

    public float MinSpeed => minSpeed;
    public float MaxSpeed => maxSpeed;

    public Vector3 Velocity => body != null
        ? body.velocity
        : Vector3.zero;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();

        body.useGravity = false;
        body.constraints = RigidbodyConstraints.None;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;

        Vector3 angles = transform.eulerAngles;

        yaw = angles.y;
        pitch = NormalizeAngle(angles.x);

        CurrentSpeed = minSpeed;
    }

    public void SetMouseInput(Vector2 delta)
    {
        pendingMouseInput += delta;
    }

    public void SetRollInput(float input)
    {
        rollInput = Mathf.Clamp(input, -1f, 1f);
    }

    public void SetThrottleInput(float input)
    {
        throttleInput = Mathf.Clamp(input, -1f, 1f);
    }

    public void SetSpeed(float speed)
    {
        CurrentSpeed = Mathf.Clamp(speed, minSpeed, maxSpeed);
    }

    private void FixedUpdate()
    {
        if (!CanMove)
            return;
        
        float dt = Time.fixedDeltaTime;

        // Mouse controls yaw and pitch.
        yaw += pendingMouseInput.x * mouseSensitivity;
        pitch -= pendingMouseInput.y * mouseSensitivity;

        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);
        pendingMouseInput = Vector2.zero;

        // A/D roll the craft.
        if (Mathf.Abs(rollInput) > 0.01f)
        {
            currentRoll -= rollInput * rollSpeed * dt;
        }
        else
        {
            currentRoll = Mathf.MoveTowards(
                currentRoll,
                0f,
                rollReturnSpeed * dt);
        }

        Quaternion targetRotation =
            Quaternion.Euler(pitch, yaw, currentRoll);

        body.MoveRotation(targetRotation);

        // W/S control acceleration and braking.
        if (throttleInput > 0f)
        {
            CurrentSpeed = Mathf.MoveTowards(
                CurrentSpeed,
                maxSpeed,
                acceleration * throttleInput * dt);
        }
        else if (throttleInput < 0f)
        {
            CurrentSpeed = Mathf.MoveTowards(
                CurrentSpeed,
                minSpeed,
                braking * -throttleInput * dt);
        }

        // Fly forward along the craft's nose.
        body.velocity = body.rotation * Vector3.forward * CurrentSpeed;
    }

    private static float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;

        return angle;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (CompareTag("Player"))
        {
            GameManager.Instance?.ReportNoise();
        }
    }
    public void DisableMovement()
    {
        CanMove = false;

        if (body != null)
        {
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.isKinematic = true;
        }
    }
}
