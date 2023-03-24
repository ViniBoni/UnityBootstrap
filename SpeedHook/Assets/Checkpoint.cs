using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    /*
    [TODO]
    Add bools
        require previous
        count to total
        include flourish
        final
    
    Add events
        execute of complete

    */
    public bool playerInsideZone = false;
    public bool passed = false;
    public bool amCurrent = false;
    bool wasCurrent = false;

    public bool sound = true;


    List<GameObject> flourish = new List<GameObject>();


    [Space(15)]
    public List<GameObject> deactivate = new List<GameObject>();
    public List<GameObject> activate = new List<GameObject>();


    void Awake()
    {
        passed = false;
        for (int i = 0; i < transform.childCount; i++)
            flourish.Add(transform.GetChild(i).gameObject);
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInsideZone = true;

            gameObject.AddComponent<BoxCollider>();
            
        
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInsideZone = false;
        
    }

    void Update()
    {
        if(amCurrent != wasCurrent)
        {
            wasCurrent = amCurrent;
            for (int i = 0; i < flourish.Count; i++)
            {
                flourish[i].SetActive(amCurrent);
            }
        }
    }

}
