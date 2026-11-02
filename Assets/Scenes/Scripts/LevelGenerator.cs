using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private GameManager gameManager;

    [Header("Sector settings")]
    [SerializeField] private float sectorLength = 40f;
    [SerializeField] private float sectorWidth = 16f;
    [SerializeField] private float sectorHeight = 12f;

    [Header("Generation")]
    [SerializeField] private int maxSectors = 20;

    private Sector previousSector;
    private Sector currentSector;

    private readonly List<Sector> candidates =
        new List<Sector>();

    private readonly List<Sector> allSectors =
        new List<Sector>();

    private int selectedExit = -1;
    private int generatedCount;

    public Sector CurrentSector => currentSector;
    public int GeneratedCount => generatedCount;

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("LevelGenerator: Player is not assigned.");
            enabled = false;
            return;
        }

        currentSector = CreateSector(
            Vector2.zero, 0f);

        generatedCount = 1;

        CreateCandidates();
    }

    private void Update()
    {
        if (currentSector == null || gameManager == null)
            return;

        if (player.position.z >= currentSector.EndZ)
        {
            AdvanceSector();
        }
    }

    
    public void SelectExit(int index)
    {
        if (index < 0 || index >= candidates.Count)
            return;

        selectedExit = index;

        Vector2 target = candidates[index].CenterXY;

        Transport transport = player.GetComponent<Transport>();

        //if (transport != null)
        //{
        //    transport.SetNavigationTarget(target);
        //}
    }
    
    private void AdvanceSector()
    {
        int choice = selectedExit;

        if (choice < 0 || choice >= candidates.Count)
        {
            choice = FindNearestCandidate();

            if (gameManager != null)
            {
                gameManager.ReportNoise();
            }
        }

        Sector nextSector = candidates[choice];

        candidates.RemoveAt(choice);

        if (previousSector != null)
        {
            allSectors.Remove(previousSector);
            Destroy(previousSector.gameObject);
        }

        previousSector = currentSector;
        currentSector = nextSector;

        // Удаляем невыбранные ветки.
        foreach (Sector candidate in candidates)
        {
            allSectors.Remove(candidate);
            Destroy(candidate.gameObject);
        }

        candidates.Clear();

        // Выравниваем игрока по выбранному выходу.
        Vector3 playerPosition = player.position;

        player.position = new Vector3(
            currentSector.CenterXY.x,
            currentSector.CenterXY.y,
            playerPosition.z);

        Transport transport = player.GetComponent<Transport>();

        //if (transport != null)
        //{
            //transport.ClearNavigationTarget();
        //}

        selectedExit = -1;
        generatedCount++;

        if (generatedCount >= maxSectors)
        {
            gameManager?.Win();
            return;
        }

        CreateCandidates();
    }

    private int FindNearestCandidate()
    {
        int nearest = 0;
        float minDistance = float.MaxValue;

        Vector2 playerPosition = new Vector2(
            player.position.x, player.position.y);

        for (int i = 0; i < candidates.Count; i++)
        {
            float distance = Vector2.Distance(
                playerPosition, candidates[i].CenterXY);

            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = i;
            }
        }

        return nearest;
    }

    private void CreateCandidates()
    {
        for (int i = 0; i < 4; i++)
        {
            Vector2 center = currentSector.CenterXY
                + currentSector.GetExitOffset(i);

            Sector candidate = CreateSector(
                center, currentSector.EndZ);

            candidates.Add(candidate);
        }
    }

    private Sector CreateSector(Vector2 center, float startZ)
    {
        GameObject sectorObject = new GameObject(
            "Sector_" + generatedCount + "_" + allSectors.Count);

        sectorObject.transform.SetParent(transform);

        Sector sector = sectorObject.AddComponent<Sector>();

        sector.Initialize(
            center,
            startZ,
            sectorLength,
            sectorWidth,
            sectorHeight);

        allSectors.Add(sector);

        return sector;
    }
}