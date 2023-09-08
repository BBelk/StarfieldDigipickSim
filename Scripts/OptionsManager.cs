using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    public bool audioToggle;
    public bool timerToggle;
    public Text timerText;
    public GameObject timerPanelObj;
    public List<Toggle> allToggles; 
    public void GetToggle(int toggleInt){
        //0 audio 1 timer
        audioToggle = allToggles[0].isOn;
        timerToggle = allToggles[1].isOn;
        Debug.Log("Test " + toggleInt + " " + allToggles[toggleInt].isOn);
        if(toggleInt == 1){
            TimerToggle();
        }
    }
    public void AudioToggle(){
        if(audioToggle){
            //audiomanager on
        }
    }
    public void TimerToggle(){
        if(timerToggle){timerPanelObj.SetActive(true);}
        if(!timerToggle){timerPanelObj.SetActive(false);}
    }

    public float startTime;
    public bool isTimerRunning;
    public Coroutine timerCo;
    private IEnumerator UpdateTimer()
    {
        while (isTimerRunning){
        float elapsedTime = Time.time - startTime;
        int minutes = (int)(elapsedTime / 60);
        int seconds = (int)(elapsedTime % 60);

        string timerTextString = $"{minutes}:{seconds:D2}";
        timerText.text = timerTextString;

            yield return null; // Wait for the next frame
        }
    }
    public void StartTimer(){
        startTime = Time.time;
        isTimerRunning = true;
        if(timerCo != null){StopCoroutine(timerCo);timerCo = null;}
        timerCo = StartCoroutine(UpdateTimer());
    }

    public void EndTimer(){
        isTimerRunning = false;
        if(timerCo != null){StopCoroutine(timerCo);timerCo = null;}
    }
}
