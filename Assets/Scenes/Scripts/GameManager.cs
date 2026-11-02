using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GamePhase
{
    Stealth,
    Chase,
    Victory,
    Defeat
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Stealth")]
    [SerializeField] private float stealthDuration = 90f;

    public GamePhase CurrentPhase { get; private set; }
        = GamePhase.Stealth;

    public float StealthRemaining { get; private set; }

    public bool IsGameOver =>
        CurrentPhase == GamePhase.Victory ||
        CurrentPhase == GamePhase.Defeat;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        StealthRemaining = stealthDuration;
    }

    private void Update()
    {
        if (CurrentPhase != GamePhase.Stealth)
            return;

        StealthRemaining -= Time.deltaTime;

        if (StealthRemaining <= 0f)
        {
            BeginChase();
        }
    }

    public void ReportNoise()
    {
        if (CurrentPhase == GamePhase.Stealth)
        {
            BeginChase();
        }
    }

    public void BeginChase()
    {
        if (CurrentPhase != GamePhase.Stealth)
            return;

        CurrentPhase = GamePhase.Chase;

        Debug.Log("CHASE STARTED!");
    }

    public void Win()
    {
        if (IsGameOver)
            return;

        CurrentPhase = GamePhase.Victory;
        PlayerController player =
            FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            Transport transport =
                player.GetComponent<Transport>();

            if (transport != null)
                transport.DisableMovement();
        }
        Debug.Log("VICTORY!");
    }

    public void Lose()
    {
        if (CurrentPhase == GamePhase.Defeat)
            return;

        CurrentPhase = GamePhase.Defeat;

        PlayerController player =
            FindFirstObjectByType<PlayerController>();

        if (player != null)
        {
            Transport transport =
                player.GetComponent<Transport>();

            if (transport != null)
                transport.DisableMovement();
        }

        Debug.Log("Player defeated!");
    }
}