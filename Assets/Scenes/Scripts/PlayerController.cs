using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Transport))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelGenerator levelGenerator;
    [SerializeField] private GameManager gameManager;

    [Header("Stealth")]
    [SerializeField] private float noiseSpeedThreshold = 28f;

    private Transport transport;

    private void Awake()
    {
        transport = GetComponent<Transport>();
    }

    private void Start()
    {
        // Capture the mouse for flight controls.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (GameManager.Instance == null)
            return;
        
        if (gameManager == null || gameManager.IsGameOver)
        {
            transport.SetThrottleInput(0f);
            transport.SetRollInput(0f);
            return;
        }
        
        if (GameManager.Instance.CurrentPhase == GamePhase.Defeat ||
            GameManager.Instance.CurrentPhase == GamePhase.Victory)
        {
            return;
        }
        

        HandleFlightInput();
        HandleRouteSelection();
        CheckNoise();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Cursor.lockState != CursorLockMode.Locked &&
            Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void HandleFlightInput()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Vector2 mouseDelta = new Vector2(
                Input.GetAxis("Mouse X"),
                Input.GetAxis("Mouse Y"));

            transport.SetMouseInput(mouseDelta);
        }

        float throttle = 0f;

        if (Input.GetKey(KeyCode.W))
            throttle += 1f;

        if (Input.GetKey(KeyCode.S))
            throttle -= 1f;

        transport.SetThrottleInput(throttle);

        float roll = 0f;

        if (Input.GetKey(KeyCode.A))
            roll += 1f;

        if (Input.GetKey(KeyCode.D))
            roll -= 1f;

        transport.SetRollInput(roll);
    }

    private void HandleRouteSelection()
    {
        if (levelGenerator == null)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            levelGenerator.SelectExit(0);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            levelGenerator.SelectExit(1);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            levelGenerator.SelectExit(2);

        if (Input.GetKeyDown(KeyCode.Alpha4))
            levelGenerator.SelectExit(3);
    }

    private void CheckNoise()
    {
        if (gameManager.CurrentPhase != GamePhase.Stealth)
            return;

        if (transport.CurrentSpeed > noiseSpeedThreshold)
        {
            gameManager.ReportNoise();
        }
    }

    private void OnDisable()
    {
        if (transport != null)
        {
            transport.SetThrottleInput(0f);
            transport.SetRollInput(0f);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}