using UnityEngine;

public class IsoFollowCamera : MonoBehaviour
{
    [SerializeField] private Transform target;

    [Header("Isometric Offset")]
    [Tooltip("Typical iso-ish: (x=+10, y=+10, z=-10)")]
    [SerializeField] private Vector3 offset = new Vector3(10f, 10f, -10f);

    [Header("Smoothing")]
    [SerializeField] private float positionSmoothTime = 0.12f;
    [SerializeField] private float rotationLerp = 10f;

    [Header("Look At")]
    [SerializeField] private Vector3 lookAtOffset = new Vector3(0f, 1f, 0f);

    private Vector3 velocity;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref velocity, positionSmoothTime);

        Vector3 lookPoint = target.position + lookAtOffset;
        Quaternion desiredRot = Quaternion.LookRotation(lookPoint - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, rotationLerp * Time.deltaTime);
    }
}