using System.Collections;
using System.Collections.Generic;


using UnityEngine;

public class CameraMovePoint : MonoBehaviour
{
    public Transform ship;

    private Vector3 localOffset;
    private Quaternion initialRotation;

    void Start()
    {
        localOffset = ship.InverseTransformPoint(transform.position);
        initialRotation = ship.rotation;
    }

    void LateUpdate()
    {
        // Изменение вращения корабля относительно начального положения
        Quaternion delta = Quaternion.Inverse(initialRotation) * ship.rotation;

        // Получаем углы относительного вращения
        Vector3 angles = delta.eulerAngles;

        // Убираем вращение вокруг локальной Z
        angles.z = 0f;

        // Создаём поворот без крена
        Quaternion rotation = initialRotation * Quaternion.Euler(angles);

        // Обновляем положение точки
        transform.position = ship.position + rotation * localOffset;
    }
}


