using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MainMenuControl : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {

    }
    
    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }
}
