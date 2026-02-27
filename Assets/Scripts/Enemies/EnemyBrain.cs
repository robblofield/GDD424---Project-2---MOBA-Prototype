using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBrain : MonoBehaviour
{
    public enum EnemyState { Idle, Chasing, Attacking }

    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Ranges (used later)")]
    [SerializeField] private float aggroRange = 8f;
    [SerializeField] private float attackRange = 2.5f;

    public EnemyState State { get; private set; } = EnemyState.Idle;

    private NavMeshAgent agent;
    private GameManager gm;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            gm.RegisterEnemy(this);
        }
    }

    private void Update()
    {
        // Section 1: intentionally empty
    }
}