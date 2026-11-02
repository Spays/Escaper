using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Targets")]
    public Transform lookPoint;
    public Transform movePoint;

    [Header("Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 5f;
    
    [Header("Roll")]
    public float rollAmount = 1f;
    public float rollDelay = 2f;

    private Quaternion initialLookRotation;
    private float currentRoll;
    
    void Start()
    {
        initialLookRotation = lookPoint.rotation;
    }
    void LateUpdate()
    {
        if (lookPoint == null || movePoint == null)
            return;

        // Плавное перемещение камеры
        transform.position = Vector3.Lerp(
            transform.position,
            movePoint.position,
            moveSpeed * Time.deltaTime
        );

        // Направление взгляда на первую точку
        Vector3 direction = lookPoint.position - transform.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Плавный поворот камеры
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
        /*
        // Получаем угол крена корабля
        float targetRoll = lookPoint.eulerAngles.z;

// Переводим угол в диапазон -180...180
        if (targetRoll > 180f)
            targetRoll -= 360f;

// Настраиваем силу крена
        targetRoll *= rollAmount;

// Плавное следование камеры
        currentRoll = Mathf.LerpAngle(
            currentRoll,
            targetRoll,
            rollDelay * Time.deltaTime
        );

// Применяем крен камеры
        transform.rotation *= Quaternion.Euler(0f, 0f, currentRoll);
        */
    }
}
