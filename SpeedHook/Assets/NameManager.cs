using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;
using TMPro;
using UnityEngine.SceneManagement;

public class NameManager : MonoBehaviour
{
    
    public TMP_InputField i;

    void Start()
    {
        StartCoroutine(LoginRoutine());
    }

    IEnumerator LoginRoutine()
    {
        bool done = false;
        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if(response.success)
            {
                Debug.Log("Successfully started LootLocker session");
                PlayerPrefs.SetString("PlayerID", response.player_id.ToString());
                done = true;
                return;
            }
            else 
            {
                Debug.Log("Error starting LootLocker session");
                done = true;
            }
        });
        yield return new WaitWhile(() => done == false);
    }

    public void SetPlayerName()
    {
        if(i.text.Length <= 8)
        {
            PlayerPrefs.SetString("Name", i.text);
            LootLockerSDKManager.SetPlayerName(i.text, (response) =>
            {
                if(response.success)
                {
                    Debug.Log("Player name set! :)");
                }
                else
                {
                    Debug.LogError("Player name not set :( " + response.Error);
                }
            });
            SceneManager.LoadScene(0);
        }
        else
        {
            i.text = "";
            i.placeholder.GetComponent<TextMeshProUGUI>().text = "max 8 characters!";
        }
    }


}
