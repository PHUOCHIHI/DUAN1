using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Ghost[] ghosts;
    [SerializeField] private Pacman pacman;
    [SerializeField] private Transform pellets;
    [SerializeField] private Text gameOverText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text livesText;
    [SerializeField] private GameObject winnerPanel;
    [SerializeField] private Text winnerScoreText;
    [SerializeField] private Text winnerTimeText;
    
    [Header("Pause Panel")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button pauseExitButton;
    [SerializeField] private Button helpButton;
    
    [Header("Game UI")]
    [SerializeField] private Button pauseButton; // Nút pause trên màn hình game
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip winnerSound;
    [SerializeField] private AudioClip gameOverSound;

    public int score { get; private set; } = 0;
    public int lives { get; private set; } = 3;
    public int currentLevel { get; private set; } = 1;

    private int ghostMultiplier = 1;
    private bool isPaused = false;

    private void Awake()
    {
        if (Instance != null) {
            DestroyImmediate(gameObject);
        } else {
            Instance = this;
        }
        
        // Đảm bảo Time.timeScale được reset khi scene mới load
        Time.timeScale = 1f;
        isPaused = false;
    }

    private void OnDestroy()
    {
        if (Instance == this) {
            Instance = null;
        }
    }

    private void Start()
    {
        // Đảm bảo game không bị pause khi bắt đầu scene mới
        Time.timeScale = 1f;
        isPaused = false;
        
        // Kiểm tra và reset trạng thái pause nếu cần
        CheckAndResetPauseState();
        
        // Lấy level hiện tại từ PlayerPrefs
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        
        // Kiểm tra scene và setup pause
        CheckSceneAndSetupPause();
        
        InitializePausePanel();
        NewGame();
        
        // Test pause functionality
        TestPauseFunctionality();
        
        // Áp dụng cài đặt âm thanh nếu có MenuController
        if (MenuController.Instance != null)
        {
            MenuController.Instance.ApplyAudioSettings();
        }
    }

    private void Update()
    {
        // Kiểm tra phím ESC để pause/unpause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        if (lives <= 0 && Input.anyKeyDown) {
            NewGame();
        }
    }

    private void NewGame()
    {
        SetScore(0);
        SetLives(3);
        NewRound();
    }

    private void NewRound()
    {
        gameOverText.enabled = false;
        if (winnerPanel != null) winnerPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        foreach (Transform pellet in pellets) {
            pellet.gameObject.SetActive(true);
        }

        ResetState();
    }

    private void ResetState()
    {
        for (int i = 0; i < ghosts.Length; i++) {
            ghosts[i].ResetState();
        }

        pacman.ResetState();
    }

    private void GameOver()
    {
        gameOverText.enabled = true;

        // Phát âm thanh game over
        if (audioSource != null && gameOverSound != null)
        {
            audioSource.PlayOneShot(gameOverSound);
        }

        for (int i = 0; i < ghosts.Length; i++) {
            ghosts[i].gameObject.SetActive(false);
        }

        pacman.gameObject.SetActive(false);
    }

    private void SetLives(int lives)
    {
        this.lives = lives;
        livesText.text = "x" + lives.ToString();
    }

    private void SetScore(int score)
    {
        this.score = score;
        scoreText.text = score.ToString().PadLeft(2, '0');
    }

    public void PacmanEaten()
    {
        pacman.DeathSequence();

        SetLives(lives - 1);

        if (lives > 0) {
            Invoke(nameof(ResetState), 3f);
        } else {
            GameOver();
        }
    }

    public void GhostEaten(Ghost ghost)
    {
        int points = ghost.points * ghostMultiplier;
        SetScore(score + points);

        ghostMultiplier++;
    }

    public void PelletEaten(Pellet pellet)
    {
        pellet.gameObject.SetActive(false);

        SetScore(score + pellet.points);

        if (!HasRemainingPellets())
        {
            ShowWinnerPanel();
        }
    }

    public void PowerPelletEaten(PowerPellet pellet)
    {
        for (int i = 0; i < ghosts.Length; i++) {
            ghosts[i].frightened.Enable(pellet.duration);
        }

        PelletEaten(pellet);
        CancelInvoke(nameof(ResetGhostMultiplier));
        Invoke(nameof(ResetGhostMultiplier), pellet.duration);
    }

    private bool HasRemainingPellets()
    {
        foreach (Transform pellet in pellets)
        {
            if (pellet.gameObject.activeSelf) {
                return true;
            }
        }

        return false;
    }

    private void ResetGhostMultiplier()
    {
        ghostMultiplier = 1;
    }

    private void ShowWinnerPanel()
    {
        // Ẩn Pacman và Ghosts
        pacman.gameObject.SetActive(false);
        for (int i = 0; i < ghosts.Length; i++) {
            ghosts[i].gameObject.SetActive(false);
        }

        // Phát âm thanh chiến thắng
        if (audioSource != null && winnerSound != null)
        {
            audioSource.PlayOneShot(winnerSound);
        }

        // Hiển thị panel winner
        if (winnerPanel != null)
        {
            winnerPanel.SetActive(true);
            
            // Hiển thị điểm số
            if (winnerScoreText != null)
            {
                winnerScoreText.text = $"{score}";
            }
            
            // Hiển thị thời gian chơi (tạm thời ẩn vì không có biến thời gian)
            if (winnerTimeText != null)
            {
                winnerTimeText.text = "";
            }
        }

        // Lưu điểm số vào file (tạm thời không có thời gian)
        if (MenuController.Instance != null)
        {
            MenuController.Instance.OnGameOver(score, 0f);
        }

        Debug.Log($"Winner! Score: {score}");
    }

    // Method để quay về menu chính từ panel winner
    public void ReturnToMainMenu()
    {
        // Đảm bảo game không bị pause khi chuyển scene
        Time.timeScale = 1f;
        isPaused = false;
        
        SceneManager.LoadScene("MainMenu");
    }

    // Method để chơi lại
    public void PlayAgain()
    {
        // Đảm bảo game không bị pause khi chuyển scene
        Time.timeScale = 1f;
        isPaused = false;
        
        // Reset về level 1
        PlayerPrefs.SetInt("CurrentLevel", 1);
        PlayerPrefs.Save();
        
        // Load lại scene hiện tại với level 1
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Method để chuyển level tiếp theo
    public void NextLevel()
    {
        // Đảm bảo game không bị pause khi chuyển scene
        Time.timeScale = 1f;
        isPaused = false;
        
        // Lưu level tiếp theo vào PlayerPrefs để scene tiếp theo có thể sử dụng
        PlayerPrefs.SetInt("CurrentLevel", currentLevel + 1);
        PlayerPrefs.Save();
        
        // Load scene tiếp theo
        // Có thể thay đổi tên scene tùy theo level
        string nextSceneName = GetNextSceneName(currentLevel + 1);
        SceneManager.LoadScene(nextSceneName);
    }

    // Method để lấy tên scene tiếp theo dựa trên level
    private string GetNextSceneName(int level)
    {
        // Có thể tùy chỉnh logic này theo cấu trúc scene của bạn
        switch (level)
        {
            case 2:
                return "PACMAN3"; // Scene cho level 2
            case 3:
                return "PACMAN4"; // Scene cho level 3
            case 4:
                return "PACMAN5"; // Scene cho level 4
            default:
                return "PACMAN2"; // Mặc định quay về scene đầu
        }
    }

    // Method để thoát game từ panel winner
    public void ExitGame()
    {
        // Đảm bảo game không bị pause khi thoát
        Time.timeScale = 1f;
        isPaused = false;
        
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // Method để xử lý khi bấm nút pause trên màn hình game
    public void OnPauseButtonClicked()
    {
        Debug.Log("Pause button clicked!");
        PauseGame();
    }
    
    // Method để test pause functionality
    public void TestPauseFunctionality()
    {
        Debug.Log("Testing pause functionality...");
        Debug.Log($"Current state - TimeScale: {Time.timeScale}, isPaused: {isPaused}");
        Debug.Log($"PauseButton: {(pauseButton != null ? "Found" : "Missing")}");
        Debug.Log($"PausePanel: {(pausePanel != null ? "Found" : "Missing")}");
        
        if (pauseButton != null)
        {
            Debug.Log("Pause button is assigned and should work");
        }
        else
        {
            Debug.LogError("Pause button is missing! Please check UI setup");
        }
    }
    
    // Method để force reset pause state
    public void ForceResetPauseState()
    {
        Time.timeScale = 1f;
        isPaused = false;
        
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        
        Debug.Log("Force reset pause state completed");
    }
    
    // Method để kiểm tra scene và setup pause
    public void CheckSceneAndSetupPause()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"Current scene: {currentSceneName}");
        
        // Force reset pause state
        ForceResetPauseState();
        
        // Tìm lại UI elements
        FindAndAssignUIElements();
        
        // Test pause functionality
        TestPauseFunctionality();
    }

    // Khởi tạo pause panel
    private void InitializePausePanel()
    {
        // Đảm bảo game không bị pause khi khởi tạo
        Time.timeScale = 1f;
        isPaused = false;
        
        // Tự động tìm các UI elements nếu chưa được assign
        FindAndAssignUIElements();
        
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // Xóa tất cả listeners cũ để tránh duplicate
        if (resumeButton != null) resumeButton.onClick.RemoveAllListeners();
        if (restartButton != null) restartButton.onClick.RemoveAllListeners();
        if (pauseExitButton != null) pauseExitButton.onClick.RemoveAllListeners();
        if (helpButton != null) helpButton.onClick.RemoveAllListeners();
        if (pauseButton != null) pauseButton.onClick.RemoveAllListeners();

        // Thiết lập button listeners cho pause panel
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
        if (pauseExitButton != null) pauseExitButton.onClick.AddListener(ExitGame);
        if (helpButton != null) helpButton.onClick.AddListener(ShowHelp);
        
        // Thiết lập button listener cho nút pause trên màn hình game
        if (pauseButton != null) pauseButton.onClick.AddListener(PauseGame);
        
        Debug.Log($"Pause panel initialized - PauseButton: {(pauseButton != null ? "Found" : "Missing")}, PausePanel: {(pausePanel != null ? "Found" : "Missing")}");
    }
    
    // Tự động tìm và assign UI elements
    private void FindAndAssignUIElements()
    {
        // Tìm pause panel nếu chưa được assign
        if (pausePanel == null)
        {
            pausePanel = GameObject.Find("PausePanel");
            if (pausePanel == null)
            {
                pausePanel = GameObject.Find("Pause Panel");
            }
        }
        
        // Tìm pause button nếu chưa được assign
        if (pauseButton == null)
        {
            pauseButton = GameObject.Find("PauseButton")?.GetComponent<Button>();
            if (pauseButton == null)
            {
                pauseButton = GameObject.Find("Pause Button")?.GetComponent<Button>();
            }
        }
        
        // Tìm các button trong pause panel nếu chưa được assign
        if (resumeButton == null && pausePanel != null)
        {
            resumeButton = pausePanel.transform.Find("ResumeButton")?.GetComponent<Button>();
            if (resumeButton == null)
            {
                resumeButton = pausePanel.transform.Find("Resume Button")?.GetComponent<Button>();
            }
        }
        
        if (restartButton == null && pausePanel != null)
        {
            restartButton = pausePanel.transform.Find("RestartButton")?.GetComponent<Button>();
            if (restartButton == null)
            {
                restartButton = pausePanel.transform.Find("Restart Button")?.GetComponent<Button>();
            }
        }
        
        if (pauseExitButton == null && pausePanel != null)
        {
            pauseExitButton = pausePanel.transform.Find("ExitButton")?.GetComponent<Button>();
            if (pauseExitButton == null)
            {
                pauseExitButton = pausePanel.transform.Find("Exit Button")?.GetComponent<Button>();
            }
        }
        
        if (helpButton == null && pausePanel != null)
        {
            helpButton = pausePanel.transform.Find("HelpButton")?.GetComponent<Button>();
            if (helpButton == null)
            {
                helpButton = pausePanel.transform.Find("Help Button")?.GetComponent<Button>();
            }
        }
        
        Debug.Log($"UI Elements found - PauseButton: {(pauseButton != null ? "Yes" : "No")}, PausePanel: {(pausePanel != null ? "Yes" : "No")}");
    }

    // Toggle pause/unpause
    private void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    // Pause game
    public void PauseGame()
    {
        if (isPaused) return;

        isPaused = true;
        Time.timeScale = 0f; // Dừng thời gian game

        // Hiển thị pause panel
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        Debug.Log("Game Paused - TimeScale: " + Time.timeScale);
    }

    // Resume game
    public void ResumeGame()
    {
        if (!isPaused) return;

        isPaused = false;
        Time.timeScale = 1f; // Khôi phục thời gian game

        // Ẩn pause panel
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Debug.Log("Game Resumed - TimeScale: " + Time.timeScale);
    }

    // Restart game
    public void RestartGame()
    {
        // Khôi phục thời gian game
        Time.timeScale = 1f;
        isPaused = false;

        // Ẩn pause panel
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // Bắt đầu game mới
        NewGame();
        
        Debug.Log("Game Restarted - TimeScale: " + Time.timeScale);
    }

    // Show help
    public void ShowHelp()
    {
        // Có thể hiển thị panel help hoặc load scene help
        Debug.Log("Help button clicked");
        // SceneManager.LoadScene("HelpScene");
    }
    
    // Method để kiểm tra và reset trạng thái pause nếu cần
    public void CheckAndResetPauseState()
    {
        if (Time.timeScale == 0f && !isPaused)
        {
            Debug.LogWarning("TimeScale is 0 but game is not paused. Resetting...");
            Time.timeScale = 1f;
        }
        
        Debug.Log($"Current pause state - TimeScale: {Time.timeScale}, isPaused: {isPaused}");
    }
}
