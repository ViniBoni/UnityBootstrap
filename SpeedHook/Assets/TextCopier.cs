using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TextCopier : MonoBehaviour
{
    public Text copy;

    void Update()
    {
        GetComponent<Text>().text = copy.text;
    }
}
