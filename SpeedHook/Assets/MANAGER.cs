using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using LootLocker.Requests;
using TMPro;
using UnityEngine.UI;

public class MANAGER : MonoBehaviour
{

    public string leaderboardID;
    public GameObject endScreen;
    public bool ended;

    public TextMeshProUGUI playerNames;
    public TextMeshProUGUI playerScores;

    public Text bestTime;

    public int nextLevelID;

    // Start is called before the first frame update
    void Start()
    {


        //Get guest login
        StartCoroutine(LoginRoutine());
    }


    void Update()
    {
        if(Input.GetKey(KeyCode.R))
        {
            Scene s = SceneManager.GetActiveScene();
            SceneManager.LoadScene(s.buildIndex);
        }
        if(Input.GetKey(KeyCode.P)) Debug.Break();
        if(Input.GetKey(KeyCode.Escape) && ended) SceneManager.LoadScene(0);
        if(Input.GetKey(KeyCode.Return) && ended) SceneManager.LoadScene(nextLevelID);
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
                Debug.Log("Failed to start LootLocker session: " + response.Error);
                done = true;
            }
        });
        yield return new WaitWhile(() => done == false);
    }

    IEnumerator GetLeaderBoard()
    {
        bool done = false;

        print (PlayerPrefs.GetString("PlayerID"));
        string playerID = PlayerPrefs.GetString("PlayerID");
        LootLockerSDKManager.GetMemberRank(leaderboardID, playerID, (response) =>
        {
            if (response.statusCode == 200)
            {
                float bestScore = response.score / 1000f;
                bestTime.text = "Best Time: " + bestScore;

                int rank = response.rank;
                int count = 10;
                int after = rank < 6 ? 0 : rank - 5;

                LootLockerSDKManager.GetScoreList(leaderboardID, count, after, (response) =>
                {
                    //If got leaderboard succesfully
                    if (response.statusCode == 200)
                    {
                        Debug.Log("Successfully got player rank");




                        string tempPlayerNames = "";
                        string tempPlayerScores = ""; 


                        LootLockerLeaderboardMember[] members = response.items;


                        for(int i = 0; i < members.Length; i++)
                        {
                            tempPlayerNames += members[i].rank + ". ";
                            tempPlayerNames += members[i].player.name;

                            float score = members[i].score / 1000f;
                            tempPlayerScores += score + "\n";
                            tempPlayerNames += "\n";
                        }

                        playerNames.text = tempPlayerNames;
                        playerScores.text = tempPlayerScores;

                        done = true;
                    }
                    else
                    {
                        Debug.Log("failed: " + response.Error);
                        done = true;
                    }
                });
            }
            else
            {
                Debug.Log("Failed to get player rank: " + response.Error);
                done = true;
            }
        });

        yield return new WaitWhile(() => done == false);
    }

    public IEnumerator SubmitScoreRoutine(int scoreToUpload)
    {
        bool done = false;
        string playerID = PlayerPrefs.GetString("PlayerID");
        LootLockerSDKManager.SubmitScore(playerID, scoreToUpload, leaderboardID, (response) => 
        {
            if(response.success)
            {
                Debug.Log("Successfully uploaded score");
                done = true;
            }
            else
            {
                Debug.LogError("Failed uploading score, error: " + response.Error);
                done = true;
            }
        });
        yield return new WaitWhile(() => done == false);

    }

    public void EndScreen()
    {

        endScreen.SetActive(true);
        ended = true;
        StartCoroutine(GetLeaderBoard());
    }

}
