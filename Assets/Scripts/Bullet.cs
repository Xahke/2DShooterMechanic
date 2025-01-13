// Bullet.cs
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 2f; // Mermi ömrü

    void Start()
    {
        Destroy(gameObject, lifetime); // Belirli süre sonra mermiyi yok et
    }

    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }
}