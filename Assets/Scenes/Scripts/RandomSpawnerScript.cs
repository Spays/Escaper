using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpawnerScript : MonoBehaviour

{
    [Header("Object")]
    [SerializeField] private GameObject prefab;

    [Header("Spawn Settings")]
    [SerializeField] private int objectCount = 500;

    [Header("Volume")]
    [SerializeField] private Vector3 volumeSize =
        new Vector3(50f, 20f, 50f);

    [Header("Large Fraction")]
    [Range(0f, 100f)]
    [SerializeField] private float largeFraction = 30f;

    [SerializeField] private float largeMinScale = 2f;
    [SerializeField] private float largeMaxScale = 4f;

    [Header("Small Fraction")]
    [SerializeField] private float smallMinScale = 0.3f;
    [SerializeField] private float smallMaxScale = 1f;

    private Mesh mesh;
    private Material material;

    private readonly List<Matrix4x4[]> batches = new();

    private const int MaxInstancesPerBatch = 1023;

    private void Start()
    {
        GenerateObjects();
    }

    private void GenerateObjects()
    {
        if (prefab == null)
            return;

        MeshFilter meshFilter = prefab.GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = prefab.GetComponent<MeshRenderer>();

        if (meshFilter == null || meshRenderer == null)
        {
            Debug.LogError("Prefab must have MeshFilter and MeshRenderer!");
            return;
        }

        mesh = meshFilter.sharedMesh;
        material = meshRenderer.sharedMaterial;
        material.enableInstancing = true;

        List<Matrix4x4> matrices = new List<Matrix4x4>();

        for (int i = 0; i < objectCount; i++)
        {
            Vector3 position = transform.TransformPoint(
                new Vector3(
                    Random.Range(-volumeSize.x / 2f, volumeSize.x / 2f),
                    Random.Range(-volumeSize.y / 2f, volumeSize.y / 2f),
                    Random.Range(-volumeSize.z / 2f, volumeSize.z / 2f)
                )
            );

            Quaternion rotation = Random.rotation;

            // Выбираем фракцию
            bool isLarge = Random.Range(0f, 100f) < largeFraction;

            // Выбираем масштаб в соответствующем диапазоне
            float scale = isLarge
                ? Random.Range(largeMinScale, largeMaxScale)
                : Random.Range(smallMinScale, smallMaxScale);

            Matrix4x4 matrix = Matrix4x4.TRS(
                position,
                rotation,
                Vector3.one * scale
            );

            matrices.Add(matrix);
        }

        for (int i = 0; i < matrices.Count; i += MaxInstancesPerBatch)
        {
            int count = Mathf.Min(
                MaxInstancesPerBatch,
                matrices.Count - i
            );

            Matrix4x4[] batch = new Matrix4x4[count];
            matrices.CopyTo(i, batch, 0, count);

            batches.Add(batch);
        }
    }

    private void Update()
    {
        if (mesh == null || material == null)
            return;

        foreach (Matrix4x4[] batch in batches)
        {
            Graphics.DrawMeshInstanced(
                mesh,
                0,
                material,
                batch
            );
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, volumeSize);
    }
}


