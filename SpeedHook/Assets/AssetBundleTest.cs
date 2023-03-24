using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AssetBundleTest : MonoBehaviour
{
    /*
    [TODO]
    Create a new csv for every object, adding to the asset bundle stuff like the model, materials, texture if it's 2d, etc, and referencing it on the csv
    */

    public Component[] c;
 
    // Start is called before the first frame update
    void Start()
    {
        AssetImporter a = AssetImporter.GetAtPath("Assets/Big Man Variant.prefab");
        a.assetBundleName = "assetbundletestmacos";

        Component[] allComps = GetComponents(typeof(Component));
        c = allComps;
        
        GameObject g = new GameObject();
        g.AddComponent(c[1].GetType());
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
