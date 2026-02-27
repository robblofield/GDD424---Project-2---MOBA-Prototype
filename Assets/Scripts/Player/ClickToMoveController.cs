using UnityEngine;
using UnityEngine.AI;

// GDD424 - Project 2  - Moba Pt.1 Section 2 - Full code reference
public class ClickToMoveController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;

    [Header("Targeting")]
    [SerializeField] private bool debugDrawTarget = true;

    private Vector3 targetPoint;
    private bool hasTarget;

    [Header("Raycast")]
    [SerializeField] private float rayDistance = 500f;

    [Header("NavMesh Movement")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private bool useNavMeshAgent = true;
    [SerializeField] private float navMeshSampleRadius = 2f;

    private CharacterController characterController;
    private float verticalVelocity;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

    }

    private void Update()
    {
        HandleClickInput();
    }

    private void HandleClickInput()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, rayDistance))
        {
            Debug.Log($"Hit: {hitInfo.collider.name} | Tag: {hitInfo.collider.tag}");
            StoreHitPoint(hitInfo);
            ApplyNavMeshDestination();
            OnDrawGizmos();
        }
        else
        {
            Debug.Log("Hit: Nothing");
        }
    }

    private void StoreHitPoint(RaycastHit hitInfo)
    {
        targetPoint = hitInfo.point;
        hasTarget = true;

        Debug.Log($"Stored targetPoint: {targetPoint}");
        
    }

    private void OnDrawGizmos()
    {
        if (!debugDrawTarget) return;
        if (!hasTarget) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(targetPoint, 0.2f);
    }

    private void ApplyNavMeshDestination()
    {
        if (agent == null)
            return;

        if (!hasTarget)
            return;

        if (NavMesh.SamplePosition(targetPoint, out NavMeshHit navHit, navMeshSampleRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(navHit.position);
        }
        else
        {
            Debug.Log("No NavMesh nearby - pick a different point.");
        }
    }


}