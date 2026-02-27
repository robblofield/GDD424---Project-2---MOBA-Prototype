using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBrain : MonoBehaviour
{
    public enum EnemyState { Idle, Moving, Attacking } // The states the character can be in
    [SerializeField] private bool debugMode = false;

    [Header("References")]
    private NavMeshAgent agent; // The Navmesh Agent attached to the character
    private DamageZone damageZone; // Our "hit/hurt" box system

    [Header("Combat")]
    [SerializeField] private float aggroRange = 4f; // How far the enemy will detect/chase the player
    [SerializeField] private float attackRange = 1.2f; // How far we can attack from
    [SerializeField] private float attackExitBuffer = 0.25f; // A small extra distance before we stop attacking.
    [SerializeField] private float attackStopBuffer = 0.1f; // A small offset so the Agent stops inside attackRange.
    [SerializeField] private LayerMask playerLayer; // Player layer that identifies our target
    private Transform currentTarget; // The location of a found player

    [Header("Auto Target")]
    private float scanRadius = 4f; // How far around the enemy we search for the player to automatically target.
    private float scanInterval = 0.2f; // How often (in seconds) we check for nearby targets
    private float scanTimer = 0f; // A countdown timer that controls when the next scan happens.

    public EnemyState State { get; private set; } = EnemyState.Idle; // Getter and Setter for states

    private GameManager gm;

    private void Awake()
    {
        // Hook up references on awake
        agent = GetComponent<NavMeshAgent>();
        damageZone = GetComponentInChildren<DamageZone>();

        gm = FindFirstObjectByType<GameManager>();
        if (gm != null) gm.RegisterEnemy(this);

        // Set scan radius from aggro range (enemy scans to detect, not to attack)
        scanRadius = aggroRange;
    }

    private void Update()
    {
        // Auto-acquire targets
        AutoAcquireTarget();

        // Switching logic
        switch (State)
        {
            case EnemyState.Idle:
                damageZone?.SetActive(false);
                agent.stoppingDistance = 0f;

                // if player is in range, start attacking/chasing
                if (currentTarget == null)
                {
                    Transform t = FindNearestEnemyInRange();
                    if (t != null)
                    {
                        SetTargetAndAttack(t);
                    }
                }
                break;

            case EnemyState.Moving:
                damageZone?.SetActive(false);
                agent.stoppingDistance = 0f;

                // If target disappeared/died, go idle
                if (currentTarget == null)
                {
                    agent.ResetPath();
                    State = EnemyState.Idle;
                    break;
                }

                // Give up if player has moved out of aggro range
                if (Vector3.Distance(transform.position, currentTarget.position) > aggroRange)
                {
                    currentTarget = null;
                    agent.ResetPath();
                    State = EnemyState.Idle;
                    break;
                }

                // Chase target
                agent.SetDestination(currentTarget.position);

                // When close enough, go to attacking
                if (Vector3.Distance(transform.position, currentTarget.position) <= attackRange)
                {
                    State = EnemyState.Attacking;
                }
                break;

            case EnemyState.Attacking:
                UpdateAttacking();
                break;
        }

        // Debug to read out the current state if debug mode enabled
        if (debugMode) { Debug.Log("Enemy State is: " + State.ToString()); }
    }

    private void UpdateAttacking()
    {
        // Clean dead/removed target
        if (currentTarget == null)
        {
            damageZone?.SetActive(false);
            State = EnemyState.Idle;
            return;
        }

        float dist = Vector3.Distance(transform.position, currentTarget.position);

        // If target leaves aggro, give up completely
        if (dist > aggroRange)
        {
            damageZone?.SetActive(false);
            currentTarget = null;
            agent.ResetPath();
            State = EnemyState.Idle;
            return;
        }

        agent.stoppingDistance = Mathf.Max(0f, attackRange - attackStopBuffer);

        // If target has moved clearly out of attack range, chase again
        if (dist > attackRange + attackExitBuffer)
        {
            damageZone?.SetActive(false);
            agent.SetDestination(currentTarget.position);
            State = EnemyState.Moving;
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
    }

    private void AutoAcquireTarget() // Find nearest target within range
    {
        scanTimer -= Time.deltaTime;
        if (scanTimer > 0f) return;
        scanTimer = scanInterval;

        if (State == EnemyState.Moving) return;
        if (currentTarget != null) return;

        Transform t = FindNearestEnemyInRange();
        if (t != null)
        {
            SetTargetAndAttack(t);
        }
    }

    private Transform FindNearestEnemyInRange() // Return the best next in-range target based on their distance
    {
        float radius = (scanRadius > 0f) ? scanRadius : aggroRange;

        Collider[] hits = Physics.OverlapSphere(transform.position, radius, playerLayer);
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
        State = EnemyState.Attacking;

        agent.SetDestination(currentTarget.position);
    }

    private void OnDestroy()
    {
        if (gm != null) gm.UnregisterEnemy(this);
    }
}