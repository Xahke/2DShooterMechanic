using UnityEngine;  // Eksik olan using direktifi

public class Enemy : MonoBehaviour
{
    [SerializeField] private float knockbackForce = 10f;
    private Rigidbody2D rb;
    private bool isTargeted = false;

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
                rb.AddForce(bullet.direction * knockbackForce, ForceMode2D.Impulse);
                
                if (isTargeted)
                {
                    GameEvents.TriggerCriticalHit();
                }
                else
                {
                    GameEvents.TriggerEnemyHit();
                }
            }
            Destroy(other.gameObject);
        }
    }

    private void OnMouseEnter()
    {
        isTargeted = true;
    }

    private void OnMouseExit()
    {
        isTargeted = false;
    }
}