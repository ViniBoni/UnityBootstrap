using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerReset : MonoBehaviour
{
    public Transform spawnPoint;
    public PlayerController plr;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ResetPlayer(other.gameObject);
        }
        else
        {
            other.gameObject.SetActive(false);
        }
    }

    public void ResetPlayer(GameObject other)
    {
        other.transform.position = spawnPoint.position;
        plr.DeleteHook();
        plr.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }
}
