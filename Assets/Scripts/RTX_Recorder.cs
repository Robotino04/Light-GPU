using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTX_Recorder : MonoBehaviour
{
    public RTX_Master master;
    public List<GameObject> hideOnScreenshot = new List<GameObject>();

    private int recordStage = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            recordStage = 1;
        }
        switch (recordStage){
            case 1:
                foreach(GameObject obj in hideOnScreenshot){
                    obj.SetActive(false);
                }
                recordStage = 2;
                break;
            case 2:
                ScreenCapture.CaptureScreenshot("Record/" +  master._currentSample + ".png");
                recordStage = 3;
                break;
            case 3:
                foreach(GameObject obj in hideOnScreenshot){
                    obj.SetActive(true);
                }
                recordStage = 0;
                break;
        }
    }
}
