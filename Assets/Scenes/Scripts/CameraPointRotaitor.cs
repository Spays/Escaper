using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraPointRotaitor : MonoBehaviour
{
    public float rotationSpeed = 5f;
    public float maxAngle = 30f;

    // Значение джойстика от -1 до 1
    public float joystickInput;

    private float currentAngle;

    void Update()
    {
        if (Gamepad.current != null)
        {
            // Левый стик: наклон носа вверх/вниз
            Vector2 leftStick = Gamepad.current.leftStick.ReadValue();
            joystickInput = leftStick.y;
        }

        float targetAngle = joystickInput * maxAngle;

        currentAngle = Mathf.Lerp(
            currentAngle,
            targetAngle,
            rotationSpeed * Time.deltaTime
        );

        transform.localRotation = Quaternion.Euler(
            currentAngle,
            0f,
            0f
        );
    }
}
