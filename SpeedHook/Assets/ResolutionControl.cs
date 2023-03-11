using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResolutionControl : MonoBehaviour
{

    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle, vsyncToggle;

    Resolution[] bulkResolutions;
    List<Resolution> resolutions;


    float refreshRate;
    int resolutionIndex;

    // Start is called before the first frame update
    void Start()
    {

        fullscreenToggle.isOn = Screen.fullScreen;
        vsyncToggle.isOn = QualitySettings.vSyncCount != 0;

        bulkResolutions = Screen.resolutions;
        resolutions = new List<Resolution>();

        resolutionDropdown.ClearOptions();
        refreshRate = Screen.currentResolution.refreshRate;


        for(int i = 0; i < bulkResolutions.Length; i++)
        {
            if(bulkResolutions[i].refreshRate == refreshRate)
            {
                resolutions.Add(bulkResolutions[i]);
            }
        }


        List<string> options = new List<string>();
        for(int i = 0; i < resolutions.Count; i++)
        {
            Resolution r = resolutions[i];
            string resolutionOption = r.width + "x" + r.height;
            options.Add(resolutionOption);
            if(r.width == Screen.width && r.height == Screen.height) resolutionIndex = i;
        }



        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = resolutionIndex;
        resolutionDropdown.RefreshShownValue();

    }

    public void SetResolution(int index)
    {
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode);

    }

    public void SetFullscreen(bool f)
    {
        Screen.fullScreen = f;
    }

    public void SetVsync(bool v)
    {
        int count = v ? 1 : 0;

        QualitySettings.vSyncCount = count;
        PlayerPrefs.SetInt("VSync", count);

    }

}
