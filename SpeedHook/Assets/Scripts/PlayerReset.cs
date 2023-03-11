using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerReset : MonoBehaviour
{
    public Transform spawnPoint;
    public PlayerController plr;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Scene s = SceneManager.GetActiveScene();
            SceneManager.LoadScene(s.buildIndex);
        }
        else
        {
            other.gameObject.SetActive(false);
        }
    }

    public void ResetPlayer()
    {
    }
}
