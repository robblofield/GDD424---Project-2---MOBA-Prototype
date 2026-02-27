using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBrain : MonoBehaviour
{
    public enum EnemyState { Idle, Chasing, Attacking }

    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Ranges")]
    [SerializeField] private float aggroRange = 8f;
    [SerializeField] private float attackRange = 1.2f;

    public EnemyState State { get; private set; } = EnemyState.Idle;

    private NavMeshAgent agent;
    private DamageZone damageZone;
    private GameManager gm;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        damageZone = GetComponentInChildren<DamageZone>();

        gm = FindFirstObjectByType<GameManager>();
        if (gm != null) gm.RegisterEnemy(this);
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    private void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        switch (State)
        {
            case EnemyState.Idle:
                damageZone?.SetActive(false);

                if (dist <= aggroRange)
                    State = EnemyState.Chasing;
                break;

            case EnemyState.Chasing:
                damageZone?.SetActive(false);

                agent.SetDestination(player.position);

                if (dist <= attackRange)
                    State = EnemyState.Attacking;
                else if (dist > aggroRange)
                    State = EnemyState.Idle;
                break;

            case EnemyState.Attacking:
                if (dist > attackRange)
                {
                    damageZone?.SetActive(false);
                    State = EnemyState.Chasing;
                    break;
                }

                agent.ResetPath();
                damageZone?.SetActive(true);
                break;
        }
        Debug.Log("State is: "+ State.ToString());
    }

    private void OnDestroy()
    {
        if (gm != null) gm.UnregisterEnemy(this);
    }
}