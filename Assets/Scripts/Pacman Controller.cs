using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class PacmanController : MonoBehaviour
{
    public float speed = 5f; // Tốc độ di chuyển
    private Vector2 direction = Vector2.zero; // Hướng di chuyển
    private Vector2 nextDirection = Vector2.zero;
    private Rigidbody2D rb; // Tham chiếu Rigidbody2D
    private int score = 0; // Điểm số
    public Text scoreText; // Tham chiếu Text UI
    private int lives = 3; // Số mạng ban đầu
    public Text livesText; // Tham chiếu Text UI cho số mạng
    private bool isPoweredUp = false; // Trạng thái power-up
    private float powerUpTime = 5f; // Thời gian power-up
    private float powerUpTimer; // Bộ đếm thời gian power-up
    public LayerMask wallLayer; // Layer tường để kiểm tra va chạm
    private Animator animator; // Tham chiếu Animator
    private bool isDead = false; // Trạng thái chết

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        UpdateScoreText();
        UpdateLivesText();
    }

    void Update()
    {
        if (isDead) return;

        // Đổi hướng NGAY LẬP TỨC khi bấm phím, nếu không bị tường cản
        if (Input.GetKeyDown(KeyCode.UpArrow) && !IsBlocked(Vector2.up))
            direction = Vector2.up;
        else if (Input.GetKeyDown(KeyCode.DownArrow) && !IsBlocked(Vector2.down))
            direction = Vector2.down;
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && !IsBlocked(Vector2.left))
            direction = Vector2.left;
        else if (Input.GetKeyDown(KeyCode.RightArrow) && !IsBlocked(Vector2.right))
            direction = Vector2.right;

        // Xoay Pacman theo hướng di chuyển
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            if (animator != null) animator.SetTrigger("Run");
        }

        // Quản lý thời gian power-up
        if (isPoweredUp)
        {
            powerUpTimer -= Time.deltaTime;
            if (powerUpTimer <= 0)
            {
                isPoweredUp = false;
            }
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;
        if (!IsBlocked(direction) && direction != Vector2.zero)
        {
            rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
        }
        else
        {
            // Nếu bị cản, dừng lại hoàn toàn
            direction = Vector2.zero;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;
        if (other.CompareTag("Dot"))
        {
            score += 10; // Tăng 10 điểm khi ăn dot
            Destroy(other.gameObject);
            UpdateScoreText();
        }
        else if (other.CompareTag("PowerDot"))
        {
            score += 50; // Tăng 50 điểm khi ăn power dot
            Destroy(other.gameObject);
            isPoweredUp = true;
            powerUpTimer = powerUpTime; // Kích hoạt power-up 5 giây
            UpdateScoreText();
        }
        else if (other.CompareTag("Ghost"))
        {
            if (isPoweredUp)
            {
                // Ăn ma khi power-up
                Destroy(other.gameObject);
                score += 200; // Thưởng 200 điểm khi ăn ma
                UpdateScoreText();
            }
            else
            {
                // Chết: thực hiện animation die, dừng di chuyển
                if (animator != null) animator.SetTrigger("Die");
                isDead = true;
                direction = Vector2.zero;
                rb.linearVelocity = Vector2.zero;
                lives--;
                UpdateLivesText();
                Invoke(nameof(HandleDeath), 1.2f); // Đợi animation die xong (1.2s tuỳ chỉnh)
            }
        }
    }

    void HandleDeath()
    {
        if (lives <= 0)
        {
            // Game over khi hết mạng
            Debug.Log("Game Over!");
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
        else
        {
            // Reset vị trí Pac-Man và trạng thái
            transform.position = new Vector2(0, 0); // Vị trí bắt đầu, chỉnh sửa theo cần
            isDead = false;
            if (animator != null) animator.SetTrigger("Run");
        }
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    void UpdateLivesText()
    {
        if (livesText != null)
        {
            livesText.text = "Lives: " + lives;
        }
    }

    bool IsBlocked(Vector2 dir)
    {
        // Raycast kiểm tra phía trước có tường không
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, Vector2.one * 0.75f, 0f, dir, 0.6f, wallLayer);
        return hit.collider != null;
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreText();
    }
}

public class PacmanDotTilemap : MonoBehaviour
{
    public Tilemap dotTilemap;
    public int scorePerDot = 10;
    public PacmanController pacmanController; // Tham chiếu script PacmanController

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Pacman phải có tag "Player"
        {
            Vector3Int cellPos = dotTilemap.WorldToCell(other.transform.position);
            if (dotTilemap.HasTile(cellPos))
            {
                dotTilemap.SetTile(cellPos, null); // Xóa Dot
                pacmanController.AddScore(scorePerDot); // Hàm cộng điểm
            }
        }
    }
}