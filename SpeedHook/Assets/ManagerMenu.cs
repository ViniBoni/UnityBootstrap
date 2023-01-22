using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ManagerMenu : MonoBehaviour
{


    public GameObject mainMenu, levelSelect;


    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;


        //If player has no name, put them on name screen
        if(!PlayerPrefs.HasKey("Name")) SceneManager.LoadScene(SceneManager.sceneCountInBuildSettings - 1);
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
    }

    public void LoadScene(int id)
    {
        SceneManager.LoadScene(id);
    }

    public void LoadNameScene()
    {
        SceneManager.LoadScene(SceneManager.sceneCountInBuildSettings - 1);
    }
    
}
