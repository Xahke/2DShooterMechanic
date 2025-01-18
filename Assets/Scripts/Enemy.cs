using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float knockbackForce = 10f;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            if (bullet != null)
            {
                // Merminin hareket yönünü kullan
                rb.AddForce(bullet.direction * knockbackForce, ForceMode2D.Impulse);
            }
            Destroy(other.gameObject);
        }
    }
}