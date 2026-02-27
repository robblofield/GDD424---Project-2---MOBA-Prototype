using UnityEngine;
using UnityEngine.AI;

public class EnemyChaseAgent : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform playerTarget;

    [Header("Chase Settings")]
    [SerializeField] private float chaseRange = 50f;
    [SerializeField] private float repathRate = 0.25f;

    private NavMeshAgent agent;
    private float repathTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (playerTarget == null)
            return;

        if (agent == null)
            return;

        float dist = Vector3.Distance(transform.position, playerTarget.position);
        if (dist > chaseRange)
            return;

        repathTimer -= Time.deltaTime;

        if (repathTimer <= 0f)
        {
            repathTimer = repathRate;
            UpdateDestination();
        }
    }

    private void UpdateDestination()
    {
        agent.SetDestination(playerTarget.position);
    }
}