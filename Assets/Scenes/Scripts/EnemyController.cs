using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Transport))]
public class EnemyController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private GameManager gameManager;

    [Header("Pursuit")]
    [SerializeField] private float stealthSpeed = 8f;
    [SerializeField] private float chaseSpeed = 22f;
    [SerializeField] private float catchDistance = 3f;
    [SerializeField] private float verticalFollowGain = 2f;

    private Transport transport;

    private void Awake()
    {
        transport = GetComponent<Transport>();
    }

    private void Start()
    {
        if (player != null)
        {
            // Начинаем погоню позади игрока.
            Vector3 position = transform.position;
            position.z = player.position.z - 25f;
            transform.position = position;
        }

        transport.SetSpeed(stealthSpeed);
    }

    private void Update()
    {
        if (player == null ||
            gameManager == null ||
            gameManager.IsGameOver)
        {
            return;
        }

        if (gameManager.CurrentPhase == GamePhase.Stealth)
        {
            transport.SetSpeed(stealthSpeed);
            //transport.ClearNavigationTarget();
        }
        else if (gameManager.CurrentPhase == GamePhase.Chase)
        {
            transport.SetSpeed(chaseSpeed);

            Vector2 target = new Vector2(
                player.position.x,
                player.position.y);

            //transport.SetNavigationTarget(target);

            float distance = Vector3.Distance(
                transform.position, player.position);

            if (distance <= catchDistance)
            {
                gameManager.Lose();
            }
        }
    }
}