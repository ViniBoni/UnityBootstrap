using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;

public class ManagerMenu : MonoBehaviour
{


    public GameObject mainMenu, levelSelect, options;

    public Slider master, music, sfx, sensitivity;
    public AudioMixer mix;

    void Start()
    {
        if(PlayerPrefs.HasKey("VSync")) QualitySettings.vSyncCount = PlayerPrefs.GetInt("VSync");
        Application.targetFrameRate = Screen.currentResolution.refreshRate * 3;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        

        //If player has no name, put them on name screen
        if(!PlayerPrefs.HasKey("Name")) SceneManager.LoadScene(SceneManager.sceneCountInBuildSettings - 1);

        mix.SetFloat("MasterVolume", Mathf.Log10(PlayerPrefs.GetFloat("MasterVolume")) * 20);
        mix.SetFloat("MusicVolume", Mathf.Log10(PlayerPrefs.GetFloat("MusicVolume")) * 20);
        mix.SetFloat("SFXVolume", Mathf.Log10(PlayerPrefs.GetFloat("SFXVolume")) * 20);

        master.value = PlayerPrefs.GetFloat("MasterVolume");
        music.value = PlayerPrefs.GetFloat("MusicVolume");
        sfx.value = PlayerPrefs.GetFloat("SFXVolume");
        sensitivity.value = PlayerPrefs.GetFloat("Sensitivity");
    }

    public void SetMaster(float sliderValue)
    {
        mix.SetFloat("MasterVolume", Mathf.Log10(sliderValue) * 20);
        PlayerPrefs.SetFloat("MasterVolume", sliderValue);
    }    
    
    public void SetMusic(float sliderValue)
    {
        mix.SetFloat("MusicVolume", Mathf.Log10(sliderValue) * 20);
        PlayerPrefs.SetFloat("MusicVolume", sliderValue);
    }

    public void SetSFX(float sliderValue)
    {
        mix.SetFloat("SFXVolume", Mathf.Log10(sliderValue) * 20);
        PlayerPrefs.SetFloat("SFXVolume", sliderValue); 
    }

    public void SetSensitivity(float sliderValue)
    {
        PlayerPrefs.SetFloat("Sensitivity", sliderValue);
    }
    public void ExitGame()
    {
        Application.Quit();
    }

    public void GoToLevelSelect()
    {
        mainMenu.SetActive(false);
        levelSelect.SetActive(true);
    }

    public void GoToMainMenu()
    {
        mainMenu.SetActive(true);
        levelSelect.SetActive(false);
        options.SetActive(false);
    }

    public void GoToOptions()
    {
        mainMenu.SetActive(false);
        options.SetActive(true);
    }

    public void LoadScene(int id)
    {
        SceneManager.LoadScene(id);
    }

    public void LoadNameScene()
    {
        SceneManager.LoadScene(SceneManager.sceneCountInBuildSettings - 1);
    }
    
    public void OpenURL(string url)
    {
        Application.OpenURL(url);
    }

    public void SelectButton(Button button)
    {
        button.Select();
    }
}
