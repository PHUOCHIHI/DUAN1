using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    public float speed = 2f;                // tốc độ chạy
    private Transform Pacman;              // vị trí player

    void Start()
    {
        // Tìm đối tượng có tag "Player"
        Pacman = GameObject.FindGameObjectWithTag("Pacman").transform;

    }

    void Update()
    {
        if (Pacman != null)
        {
            // Di chuyển enemy về phía player
            Vector2 direction = (Pacman.position - transform.position).normalized;
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
        }
    }
}
