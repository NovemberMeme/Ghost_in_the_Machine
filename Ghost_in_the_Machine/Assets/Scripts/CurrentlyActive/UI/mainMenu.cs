﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenu : MonoBehaviour {

    private int width;
    private int height;

	public void PlayGame()
    {
        SceneManager.LoadScene("Ghost_in_the_Machine");
    }

    public void QuitGame()
    {
        Debug.Log("Quit!");
        Application.Quit();
    }

    public void SetFullscreen (bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetWidth(int newWidth){
        width = newWidth;
    }

    public void SetHeight(int newHeight){
        height = newHeight;
    }

    public void SetRes(){
        Screen.SetResolution(width, height, false);
    }
}
