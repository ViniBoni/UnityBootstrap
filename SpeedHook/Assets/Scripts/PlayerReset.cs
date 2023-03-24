using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerReset : MonoBehaviour
{
    MANAGER manager;

    void Start()
    {
        manager = GameObject.Find("Game Manager").GetComponent<MANAGER>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            manager.DeathScreen();
        }
        else
        {
            other.gameObject.SetActive(false);
        }
    }

}
