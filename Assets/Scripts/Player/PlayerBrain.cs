using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerBrain : MonoBehaviour
{
    public enum PlayerState { Idle, Moving, Attacking } // The states the character can be in
    [SerializeField] private bool debugMode = false;

    [Header("References")]
    [SerializeField] private Camera mainCamera; // The ScreenPointToRayCamera
    private NavMeshAgent agent; // The Navmesh Agent attached to the character
    private DamageZone damageZone; // Our "hit/hurt" box system

    [Header("Move Settings")]
    [SerializeField] private float rayDistance = 500f; // How far our ScreenPointToRay probes into the scene
    [SerializeField] private float navMeshSampleRadius = 2f; // Margin of error to find nearby navmesh

    [Header("Combat")]
    [SerializeField] private float attackRange = 1.8f; // How far we can attack from
    [SerializeField] private float attackExitBuffer = 0.25f; // A small extra distance before we stop attacking.
    [SerializeField] private float attackStopBuffer = 0.1f; // A small offset so the Agent stops inside attackRange.
    [SerializeField] private LayerMask enemyLayer; // Enemy layer that identifies our targets
    private Transform currentTarget; // The location of a found enemy

    [Header("Auto Attack")]
    private float scanRadius = 2.5f; // How far around the player we search for enemies to automatically target.
    private float scanInterval = 0.2f; // How often (in seconds) we check for nearby enemies
    private float scanTimer = 0f; // A countdown timer that controls when the next enemy scan happens.


    private bool isFleeing = false; // Flee means: player clicked ground to move away, so ignore auto-attack

    public PlayerState State { get; private set; } = PlayerState.Idle; // Getter and Setter for states

    private void Awake()
    {
        // Hook up references on awake
        agent = GetComponent<NavMeshAgent>();
        damageZone = GetComponentInChildren<DamageZone>();
        if (mainCamera == null) mainCamera = Camera.main;

        // Set scan radius from attack range
        scanRadius = attackRange;
    }

    private void Update()
    {
        // Check for mouse clicks
        if (Input.GetMouseButtonDown(0))
            HandleClick();

        // Auto-acquire targets if not fleeing
        if (!isFleeing)
            AutoAcquireTarget();

        // Switching logic
        switch (State)
        {
            case PlayerState.Idle:
                damageZone?.SetActive(false);
                agent.stoppingDistance = 0f;

                // if enemy is in range, start attacking
                if (!isFleeing && currentTarget == null)
                {
                    Transform t = FindNearestEnemyInRange();
                    if (t != null)
                    {
                        SetTargetAndAttack(t);
                    }
                }
                break;

            case PlayerState.Moving:
                damageZone?.SetActive(false);
                agent.stoppingDistance = 0f;

                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    State = PlayerState.Idle;
                    // When we finish moving, fleeing ends (auto-attack can resume)
                    isFleeing = false;
                }
                break;

            case PlayerState.Attacking:
                UpdateAttacking();
                break;

        }
        // Debug to read out the current state if debug mode enabled
        if (debugMode) { Debug.Log("Player State is: " + State.ToString()); }
    }

    private void HandleClick()
    {
        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, rayDistance))
            return;

        // If click hits an enemy (any child collider), attack it
        Health targetHealth = hit.collider.GetComponentInParent<Health>();
        if (targetHealth != null && targetHealth.gameObject != gameObject)
        {
            isFleeing = false;
            SetTargetAndAttack(targetHealth.transform);
            return;
        }

        // Otherwise, click is a move command.
        // if clicked spot is outside attack range, we are fleeing (ignore auto attack)
        Vector3 clickedPoint = hit.point;

        float distFromPlayer = Vector3.Distance(transform.position, clickedPoint);
        isFleeing = (distFromPlayer > attackRange);

        currentTarget = null;
        damageZone?.SetActive(false);

        if (NavMesh.SamplePosition(clickedPoint, out NavMeshHit navHit, navMeshSampleRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(navHit.position);
            State = PlayerState.Moving;
        }
    }

    private void UpdateAttacking()
    {
        // Clean dead/removed target
        if (currentTarget == null)
        {
            damageZone?.SetActive(false);

            // Stay attacking while ANY enemies in range exist
            Transform newTarget = (!isFleeing) ? FindNearestEnemyInRange() : null;
            if (newTarget != null)
            {
                SetTargetAndAttack(newTarget);
                return;
            }

            State = PlayerState.Idle;
            return;
        }

        float dist = Vector3.Distance(transform.position, currentTarget.position);

        agent.stoppingDistance = Mathf.Max(0f, attackRange - attackStopBuffer);

        // If target has moved clearly out of range, chase again (unless fleeing)
        if (dist > attackRange + attackExitBuffer)
        {
            damageZone?.SetActive(false);

            // If there are still enemies in range, switch target; otherwise chase current
            if (!isFleeing)
            {
                Transform inRange = FindNearestEnemyInRange();
                if (inRange != null)
                {
                    SetTargetAndAttack(inRange);
                    return;
                }
            }

            agent.SetDestination(currentTarget.position);
            return;
        }

        // Close enough to attack
        damageZone?.SetActive(true);

        // If we're "in range" but not actually overlapping the hit zone, step closer.
        if (damageZone != null && !damageZone.HasTargets)
        {
            agent.SetDestination(currentTarget.position);
        }
        else
        {
            agent.ResetPath();
        }

        // If target dies, we should keep attacking if other enemies are in range.
        // We'll handle that next frame via currentTarget == null check (or via Health destroying Transform).
    }

    private void AutoAcquireTarget() //Find nearest target within range
    {
        scanTimer -= Time.deltaTime;
        if (scanTimer > 0f) return;
        scanTimer = scanInterval;

        if (State == PlayerState.Moving) return;
        if (currentTarget != null) return;

        Transform t = FindNearestEnemyInRange();
        if (t != null)
        {
            SetTargetAndAttack(t);
        }
    }

    private Transform FindNearestEnemyInRange() // Return the best next in-range target based on their distance
    {
        float radius = (scanRadius > 0f) ? scanRadius : attackRange;

        Collider[] hits = Physics.OverlapSphere(transform.position, radius, enemyLayer);
        if (hits == null || hits.Length == 0) return null;

        Transform best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            Health h = hits[i].GetComponentInParent<Health>();
            if (h == null) continue;

            float d = Vector3.Distance(transform.position, h.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = h.transform;
            }
        }

        return best;
    }

    private void SetTargetAndAttack(Transform target) // Attack the target
    {
        if (target == null) return;

        currentTarget = target;
        State = PlayerState.Attacking;

        agent.SetDestination(currentTarget.position);
    }

    // If an enemy hits us, we attack back (unless fleeing)
    public void NotifyDamagedBy(Transform attacker)
    {
        if (attacker == null) return;
        if (isFleeing) return;

        // If we already have a target, keep it
        if (currentTarget != null) return;

        SetTargetAndAttack(attacker);
    }
}