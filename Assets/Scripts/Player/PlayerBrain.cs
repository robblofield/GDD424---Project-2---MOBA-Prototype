using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerBrain : MonoBehaviour
{
    public enum PlayerState { Idle, Moving, Attacking }

    [Header("References")]
    [SerializeField] private Camera mainCamera;
    private NavMeshAgent agent;
    private Transform currentTarget;
    private DamageZone damageZone;
    private Health health;

    [Header("Move Settings")]
    [SerializeField] private float rayDistance = 500f;
    [SerializeField] private float navMeshSampleRadius = 2f;

    [Header("Combat")]
    [SerializeField] private float attackRange = 2.5f;

    public PlayerState State { get; private set; } = PlayerState.Idle;

   

    private void Awake()
    {
        // Find references on awake
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();
        damageZone = GetComponentInChildren<DamageZone>();
        if (mainCamera == null) mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryMoveToMouseClick();
        }

        if (State == PlayerState.Moving && !agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                State = PlayerState.Idle;
            }
        }

        if (State == PlayerState.Attacking)
        {
            if (currentTarget == null)
            {
                State = PlayerState.Idle;
                damageZone?.SetActive(false);
                return;
            }

            float distance = Vector3.Distance(transform.position, currentTarget.position);

            if (distance > attackRange)
            {
                agent.SetDestination(currentTarget.position);
                damageZone?.SetActive(false);
            }
            else
            {
                agent.ResetPath();
                damageZone?.SetActive(true);
            }
        }
        Debug.Log("State is: " + State.ToString());
    }

    private void TryMoveToMouseClick()
    {
        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
        {
            Health targetHealth = hit.collider.GetComponent<Health>();

            if (targetHealth != null && hit.collider.gameObject != gameObject)
            {
                currentTarget = hit.collider.transform;
                State = PlayerState.Attacking;

                // Immediately begin chasing target
                agent.SetDestination(currentTarget.position);

                return;
            }

            currentTarget = null;
            damageZone?.SetActive(false);

            if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, navMeshSampleRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(navHit.position);
                State = PlayerState.Moving;
            }
        }
    }

    public void NotifyDamagedBy(Transform attacker)
    {
        if (attacker == null) return;

        // If the player has explicitly clicked to move away, do not override.
        if (State == PlayerState.Moving) return;

        // If we already have a target, keep it (optional: you can replace this rule).
        if (currentTarget != null) return;

        currentTarget = attacker;
        State = PlayerState.Attacking;

        agent.SetDestination(currentTarget.position);
    }
}