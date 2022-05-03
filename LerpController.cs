using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpController : MonoBehaviour
{
    public float t = 0.0f;
    public bool canUseExtremeValues = false;


    // float sliderValue = 0;
 
    // float minVal = -10;
    // float maxVal = 10;
 
    // void OnGUI () {
    //     sliderValue = GUILayout.VerticalSlider( sliderValue, minVal, maxVal, GUILayout.Height( 800 ), GUILayout.Width( 300 ) );
    // }
 
    // void LateUpdate () {
    //     sliderValue = Mathf.Clamp( sliderValue - Input.GetAxisRaw( "Mouse ScrollWheel" ) * 5f, minVal, maxVal );
    //     // camera.orthographicSize = sliderValue;
    //     t = sliderValue;
    // }
    // void OnPreRender(){
    //     if(!canUseExtremeValues){
    //         t = Mathf.Clamp(t,0.0f,1.0f);
    //     }
    // }
    void Update(){
        if(Input.GetKeyDown(KeyCode.Q)){
            t = t - 0.3f;
        }

        if(Input.GetKeyDown(KeyCode.E)){
            t = t + 0.3f;
        }
        if(!canUseExtremeValues){
            t = Mathf.Clamp(t,0.0f,1.0f);
        }
    }
}

