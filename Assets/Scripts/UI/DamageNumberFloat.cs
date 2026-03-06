using UnityEngine;

public class DamageNumberFloat : MonoBehaviour
{
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float lifetime = 1f;

    private void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
        lifetime -= Time.deltaTime;

        if (lifetime <= 0f)
            Destroy(gameObject);
    }
}