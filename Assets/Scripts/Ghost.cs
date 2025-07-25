using UnityEngine;

public class Ghost : MonoBehaviour
{
    public enum GhostState { Chase, Scatter, Frightened }
    public GhostState currentState;

    public float speed = 3f;
    private Transform pacmanTarget;
    public Vector2 scatterTarget; // góc map riêng của ghost

    private Vector2 direction;

    void Start()
    {
        pacmanTarget = GameObject.FindGameObjectWithTag("Player").transform;
        ChangeState(GhostState.Scatter);
        InvokeRepeating(nameof(SwitchScatterChase), 7f, 10f); // 7s scatter, 10s chase
    }

    void Update()
    {
        MoveByState();
    }

    void MoveByState()
    {
        switch (currentState)
        {
            case GhostState.Chase:
                MoveTowards(pacmanTarget.position);
                break;
            case GhostState.Scatter:
                MoveTowards(scatterTarget);
                break;
            case GhostState.Frightened:
                MoveRandom(); // di chuyển ngẫu nhiên
                break;
        }
    }

    void MoveTowards(Vector2 target)
    {
        Vector2 dir = (target - (Vector2)transform.position).normalized;
        transform.Translate(dir * speed * Time.deltaTime);
    }

    void MoveRandom()
    {
        if (Random.value < 0.01f)
        {
            Vector2[] dirs = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
            direction = dirs[Random.Range(0, dirs.Length)];
        }
        transform.Translate(direction * speed * Time.deltaTime);
    }

    void SwitchScatterChase()
    {
        if (currentState == GhostState.Scatter)
            ChangeState(GhostState.Chase);
        else if (currentState == GhostState.Chase)
            ChangeState(GhostState.Scatter);
    }

    public void ChangeState(GhostState newState)
    {
        currentState = newState;

        if (newState == GhostState.Frightened)
        {
            GetComponent<SpriteRenderer>().color = Color.blue;
            CancelInvoke(nameof(SwitchScatterChase));
            Invoke(nameof(EndFrightened), 5f); // 5s sợ hãi
        }
        else
        {
            GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    void EndFrightened()
    {
        ChangeState(GhostState.Scatter);
        InvokeRepeating(nameof(SwitchScatterChase), 7f, 10f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (currentState == GhostState.Frightened && other.CompareTag("Player"))
        {
            Destroy(gameObject); // bị ăn
        }
        else if (other.CompareTag("Player"))
        {
            Debug.Log("Pac-Man bị bắt! Game Over!");
        }
    }
}
