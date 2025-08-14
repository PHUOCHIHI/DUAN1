using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private InputField usernameInput; // Sử dụng InputField Legacy
    private string userDataPath = @"C:\Users\Quang Phuoc\Downloads\Pacman\Assets\User\player_data.txt";

    private void Start()
    {
        // Chỉ ẩn panel đăng nhập, không ẩn panel setting
        if (loginPanel != null)
        {
            loginPanel.SetActive(false);
        }

        // Tạo thư mục nếu chưa tồn tại
        string directory = Path.GetDirectoryName(userDataPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    // Xử lý nút Play
    public void OnPlayButton()
    {
        if (loginPanel != null)
        {
            loginPanel.SetActive(true);
        }
    }

    // Xử lý nút Play trong panel đăng nhập
    public void OnLoginPlayButton()
    {
        if (usernameInput != null && !string.IsNullOrEmpty(usernameInput.text))
        {
            string username = usernameInput.text;
            SavePlayerData(username, 0, 0f); // Lưu tên người chơi khi bắt đầu
            loginPanel.SetActive(false);
            SceneManager.LoadScene("PacMan"); // Load scene PacMan
        }
        else
        {
            Debug.LogWarning("Vui lòng nhập tên người chơi!");
        }
    }

    // Xử lý nút Hủy trong panel đăng nhập
    public void OnCancelButton()
    {
        if (loginPanel != null)
        {
            loginPanel.SetActive(false);
            usernameInput.text = ""; // Xóa input
        }
    }

    // Xử lý nút Setting
    public void OnSettingButton()
    {
        if (settingPanel != null)
        {
            settingPanel.SetActive(true); // Hiển thị panel setting khi bấm nút
        }
    }

    // Xử lý nút thoát trong panel setting
    public void OnCloseSettingButton()
    {
        if (settingPanel != null)
        {
            settingPanel.SetActive(false); // Ẩn panel setting khi bấm nút đóng
        }
    }

    // Xử lý nút Exit
    public void OnExitButton()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // Lưu thông tin người chơi vào file
    private void SavePlayerData(string username, int score, float playTime)
    {
        string data = $"Username: {username}, Score: {score}, PlayTime: {playTime}\n";
        File.AppendAllText(userDataPath, data);
    }
}