using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraPointRotaitor : MonoBehaviour
{
    public float rotationSpeed = 5f;
    public float maxAngle = 30f;

    // Значение джойстика от -1 до 1
    private float joystickYInput;
    private float joystickXInput;

    private float currentXAngle;
    private float currentYAngle;

    void Update()
    {
        if (Gamepad.current != null)
        {
            // Левый стик: наклон носа вверх/вниз
            Vector2 leftStick = Gamepad.current.leftStick.ReadValue();
            Vector2 rightStick = Gamepad.current.rightStick.ReadValue();
            joystickYInput = leftStick.y;
            joystickXInput = rightStick.x;
        }

        float targetYAngle = joystickYInput * maxAngle;
        float targetXAngle = joystickXInput * maxAngle;

        currentXAngle = Mathf.Lerp(
            currentXAngle,
            targetXAngle,
            rotationSpeed * Time.deltaTime
        );
        currentYAngle = Mathf.Lerp(
            currentYAngle,
            targetYAngle,
            rotationSpeed * Time.deltaTime
        );

        transform.localRotation = Quaternion.Euler(
            currentYAngle,
            currentXAngle,
            0f
        );
    }
}
