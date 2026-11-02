using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Sector : MonoBehaviour
{
    [Header("Geometry")]
    [SerializeField] private float length = 40f;
    [SerializeField] private float width = 16f;
    [SerializeField] private float height = 12f;
    [SerializeField] private float wallThickness = 1f;

    [Header("Branching")]
    [SerializeField] private float horizontalOffset = 5f;
    [SerializeField] private float verticalOffset = 3f;

    private Transform geometryRoot;

    public float StartZ { get; private set; }
    public float EndZ => StartZ + length;
    public Vector2 CenterXY { get; private set; }
    public float Length => length;

    public void Initialize(
        Vector2 centerXY,
        float startZ,
        float sectorLength,
        float sectorWidth,
        float sectorHeight)
    {
        CenterXY = centerXY;
        StartZ = startZ;
        length = sectorLength;
        width = sectorWidth;
        height = sectorHeight;

        BuildGeometry();
    }

    public Vector2 GetExitOffset(int index)
    {
        switch (index)
        {
            case 0:
                return new Vector2(-horizontalOffset, 0f);

            case 1:
                return new Vector2(horizontalOffset, 0f);

            case 2:
                return new Vector2(0f, verticalOffset);

            case 3:
                return new Vector2(0f, -verticalOffset);

            default:
                return Vector2.zero;
        }
    }

    public Vector2 GetExitPosition(int index)
    {
        return CenterXY + GetExitOffset(index);
    }

    private void BuildGeometry()
    {
        if (geometryRoot != null)
        {
            Destroy(geometryRoot.gameObject);
        }

        GameObject root = new GameObject("Geometry");
        root.transform.SetParent(transform, false);
        geometryRoot = root.transform;

        float centerZ = StartZ + length * 0.5f;
        float centerX = CenterXY.x;
        float centerY = CenterXY.y;

        CreateCube(
            "Floor",
            new Vector3(centerX, centerY - height * 0.5f, centerZ),
            new Vector3(width, wallThickness, length));

        CreateCube(
            "Ceiling",
            new Vector3(centerX, centerY + height * 0.5f, centerZ),
            new Vector3(width, wallThickness, length));

        CreateCube(
            "LeftWall",
            new Vector3(centerX - width * 0.5f, centerY, centerZ),
            new Vector3(wallThickness, height, length));

        CreateCube(
            "RightWall",
            new Vector3(centerX + width * 0.5f, centerY, centerZ),
            new Vector3(wallThickness, height, length));

        for (int i = 0; i < 4; i++)
        {
            Vector2 exit = GetExitPosition(i);

            GameObject marker = GameObject.CreatePrimitive(
                PrimitiveType.Cube);

            marker.name = "Exit_" + (i + 1);
            marker.transform.SetParent(geometryRoot, true);

            marker.transform.position = new Vector3(
                exit.x,
                exit.y,
                EndZ - 0.5f);

            marker.transform.localScale = new Vector3(1f, 1f, 1f);

            Collider markerCollider =
                marker.GetComponent<Collider>();

            if (markerCollider != null)
            {
                Destroy(markerCollider);
            }
        }
    }

    private void CreateCube(
        string objectName,
        Vector3 position,
        Vector3 scale)
    {
        GameObject cube = GameObject.CreatePrimitive(
            PrimitiveType.Cube);

        cube.name = objectName;
        cube.transform.SetParent(geometryRoot, true);
        cube.transform.position = position;
        cube.transform.localScale = scale;
    }
}