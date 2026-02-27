using UnityEngine;

// MOBA Prototype - Pt.1 - This is the full script reference for Section 1
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

    [Header("Movement (CharacterController)")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float arriveDistance = 0.2f;
    [SerializeField] private float turnSpeed = 12f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -20f;

    private CharacterController characterController;
    private float verticalVelocity;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        HandleClickInput();
        HandleMovement();
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

    private void HandleMovement()
    {
        // Check we have a CharacterController to work with
        if (characterController == null)
            return;

        // Calculate movement direction and speed along the ground
        Vector3 horizontalVelocity = Vector3.zero;

        // Check we have a target
        if (hasTarget)
        {
            Vector3 toTarget = targetPoint - transform.position;
            toTarget.y = 0f;

            // - Find the distance to target
            float distance = toTarget.magnitude;

            // - Check we’re not already at the target
            if (distance <= arriveDistance)
            {
                //   - If we have arrived then remove our target
                hasTarget = false;
            }
            else
            {
                //   - If not then keep on moving
                Vector3 direction = toTarget.normalized;
                horizontalVelocity = direction * moveSpeed;

                // Rotate towards dirction of movement
                if (direction.sqrMagnitude > 0.0001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        targetRot,
                        turnSpeed * Time.deltaTime
                    );
                }
            }
        }

        float yVel = HandleGravity();
        // Apply movement (horizontal + gravity)
        Vector3 finalVelocity = horizontalVelocity;
        finalVelocity.y = yVel;


        characterController.Move(finalVelocity * Time.deltaTime);
    }

    private float HandleGravity()
    {
        // Check we have a CharacterController to work with
        if (characterController == null)
            return 0f;
        // If grounded and falling, clamp to small negative value
        if (characterController.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;
        // Apply gravity over time
        verticalVelocity += gravity * Time.deltaTime;
        // Return vertical velocity
        return verticalVelocity;

    }


}