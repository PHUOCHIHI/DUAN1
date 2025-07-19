using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject loginPanel; // Kéo panel đăng nhập vào đây trong Inspector

    public void PlayGame()
    {
        // Hiện panel đăng nhập thay vì vào game ngay
        if (loginPanel != null)
        {
            loginPanel.SetActive(true);
        }
    }
    public void Exxit()
    {
        Application.Quit();
        Debug.Log("exitt");
    }
}
