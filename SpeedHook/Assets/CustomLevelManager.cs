using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;
using LootLocker.Requests;
using TMPro;
using UnityEngine.Networking;

/*

    [TODO]
    - Copy over leaderboard object and assign text variables
    - Get list of players with scores on this level from google sheets

*/

public class CustomLevelManager : MonoBehaviour
{
    class LeaderboardEntry
    {
        public int rank = 999999;
        public string playerID;
        public int score;
    }

    List<string> idList = new List<string>();
    public string levelID = "1234";

    public int score;

    List<LeaderboardEntry> entries = new List<LeaderboardEntry>();

    public TextMeshProUGUI playerNames, playerScores;

    bool done;

    public string BASE_URL = "https://docs.google.com/forms/u/0/d/e/1FAIpQLSfJTeOthaWgIi-Q2k71oFOW9OPnqJDiNIYbiHWddQYvUOMrEw/formResponse";




    IEnumerator GetLeaderboard()
    {
        //Get list from google sheet


        

        //For each player id in list
        for (int i = 0; i < idList.Count; i++)
        {
            done = false;
            LeaderboardEntry e = new LeaderboardEntry();
            e.playerID = idList[i];

            //New score, default to infinity
            int score = RoundToInt(Infinity);

            //Get all key/value pairs of current player
            LootLockerSDKManager.GetOtherPlayersPublicKeyValuePairs(idList[i], (response) => 
            {
                if(response.success)
                {
                    LootLockerPayload[] payloads = response.payload;

                    //for each key/value pair
                    for (int x = 0; x < payloads.Length; x++)
                    {
                        //if the key is the level id...
                        if(payloads[x].key == levelID) 
                        {
                            //...set the score to the value
                            score = int.Parse(payloads[x].value);
                            break;
                        }
                    }

                    done = true;
                }
                else
                {
                    Debug.LogError("Failed to get other player's score: " + response.Error);
                    done = true;
                }
            });

            yield return new WaitWhile(() => done == false);

            e.score = score;

            //If a score was found, add it to list
            if(score != RoundToInt(Infinity)) 
                entries.Add(e);

        }



        //If i'm not on list
        if(!idList.Contains(PlayerPrefs.GetString("PlayerID")))
        {
            bool online = false;

            //If i'm also not saved locally, put my id on the google sheet
            if(!PlayerPrefs.HasKey(levelID))
            {         

                WWWForm form = new WWWForm();
                form.AddField("entry.1899509252", levelID);
                form.AddField("entry.2050949384", PlayerPrefs.GetString("PlayerID"));
                
                using (UnityWebRequest www = UnityWebRequest.Post(BASE_URL, form))
                {
                    yield return www.SendWebRequest();
                    if (www.result == UnityWebRequest.Result.ConnectionError)
                    {
                        Debug.Log(www.error);
                    }
                    else
                    {
                        Debug.Log("Success");
                    
                    }
                }


            }

            //If local save is better, keep it.
            else if (PlayerPrefs.GetInt(levelID) < score)
            {
                score = PlayerPrefs.GetInt(levelID);
            }

            StartCoroutine(UpdateScore());



            IEnumerator UpdateScore()
            {
                //Put my score on the local list
                LeaderboardEntry myEntry = new LeaderboardEntry();
                myEntry.playerID = PlayerPrefs.GetString("PlayerID");
                myEntry.score = score;
                entries.Add(myEntry);

                //Put my score on my lootlocker profile
                done = false;

                //Create a payload
                LootLockerPayload payload = new LootLockerPayload();
                payload.key = levelID;
                payload.value = score.ToString();
                payload.is_public = true;

                //Create a request
                LootLockerGetPersistentStorageRequest request = new LootLockerGetPersistentStorageRequest();
                request.AddToPayload(payload);

                //Actually upload score
                LootLockerSDKManager.UpdateOrCreateKeyValue(request, (response) => 
                {

                    if(response.success)
                    {
                        print("Successfully uploaded new score");
                        done = true;
                        online = true;
                    }

                    else 
                    {
                        Debug.LogError("Error uploading new score: " + response.Error);
                        done = true;
                    }

                });

                yield return new WaitWhile(() => done == false);

                //If successfully uploaded score online, add it locally.
                if(online) PlayerPrefs.SetInt(levelID, score);
            }
            
        }




        //Sort local list
        entries.Sort((x, y) => x.score.CompareTo(y.score));




        //Get my place in scoreboard
        int rank = 0;
        for (int i = 0; i < entries.Count; i++)
        {
            if(entries[i].playerID == PlayerPrefs.GetString("PlayerID")) 
            {
                rank = i; 
                break;
            }
        }


        
        //Detect if i'm 5th or above
        int after = rank < 6 ? 0 : rank - 5;

        //If there are up to 10 players,
        //show the top players always
        if(entries.Count <= 10)
        {
            after = 0;
        }

        //If there are more than 10 players, 
        //make sure to always show a top 10
        else
        {
            do { after--; } 
            while (after + 10 > entries.Count);
        }



        //Get list to show
        List<LeaderboardEntry> list = new List<LeaderboardEntry>();
        int length = entries.Count < 10 ? entries.Count : 10;
        ulong[] ids = new ulong[length];

        for (int i = 0; i < length; i++)
        {
            LeaderboardEntry L = entries[after + i];

            L.rank = after + i;
            list.Add(L);

            ids[i] = ulong.Parse(L.playerID);

        }

        //Get player names
        PlayerNameWithIDs[] playerNameWithIDs = new PlayerNameWithIDs[length];
        done = false;
        LootLockerSDKManager.LookupPlayerNamesByPlayerIds(ids, (response) => 
        {

            if(response.success)
            {
                playerNameWithIDs = response.players;
                print("Successfully got player names");
                done = true;
            }
            else
            {
                Debug.LogError("Error getting player names: " + response.Error);
                done = true;
            }

        });

        yield return new WaitWhile(() => done == false);
        

        //Show list
        string tempPlayerNames = "";
        string tempPlayerScores = "";
        for(int i = 0; i < list.Count; i++)
        {
            //Add rank
            tempPlayerNames += list[i].rank + ". ";

            //Add name
            tempPlayerNames += playerNameWithIDs[i].name;

            //Add new line
            tempPlayerNames += "\n";


            //Get score
            float score = list[i].score / 1000f;

            //Display score
            tempPlayerScores += $"{score:#.000}" + "\n";


        }

        playerNames.text = tempPlayerNames;
        playerScores.text = tempPlayerScores;

    }





}
