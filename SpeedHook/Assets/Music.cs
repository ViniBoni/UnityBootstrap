using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour
{
    // Start is called before the first frame update
    [System.NonSerialized] public bool first;
    void Start()
    {
        if(GameObject.FindGameObjectsWithTag("Music").Length == 1) first = true;
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if(GameObject.FindGameObjectsWithTag("Music").Length > 1)
            foreach (GameObject g in GameObject.FindGameObjectsWithTag("Music"))
            {
                Music m = g.GetComponent<Music>();
                if(!m.first) Destroy(g);
            }

    }
}
