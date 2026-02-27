using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavMeshPlayerController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private string horizontalAxis = "Horizontal";
    [SerializeField] private string verticalAxis = "Vertical";

    [Header("Isometric Mapping")]
    [Tooltip("Common iso mapping is 45. If your world/camera is rotated differently, tweak this.")]
    [SerializeField] private float isoYawDegrees = 45f;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float acceleration = 40f;
    [SerializeField] private float angularSpeed = 720f;
    [SerializeField] private float stoppingDistance = 0.05f;

    [Header("Look")]
    [SerializeField] private bool faceMoveDirection = true;
    [SerializeField] private float rotateLerp = 12f;

    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.acceleration = acceleration;
        agent.angularSpeed = angularSpeed;
        agent.stoppingDistance = stoppingDistance;
        agent.autoBraking = false;
    }

    private void Update()
    {
        float x = Input.GetAxisRaw(horizontalAxis);
        float y = Input.GetAxisRaw(verticalAxis);

        Vector2 input = new Vector2(x, y);
        if (input.sqrMagnitude < 0.0001f)
        {
            agent.ResetPath();
            return;
        }

        input = Vector2.ClampMagnitude(input, 1f);

        // Input in XZ space (x = right, y = forward)
        Vector3 move = new Vector3(input.x, 0f, input.y);

        // Rotate input to match isometric world orientation (fixes “skewed” feel)
        move = Quaternion.Euler(0f, isoYawDegrees, 0f) * move;
        move = Vector3.ClampMagnitude(move, 1f);

        Vector3 desired = transform.position + move;

        if (NavMesh.SamplePosition(desired, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }

        if (faceMoveDirection)
        {
            Vector3 vel = agent.desiredVelocity;
            vel.y = 0f;

            if (vel.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(vel.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateLerp * Time.deltaTime);
            }
        }
    }
}