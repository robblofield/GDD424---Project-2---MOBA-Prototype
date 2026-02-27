using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerBrain : MonoBehaviour
{
    public enum PlayerState { Idle, Moving, Attacking }

    [Header("References")]
    [SerializeField] private Camera mainCamera;

    [Header("Move Settings")]
    [SerializeField] private float rayDistance = 500f;
    [SerializeField] private float navMeshSampleRadius = 2f;
    [SerializeField] private bool debugLogs = false;

    public PlayerState State { get; private set; } = PlayerState.Idle;

    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (mainCamera == null) mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryMoveToMouseClick();
        }

        // Update state logic while moving
        if (State == PlayerState.Moving && !agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                State = PlayerState.Idle;
                if (debugLogs) Debug.Log("[PlayerBrain] Reached destination -> Idle", this);
            }
        }
    }

    private void TryMoveToMouseClick()
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("[PlayerBrain] No camera assigned and Camera.main not found.", this);
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
        {
            // Snap click point to nearest NavMesh position
            if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, navMeshSampleRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(navHit.position);
                State = PlayerState.Moving;

                if (debugLogs) Debug.Log($"[PlayerBrain] Moving to {navHit.position}", this);
            }
            else
            {
                if (debugLogs) Debug.Log("[PlayerBrain] Click was not near NavMesh (SamplePosition failed).", this);
            }
        }
    }
}