using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class StreakManager : MonoBehaviour
{
    public GameManager GameManager;
    public TMP_Text countTMP_Text;
    public long currentDayUnixTimestamp;
    public List<GameObject> allStarCountPanelObjects;
    public int completedCount;
    public int attemptedCount;

    public bool toggleStreak;

    public List<Sprite> allStarSprites;
    public List<GameObject> allStarImgObjects;

    public bool doTest;

    public Button dailyButton;
    void Start()
    {
       currentDayUnixTimestamp = (System.DateTime.Now.Date.Ticks / System.TimeSpan.TicksPerSecond);

        CheckPreviousEntries();
    }
    
        public void GenerateTestData(int generateInt){
        var newDayUnixTimestamp = currentDayUnixTimestamp;
        for(int x = 0; x < generateInt; x++){
            var rand = UnityEngine.Random.Range(1, 3);
            PlayerPrefs.SetInt("" + (newDayUnixTimestamp -= 86400), rand);
        }
    }

    public void SaveInput(int newInput){
        PlayerPrefs.SetInt("" + (currentDayUnixTimestamp), newInput);
        CheckPreviousEntries();

        toggleStreak = !toggleStreak;
        ToggleStreak();
    }


    public void CheckPreviousEntries(){
        completedCount = 0;
        attemptedCount = 0;

        var newDayUnixTimestamp = currentDayUnixTimestamp;
        dailyButton.interactable = true;
        var newTimeStatus = PlayerPrefs.GetInt("" + newDayUnixTimestamp); //starting loop
        if(newTimeStatus == 1){attemptedCount += 1;dailyButton.interactable = false;}
        if(newTimeStatus == 2){completedCount += 1;dailyButton.interactable = false;}
        newTimeStatus = 3;
        while(newTimeStatus != 0){
            newTimeStatus = PlayerPrefs.GetInt("" + (newDayUnixTimestamp -= 86400));
            if(newTimeStatus == 1){attemptedCount += 1;}
            if(newTimeStatus == 2){completedCount += 1;}
        }
        var combinedCount = (attemptedCount + completedCount);
        countTMP_Text.text = "" + combinedCount;
        if(combinedCount <= 0){
            foreach(GameObject newObj in allStarCountPanelObjects){
                newObj.SetActive(false);
            }
        }
        if(combinedCount > 0){
            foreach(GameObject newObj in allStarCountPanelObjects){
                newObj.SetActive(true);
            }
        }
    }

    public void ToggleStreak(){
        toggleStreak = !toggleStreak;
        var combinedCount = (attemptedCount + completedCount);
        if(!toggleStreak){
            countTMP_Text.fontSize = 52f;
            countTMP_Text.text = "" + combinedCount;
            //
            allStarImgObjects[0].GetComponent<Image>().color = Color.white;
            allStarImgObjects[1].GetComponent<Image>().color = Color.black;
            allStarImgObjects[1].GetComponent<Image>().sprite = allStarSprites[0];
        }
        if(toggleStreak){
            countTMP_Text.fontSize = 32f;
            countTMP_Text.text = $"<u>Attempted:</u> {attemptedCount} <u>Complete:</u> {completedCount} <u>Total:</u> {combinedCount}";
            //
            allStarImgObjects[0].GetComponent<Image>().color = Color.black;
            allStarImgObjects[1].GetComponent<Image>().color = Color.white;
            allStarImgObjects[1].GetComponent<Image>().sprite = allStarSprites[0];
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate( countTMP_Text.transform.parent.gameObject.GetComponent<RectTransform>());
    }
}
