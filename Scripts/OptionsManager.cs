using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsManager : MonoBehaviour
{
    public bool audioToggle;
    public bool timerToggle;
    public Text timerText;
    public GameObject timerPanelObj;
    public List<Toggle> allToggles; 
    public bool infoToggle;
    public GameObject infoToggleObject;
    public List<GameObject> infoToggleButtonObjects;
    public List<Sprite> allToggleSprites;
    // 0/1 info, 2/3 audio, 4/5 timer

    public List<AudioClip> allAudioClips;
    public List<AudioSource> allAudioSources;
    public int currentAudioSourceIndex;

    

    void Start(){
        CreateAudioObjectPool();
        Invoke("LoadSettings", 0.05f);
    }

    void Update(){
        if(rotationAudioTimelapseBool){
            rotationAudioTimelapseFloat += Time.deltaTime;
            if(rotationAudioTimelapseFloat >= 0.115f){rotationAudioTimelapseBool = false;}
        }
    }

    public void LoadSettings(){
        var audioInt = PlayerPrefs.GetInt("audioString");
        if(audioInt == 0){// on, default
            audioToggle = true;
            AudioToggle();
        }
        if(audioInt == 1){// off
            audioToggle = false;
            AudioToggle();
        }
    }



    public void MobileMode(){
        infoToggleButtonObjects[0].gameObject.SetActive(false);
    }
    public void AudioToggle(){
        audioToggle = !audioToggle;
        MuteStatus(audioToggle);
        if(audioToggle){
            PlayerPrefs.SetInt("audioString", 1);
            infoToggleButtonObjects[2].GetComponent<Image>().color = Color.black;
            infoToggleButtonObjects[3].GetComponent<Image>().sprite = allToggleSprites[1];
            infoToggleButtonObjects[3].GetComponent<Image>().color = Color.white;
        }
        if(!audioToggle){
            PlayerPrefs.SetInt("audioString", 0);
            infoToggleButtonObjects[2].GetComponent<Image>().color = Color.white;
            infoToggleButtonObjects[3].GetComponent<Image>().sprite = allToggleSprites[0];
            infoToggleButtonObjects[3].GetComponent<Image>().color = Color.black;
        }
    }
    public void ToggleTimer(){
        timerToggle = !timerToggle;
        if(timerToggle){
            timerPanelObj.SetActive(true);
            infoToggleButtonObjects[4].GetComponent<Image>().color = Color.black;
            infoToggleButtonObjects[5].GetComponent<Image>().color = Color.white;
            }
        if(!timerToggle){
            timerPanelObj.SetActive(false);
            infoToggleButtonObjects[4].GetComponent<Image>().color = Color.white;
            infoToggleButtonObjects[5].GetComponent<Image>().color = Color.black;
            }
    }

    public float startTime;
    public bool isTimerRunning;
    public Coroutine timerCo;
    private IEnumerator UpdateTimer(){
        while (isTimerRunning){
        float elapsedTime = Time.time - startTime;
        int minutes = (int)(elapsedTime / 60);
        int seconds = (int)(elapsedTime % 60);

        string timerTextString = $"{minutes}:{seconds:D2}";
        timerText.text = timerTextString;

            yield return null;
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

    public void ToggleInfoPanel(){
        infoToggle = !infoToggle;
        if(infoToggle){
            infoToggleObject.SetActive(true);
            infoToggleButtonObjects[0].GetComponent<Image>().color = Color.black;
            infoToggleButtonObjects[1].GetComponent<TMP_Text>().color = Color.white;
        }
        if(!infoToggle){
            infoToggleObject.SetActive(false);
            infoToggleButtonObjects[0].GetComponent<Image>().color = Color.white;
            infoToggleButtonObjects[1].GetComponent<TMP_Text>().color = Color.black;
        }
    }


    public void CreateAudioObjectPool(){
        for(int x = 0; x < 15; x++){
            var newAS = Instantiate(allAudioSources[x].gameObject, allAudioSources[x].gameObject.transform.parent);
            allAudioSources.Add(newAS.GetComponent<AudioSource>());
        }
    }

    public float rotationAudioTimelapseFloat;
    public bool rotationAudioTimelapseBool;

    public void PlayAudioClip(int clipIndex){
        //0 change ring 1 move/rotate ring 2 finish 3 regular submit 4 undo 5 start long 6 med 7 short

        if(rotationAudioTimelapseFloat < 0.115f && clipIndex == 1){return;}
        if(clipIndex == 1){rotationAudioTimelapseFloat = 0f;rotationAudioTimelapseBool = true;}

        allAudioSources[currentAudioSourceIndex].clip = allAudioClips[clipIndex];
        allAudioSources[currentAudioSourceIndex].Play();
        currentAudioSourceIndex += 1;
        if(currentAudioSourceIndex >= allAudioSources.Count){
            currentAudioSourceIndex = 0;
        }
    }

    public Coroutine DelayCo;
    public void DelayAudio(int clipIndex, float delayTime){
        if(DelayCo != null){StopCoroutine(DelayCo);DelayCo = null;}
        DelayCo = StartCoroutine(DelayIEnum(clipIndex, delayTime));
    }

    public IEnumerator DelayIEnum(int clipIndex, float delayTime){
        yield return new WaitForSeconds(delayTime);
        PlayAudioClip(clipIndex);
    }
    public void MuteStatus(bool newStatus){
        foreach(AudioSource newAS in allAudioSources){
            newAS.mute = newStatus;
        }
    }
}
