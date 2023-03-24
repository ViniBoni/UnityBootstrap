using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using LootLocker.Requests;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MANAGER : MonoBehaviour
{

    #region Timer Stuff
    [Header("Timer Stuff")]
    public float timer = 0;
    private bool isTimerRunning = false;

    long systemMillisecondsStart;
    public int systemTimerMilliseconds;

    public PlatformTriggerScript startPlatform;
    public List<Checkpoint> checkpoints = new List<Checkpoint>();

    public int currentCheckpoint = 0;

    public Text timerText;

    public bool isTimerSet = false;


    public bool ended;

    #endregion

    #region Leaderboard Stuff


    [Space(15)]
    [Header("Leaderboard Stuff")]
    public string leaderboardID;
    bool cheated;
    public GameObject endScreen;
    public TextMeshProUGUI playerNames;
    public TextMeshProUGUI playerScores;

    public Text bestTime;

    #endregion

    #region Misc

    [Space(15)]
    [Header("Misc")]
    public int nextLevelID;
    public PlayerController player;
    public CheckpointPointer pointer;
    public GameObject deathScreen;
    public AudioClip deathAudio;

    #endregion


    // Start is called before the first frame update
    void Start()
    {

        StartCoroutine(resetTimerDelay());

        //Get guest login
        StartCoroutine(LoginRoutine());

        checkpoints[0].amCurrent = true;
        pointer.follow = checkpoints[0].transform;
    }

    IEnumerator resetTimerDelay()
    {
        yield return new WaitForSeconds(.1f);
        timer = 0;
        isTimerSet = true;
    }

    void Update()
    {
        if(Input.GetKey(KeyCode.R))
        {
            Scene s = SceneManager.GetActiveScene();
            SceneManager.LoadScene(s.buildIndex);
        }
        if(Input.GetKey(KeyCode.P)) Debug.Break();
        if(Input.GetKey(KeyCode.Escape) && !Application.isEditor) SceneManager.LoadScene(0);
        if(Input.GetKey(KeyCode.Return) && ended) SceneManager.LoadScene(nextLevelID);

        if (isTimerRunning && isTimerSet)
        {
            timer += Time.deltaTime;
            systemTimerMilliseconds = Mathf.RoundToInt( DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - systemMillisecondsStart );
        }

        if (isTimerSet && !startPlatform.playerInsideZone)
        {
            if(systemMillisecondsStart < 1) systemMillisecondsStart = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            isTimerRunning = true;
        }

        if (isTimerRunning && checkpoints[currentCheckpoint].playerInsideZone)
        {
            bool passed = true;
            for (int i = 0; i < currentCheckpoint; i++)
            {
                if(!checkpoints[i].passed) passed = false;
            }
            if(passed) 
            {

                Checkpoint c = checkpoints[currentCheckpoint];
                
                c.passed = true;
                c.amCurrent = false;

                AudioSource cAudio = c.GetComponent<AudioSource>();
                cAudio.pitch = UnityEngine.Random.Range(.8f, 1.2f);
                if(c.sound) cAudio.Play();

                for (int i = 0; i < c.deactivate.Count; i++)
                    c.deactivate[i].SetActive(false);
                for (int i = 0; i < c.activate.Count; i++)
                    c.activate[i].SetActive(true);
                
                currentCheckpoint++;

                if(currentCheckpoint == checkpoints.Count)
                {
                    isTimerRunning = false;
                    isTimerSet = false;
                    StartCoroutine(Finish());
                }

                else 
                {
                    checkpoints[currentCheckpoint].amCurrent = true;
                    pointer.follow = checkpoints[currentCheckpoint].transform;
                }
                
            }
        }

        string checkpoint = "";
        if(checkpoints.Count > 1) 
            checkpoint = " - " + currentCheckpoint + "/" + checkpoints.Count;

        if(!ended) 
            timerText.text = $"Time: {timer:#.000}" + checkpoint;

        AssetImporter a = this.GetComponent<AssetImporter>();
        
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
        
        /*
        LootLockerGetPersistentStorageRequest data = new LootLockerGetPersistentStorageRequest();
        LootLockerPayload payload = new LootLockerPayload();
        payload.key = "level id";
        payload.value = "record";
        payload.is_public = true;

        data.AddToPayload(payload);

        LootLockerSDKManager.UpdateOrCreateKeyValue(data, (response) =>
        {
            if(response.success) 
            {
                print("successfully created/updated key value");
            }
            else Debug.LogError("Error creating/updating key value: " + response.Error);
        });
        */



    }

    IEnumerator GetLeaderBoard()
    {
        bool done = false;

        if(!cheated)
        {
            string playerID = PlayerPrefs.GetString("PlayerID");
            LootLockerSDKManager.GetMemberRank(leaderboardID, playerID, (response) =>
            {
                if (response.statusCode == 200)
                {
                    float bestScore = response.score / 1000f;
                    bestTime.text = $"Best Time: {bestScore:#.000}";

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


                            GetMoreMembers();
                            void GetMoreMembers()
                            {
                                if(members.Length < 10) //if in the bottom of the list
                                {
                                    LootLockerSDKManager.GetMemberRank(leaderboardID, playerID, (response) => //get new list
                                    {
                                        if (response.statusCode == 200)
                                        {
                                            float bestScore = response.score / 1000f;
                                            bestTime.text = "Best Time: " + bestScore;
                                            after -= 1;

                                            if(after == -1) 
                                                return;

                                            LootLockerSDKManager.GetScoreList(leaderboardID, count, after, (response) =>
                                            {
                                                //If got leaderboard succesfully
                                                if (response.statusCode == 200)
                                                {

                                                    members = response.items;


                                                    if(members.Length < 10) GetMoreMembers();
                                                }
                                            });  
                                        }
                                    });
                                }
                            }


                            for(int i = 0; i < members.Length; i++)
                            {
                                //Add rank
                                tempPlayerNames += members[i].rank + ". ";

                                //Add name
                                tempPlayerNames += members[i].player.name;

                                //Add new
                                tempPlayerNames += "\n";

                                //Get score
                                float score = members[i].score / 1000f;

                                //Display score
                                tempPlayerScores += $"{score:#.000}" + "\n";


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

        else 
        {
            playerNames.text = "CHEAT DETECTED";
            playerScores.text = ":(";
            bestTime.text = "CHEAT DETECTED";
        }
    }

    public IEnumerator SubmitScore(int scoreToUpload, int systemMilliseconds)
    {

        if(Mathf.Abs(scoreToUpload - systemMilliseconds) > 50) 
        {
            cheated = true;
            goto End;
        }
        
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

        End:
        yield return null;

    }

    public void EndScreen()
    {
        player.DeleteHook(false);
        pointer.gameObject.SetActive(false);
        endScreen.SetActive(true);
        ended = true;
        StartCoroutine(GetLeaderBoard());
    }

    public void DeathScreen()
    {

        player.DeleteHook(false);
        player.a.pitch = UnityEngine.Random.Range(.8f, 1.2f);
        player.a.PlayOneShot(deathAudio);
        ended = true;
        deathScreen.SetActive(true);
        

    }
    public IEnumerator Finish()
    {

        int miliseconds = Mathf.RoundToInt(timer * 1000);

        yield return SubmitScore(miliseconds, systemTimerMilliseconds);
        EndScreen();
    }

}
