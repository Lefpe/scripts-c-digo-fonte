using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class homePage : MonoBehaviour
{
    public Button buttonOnPlay;
    public Button buttonQuitApp;

    void Start()
    {
        buttonQuitApp.onClick.AddListener(() => OnQuitApp());
        buttonOnPlay.onClick.AddListener(() => OnPlay());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPlay()
    {
        SceneManager.LoadScene("PlayPage");
    }

    public void OnQuitApp()
    {
        Application.Quit();
    }
}
