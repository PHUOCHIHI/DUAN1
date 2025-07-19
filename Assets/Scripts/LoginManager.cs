using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    public InputField usernameInput;
    public GameObject loginPanel;
    private string userFolder;
    private string scoreFile;

    void Start()
    {
        // Đường dẫn tuyệt đối bạn muốn lưu
        userFolder = @"C:\\Users\\Quang Phuoc\\Downloads\\Pacman\\Assets\\User";
        Debug.Log($"[LoginManager] Save path: {userFolder}");
        if (!Directory.Exists(userFolder))
            Directory.CreateDirectory(userFolder);
        scoreFile = Path.Combine(userFolder, "UserScore.txt");

        // TEST: Tạo file test khi vào game
        try
        {
            File.WriteAllText(scoreFile, "TestUser:999\n");
            Debug.Log("Đã tạo file test UserScore.txt thành công!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Lỗi khi tạo file test: " + ex.Message);
        }
    }

    public void OnLoginButton()
    {
        string username = usernameInput.text.Trim();
        if (!string.IsNullOrEmpty(username))
        {
            PlayerPrefs.SetString("username", username);
            loginPanel.SetActive(false);
            SceneManager.LoadScene("PacMan");
        }
    }

    // Gọi hàm này khi kết thúc game để lưu điểm
    public static void SaveScore(string username, int score)
    {
        string userFolder = @"C:\\Users\\Quang Phuoc\\Downloads\\Pacman\\Assets\\User";
        string scoreFile = Path.Combine(userFolder, "UserScore.txt");
        List<(string, int)> scores = new List<(string, int)>();
        if (File.Exists(scoreFile))
        {
            foreach (var line in File.ReadAllLines(scoreFile))
            {
                var parts = line.Split(':');
                if (parts.Length == 2 && int.TryParse(parts[1], out int s))
                    scores.Add((parts[0], s));
            }
        }
        // Thêm điểm mới
        scores.Add((username, score));
        // Sắp xếp điểm từ cao đến thấp
        scores.Sort((a, b) => b.Item2.CompareTo(a.Item2));
        // Ghi lại file
        using (StreamWriter sw = new StreamWriter(scoreFile, false))
        {
            foreach (var entry in scores)
                sw.WriteLine($"{entry.Item1}:{entry.Item2}");
        }
    }
}