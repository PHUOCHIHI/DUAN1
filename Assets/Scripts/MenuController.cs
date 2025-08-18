using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System;

public class MenuController : MonoBehaviour
{
    public static MenuController Instance { get; private set; }
    [Header("UI Panels")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject settingPanel;
    
    [Header("Login Panel Elements")]
    [SerializeField] private InputField usernameInput;
    [SerializeField] private Button loginPlayButton;
    [SerializeField] private Button loginBackButton;
    [SerializeField] private Text errorMessageText; // Text hiển thị thông báo lỗi
    
    [Header("Setting Panel Elements")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Button settingBackButton;
    
    [Header("Main Menu Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button exitButton;
    
    [Header("Audio")]
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioSource sfxAudioSource;
    
    private string userDataPath = @"C:\Users\Quang Phuoc\Downloads\Pacman\Assets\User\player_score.txt";
    private string settingsPath = @"C:\Users\Quang Phuoc\Downloads\Pacman\Assets\User\settings.txt";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeUI();
        LoadSettings();
        CreateDirectories();
    }

    private void InitializeUI()
    {
        // Ẩn các panel khi bắt đầu
        if (loginPanel != null) loginPanel.SetActive(false);
        if (settingPanel != null) settingPanel.SetActive(false);
        
        // Xóa input và thông báo lỗi khi bắt đầu
        if (usernameInput != null) usernameInput.text = "";
        if (errorMessageText != null) errorMessageText.text = "";
        
        // Thiết lập các button listeners
        if (playButton != null) playButton.onClick.AddListener(OnPlayButton);
        if (settingButton != null) settingButton.onClick.AddListener(OnSettingButton);
        if (exitButton != null) exitButton.onClick.AddListener(OnExitButton);
        
        if (loginPlayButton != null) loginPlayButton.onClick.AddListener(OnLoginPlayButton);
        if (loginBackButton != null) loginBackButton.onClick.AddListener(OnLoginBackButton);
        
        if (settingBackButton != null) settingBackButton.onClick.AddListener(OnSettingBackButton);
        
        // Thiết lập slider listeners
        if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
    }

    private void CreateDirectories()
    {
        // Tạo thư mục User nếu chưa tồn tại
        string directory = Path.GetDirectoryName(userDataPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    // Xử lý nút Play chính
    public void OnPlayButton()
    {
        if (loginPanel != null)
        {
            loginPanel.SetActive(true);
            if (usernameInput != null)
            {
                usernameInput.text = "";
                usernameInput.Select();
            }
        }
    }

    // Xử lý nút Play trong panel đăng nhập
    public void OnLoginPlayButton()
    {
        if (usernameInput != null && !string.IsNullOrEmpty(usernameInput.text.Trim()))
        {
            string username = usernameInput.text.Trim();
            
            // Kiểm tra tên đã tồn tại chưa
            if (IsUsernameExists(username))
            {
                ShowErrorMessage($"Tên '{username}' đã tồn tại! Vui lòng chọn tên khác.");
                return;
            }
            
            // Xóa thông báo lỗi nếu có
            ClearErrorMessage();
            
            // Lưu tên người chơi vào PlayerPrefs để GameManager có thể sử dụng
            PlayerPrefs.SetString("CurrentPlayer", username);
            PlayerPrefs.Save();
            
            SavePlayerData(username, 0, 0f); // Lưu tên người chơi khi bắt đầu
            loginPanel.SetActive(false);
            SceneManager.LoadScene("PACMAN2"); // Load scene PacMan
        }
        else
        {
            ShowErrorMessage("Vui lòng nhập tên người chơi!");
        }
    }

    // Xử lý nút Back trong panel đăng nhập
    public void OnLoginBackButton()
    {
        if (loginPanel != null)
        {
            loginPanel.SetActive(false);
            if (usernameInput != null)
            {
                usernameInput.text = "";
            }
            ClearErrorMessage();
        }
    }

    // Xử lý nút Setting
    public void OnSettingButton()
    {
        if (settingPanel != null)
        {
            settingPanel.SetActive(true);
        }
    }

    // Xử lý nút Back trong panel setting
    public void OnSettingBackButton()
    {
        if (settingPanel != null)
        {
            settingPanel.SetActive(false);
            SaveSettings(); // Lưu cài đặt khi đóng panel
        }
    }

    // Xử lý nút Exit
    public void OnExitButton()
    {
        SaveSettings(); // Lưu cài đặt trước khi thoát
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // Method để GameManager gọi khi game kết thúc
    public void OnGameOver(int finalScore, float playTime)
    {
        SaveGameScore(finalScore, playTime);
    }

    // Method để hiển thị điểm số cao nhất
    public void DisplayHighScore(Text highScoreText)
    {
        if (highScoreText != null)
        {
            string currentPlayer = PlayerPrefs.GetString("CurrentPlayer", "Unknown");
            int highScore = GetHighScore(currentPlayer);
            highScoreText.text = $"High Score: {highScore}";
        }
    }

    // Method để áp dụng âm lượng cho toàn bộ game
    public void ApplyAudioSettings()
    {
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        // Áp dụng cho tất cả AudioSource trong scene
        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource audioSource in allAudioSources)
        {
            if (audioSource == musicAudioSource)
            {
                audioSource.volume = musicVolume;
            }
            else
            {
                audioSource.volume = sfxVolume;
            }
        }
        
        Debug.Log($"Đã áp dụng âm lượng - Music: {musicVolume:F2}, SFX: {sfxVolume:F2}");
    }

    // Method để hiển thị thông báo lỗi
    private void ShowErrorMessage(string message)
    {
        if (errorMessageText != null)
        {
            errorMessageText.text = message;
            errorMessageText.color = Color.red;
        }
        Debug.LogWarning(message);
    }

    // Method để xóa thông báo lỗi
    private void ClearErrorMessage()
    {
        if (errorMessageText != null)
        {
            errorMessageText.text = "";
        }
    }

    // Xử lý thay đổi âm lượng nhạc
    public void OnMusicVolumeChanged(float value)
    {
        if (musicAudioSource != null)
        {
            musicAudioSource.volume = value;
        }
        
        // Lưu cài đặt âm lượng nhạc vào PlayerPrefs
        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();
        
        Debug.Log($"Music Volume: {value:F2}");
    }

    // Xử lý thay đổi âm lượng hiệu ứng
    public void OnSFXVolumeChanged(float value)
    {
        if (sfxAudioSource != null)
        {
            sfxAudioSource.volume = value;
        }
        
        // Lưu cài đặt âm lượng hiệu ứng vào PlayerPrefs
        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();
        
        Debug.Log($"SFX Volume: {value:F2}");
    }

    // Lưu thông tin người chơi vào file
    private void SavePlayerData(string username, int score, float playTime)
    {
        try
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string data = $"Username: {username}, Score: {score}, PlayTime: {playTime:F2}s, Date: {timestamp}\n";
            File.AppendAllText(userDataPath, data);
            Debug.Log($"Đã lưu dữ liệu người chơi: {username}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Lỗi khi lưu dữ liệu người chơi: {e.Message}");
        }
    }

    // Lưu điểm số khi kết thúc game (có thể gọi từ GameManager)
    public void SaveGameScore(int score, float playTime)
    {
        try
        {
            string username = PlayerPrefs.GetString("CurrentPlayer", "Unknown");
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string data = $"Username: {username}, Score: {score}, PlayTime: {playTime:F2}s, Date: {timestamp}\n";
            File.AppendAllText(userDataPath, data);
            Debug.Log($"Đã lưu điểm số: {username} - {score} điểm - {playTime:F2}s");
        }
        catch (Exception e)
        {
            Debug.LogError($"Lỗi khi lưu điểm số: {e.Message}");
        }
    }

    // Kiểm tra tên người chơi đã tồn tại chưa
    public bool IsUsernameExists(string username)
    {
        try
        {
            if (!File.Exists(userDataPath))
                return false;

            string[] lines = File.ReadAllLines(userDataPath);
            
            foreach (string line in lines)
            {
                if (line.Contains($"Username: {username}"))
                {
                    return true; // Tên đã tồn tại
                }
            }

            return false; // Tên chưa tồn tại
        }
        catch (Exception e)
        {
            Debug.LogError($"Lỗi khi kiểm tra tên người chơi: {e.Message}");
            return false;
        }
    }

    // Lấy điểm số cao nhất của người chơi
    public int GetHighScore(string username)
    {
        try
        {
            if (!File.Exists(userDataPath))
                return 0;

            string[] lines = File.ReadAllLines(userDataPath);
            int highScore = 0;

            foreach (string line in lines)
            {
                if (line.Contains($"Username: {username}"))
                {
                    // Tìm điểm số trong dòng
                    int scoreIndex = line.IndexOf("Score: ");
                    if (scoreIndex != -1)
                    {
                        int commaIndex = line.IndexOf(",", scoreIndex);
                        if (commaIndex != -1)
                        {
                            string scoreStr = line.Substring(scoreIndex + 7, commaIndex - scoreIndex - 7);
                            if (int.TryParse(scoreStr, out int score))
                            {
                                if (score > highScore)
                                    highScore = score;
                            }
                        }
                    }
                }
            }

            return highScore;
        }
        catch (Exception e)
        {
            Debug.LogError($"Lỗi khi đọc điểm số: {e.Message}");
            return 0;
        }
    }

    // Lưu cài đặt
    private void SaveSettings()
    {
        try
        {
            float musicVolume = musicVolumeSlider != null ? musicVolumeSlider.value : 1f;
            float sfxVolume = sfxVolumeSlider != null ? sfxVolumeSlider.value : 1f;
            
            string settings = $"MusicVolume:{musicVolume}\nSFXVolume:{sfxVolume}";
            File.WriteAllText(settingsPath, settings);
            Debug.Log("Đã lưu cài đặt");
        }
        catch (Exception e)
        {
            Debug.LogError($"Lỗi khi lưu cài đặt: {e.Message}");
        }
    }

    // Tải cài đặt
    private void LoadSettings()
    {
        try
        {
            // Ưu tiên tải từ PlayerPrefs trước
            float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
            float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            
            // Áp dụng âm lượng cho sliders
            if (musicVolumeSlider != null) musicVolumeSlider.value = musicVolume;
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfxVolume;
            
            // Áp dụng âm lượng cho audio sources
            if (musicAudioSource != null) musicAudioSource.volume = musicVolume;
            if (sfxAudioSource != null) sfxAudioSource.volume = sfxVolume;
            
            Debug.Log($"Đã tải cài đặt - Music: {musicVolume:F2}, SFX: {sfxVolume:F2}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Lỗi khi tải cài đặt: {e.Message}");
        }
    }
}