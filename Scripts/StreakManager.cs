using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using System.IO;
using System.Runtime.InteropServices;
using Unity.VisualScripting.Dependencies.NCalc;
using System.Diagnostics;

public class StreakManager : MonoBehaviour
{
    public bool testing;
    public bool doDelete;
    public GameManager GameManager;
    public TMP_Text countTMP_Text;
    public long currentDayUnixTimestamp;
    public List<GameObject> allStarCountPanelObjects;
    public int completedCount;
    public int attemptedCount;

    public bool toggleStreak;

    public List<Sprite> allStarSprites;
    public List<GameObject> allStarImgObjects;
    public List<string> allSavedStrings;
    public List<string> allPreGenStrings;

    public bool doTest;

    public Button dailyButton;

    public List<TMP_Text> allStatTMP_Texts;
    public List<int> allGlobalCounts;
    // 0 total, 1 undo, 2 auto

    public GameObject statsScreenObject;
public void GotNewString(string newString){
    // I was using this to print stuff to the streak counter text box. Imma leave it, its handing for testing things in webgl
    UnityEngine.Debug.Log("" + newString);
    countTMP_Text.text = newString;
    countTMP_Text.fontSize = 24f;
    LayoutRebuilder.ForceRebuildLayoutImmediate(countTMP_Text.transform.parent.gameObject.GetComponent<RectTransform>());
}
    void Start()
    {
        DeactivateStarCountObjects();
        persistentDataPathString = "/idbfs/Digipick90210/";

        //I've been doing a lot of testing and I'm tired, so for now this is a bit of a mess. Sue me! I'll clean it later

        if(doDelete){
            DeleteFile();
            ReadLinesFromFile();
        }


        if(testing){
            DeleteFile();
        }
        ReadLinesFromFile();
        if(testing){
            GenerateTestDataFromString();
            WriteLinesToFile();
        }
        CheckPreviousEntriesFromStringList();
    }


public void DebugOpenDirectory()
{
    UnityEngine.Debug.Log("OPEN LOG");
    if (Directory.Exists(persistentDataPathString))
    {
        Process.Start("explorer.exe", "/select,\"" + persistentDataPathString.Replace("\\", "/") + "\"");
    }
    else
    {
        UnityEngine.Debug.LogWarning("Directory does not exist: " + persistentDataPathString);
    }
}

public void DeleteFile(){
    persistentDataPathString = "/idbfs/Digipick90210/";
    string filePath = Path.Combine(persistentDataPathString, "saveFile.txt");
    if (File.Exists(filePath))
    {
        try
        {
            File.Delete(filePath);
            UnityEngine.Debug.Log("File deleted: " + filePath);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("Error deleting file: " + ex.Message);
        }
    }
    else
    {
        UnityEngine.Debug.LogWarning("File does not exist: " + filePath);
    }
}


    public void DebugOpenFile(){
        UnityEngine.Debug.Log("OPEN LOG");
         string filePath = Path.Combine(persistentDataPathString, "saveFile.txt");
        if (File.Exists(filePath)){
            Application.OpenURL("file://" + filePath.Replace("\\", "/"));
        }
    }

    public void ReadLinesFromFile(){
        currentDayUnixTimestamp = (System.DateTime.Now.Date.Ticks / System.TimeSpan.TicksPerSecond);
         string filePath = Path.Combine(persistentDataPathString, "saveFile.txt");

        if (!Directory.Exists(persistentDataPathString))
        {
            UnityEngine.Debug.Log("PATH DOES NOT EXIST READ");
            Directory.CreateDirectory(persistentDataPathString);
        }

        if (!File.Exists(filePath))
        {
            UnityEngine.Debug.Log("FILE DOES NOT EXIST READ");
            allSavedStrings.Clear();
            allSavedStrings.Add($"Digipick-Save-File {allGlobalCounts[0]} {allGlobalCounts[1]} {allGlobalCounts[2]}");
            allSavedStrings.Add($"-THIS-LINE-INTENTIONALLY-LEFT-BLANK-");
            allSavedStrings.Add($"-THIS-LINE-INTENTIONALLY-LEFT-BLANK-");
            File.WriteAllLines(filePath, allSavedStrings.ToArray());
            PushToFile();

            // return;
        }
        allSavedStrings = new List<string>(File.ReadAllLines(filePath));
        for(int w = 0; w < allGlobalCounts.Count; w++){
            allGlobalCounts[w] = GetIntFromString(allSavedStrings[0], w+1);
        }
        for(int x = 0; x< allSavedStrings.Count; x++){
            UnityEngine.Debug.Log("LOADED: " + allSavedStrings[x]);
        }
    }

    public void WriteLinesToFile(){
        string filePath = Path.Combine(persistentDataPathString, "saveFile.txt");
        if (!Directory.Exists(persistentDataPathString)){
            Directory.CreateDirectory(persistentDataPathString);
        }
        File.WriteAllLines(filePath, allSavedStrings.ToArray());
        PushToFile();
        UnityEngine.Debug.Log("SAVED TO FILE WRITE");
    }

    public string persistentDataPathString;
    public void TestSave(){
    string filePath = Path.Combine(persistentDataPathString, "saveFile.txt");

    if (!Directory.Exists(persistentDataPathString))
    {
        Directory.CreateDirectory(persistentDataPathString);
    }

    if (!File.Exists(filePath))
    {
        File.WriteAllText(filePath, "Digipick Save File ");
    }
    var getOldText = File.ReadAllText(filePath);
    File.AppendAllText(filePath, "| 6test ");

    var newString = File.ReadAllText(filePath);
    countTMP_Text.text = newString;
    countTMP_Text.fontSize = 24f;
    LayoutRebuilder.ForceRebuildLayoutImmediate(countTMP_Text.transform.parent.gameObject.GetComponent<RectTransform>());
}

    public void PushToFile(){
        //I struggled with this for a couple of days. Apparently, if you writeText/lines to a webgl project in the Start method, it will go through. (though maybe it allows the first write everytime, I'm not testing it). The problem is, indexedDB doesn't update automatically after that, so for every additional rewrite we have to sync it manually.
        #if UNITY_WEBGL
            Application.ExternalEval("_JS_FileSystem_Sync();");
        #endif
    }

    public void SaveInput(int newInput, int timeElapsed){
        // PlayerPrefs.SetInt("" + (currentDayUnixTimestamp), newInput);
        if(newInput == 1){
            allSavedStrings.Add($"{currentDayUnixTimestamp} {newInput} {0} {0}");
        }
        if(newInput == 2){
            allSavedStrings[allSavedStrings.Count - 1] = $"{currentDayUnixTimestamp} {newInput} {timeElapsed} {0}";
        }
        WriteLinesToFile();
        CheckPreviousEntriesFromStringList();

        // toggleStreak = !toggleStreak;
        // ToggleStreak();
    }
    
    public void GenerateTestDataFromString(){
        var generateInt = 5;
        var newDayUnixTimestamp = currentDayUnixTimestamp;
        var newTime = newDayUnixTimestamp;
        newDayUnixTimestamp -= 86400;
        var elapsedTime = 61;
        for(int x = 0; x < generateInt; x++){
            var rand = UnityEngine.Random.Range(1, 3);
            // PlayerPrefs.SetInt("" + (newDayUnixTimestamp -= 86400), rand);
            newDayUnixTimestamp -= 86400;
            allSavedStrings.Insert(3, $"{newDayUnixTimestamp} {rand} {elapsedTime +=1}");
        }

        newDayUnixTimestamp -= (86400 * 5);

        for(int x = 0; x < generateInt + 2; x++){
            var rand = UnityEngine.Random.Range(1, 3);
            newDayUnixTimestamp -= 86400;
            allSavedStrings.Insert(3, $"{newDayUnixTimestamp} {rand} {elapsedTime +=8}");
        }
    }

    ////
    public string ReturnPartFromString(string inputString, int newPartIndex){
    string[] parts = inputString.Split(' ');
    if (newPartIndex >= 0 && newPartIndex < parts.Length){
        return parts[newPartIndex];
    }
    else{
        return string.Empty;
    }
}
    //// 0 unixtimestamp, 1 lock status, 2 time for solve, 3 turns to solve? 4 uhhh 

    public long GetUnixTimestampFromString(string newString){
        return long.Parse(ReturnPartFromString(newString, 0));
    }

    public int GetTimeStatusFromString(string newString){
        return int.Parse(ReturnPartFromString(newString, 1));
    }

    public int GetIntFromString(string newString, int newIndex){
        return int.Parse(ReturnPartFromString(newString, newIndex));
    }

    public List<int> streakList;

    public void CheckPreviousEntriesFromStringList(){
        completedCount = 0;
        attemptedCount = 0;

        UnityEngine.Debug.Log("allSavedStrings.Count:" + allSavedStrings.Count);

        var newDayUnixTimestamp = currentDayUnixTimestamp;
        dailyButton.interactable = true;
        var newTimeStatus = 0;
        if(allSavedStrings.Count <= 3){DeactivateStarCountObjects();return;}
            
        if(GetUnixTimestampFromString(allSavedStrings[allSavedStrings.Count -1]) == currentDayUnixTimestamp){
            //today already played
            UnityEngine.Debug.Log("TODAY PLAYED");
            newTimeStatus = GetTimeStatusFromString(allSavedStrings[allSavedStrings.Count -1]);
            if(newTimeStatus >= 1){;dailyButton.interactable = false;}
            
        }
        newTimeStatus = 3;
        // while(newTimeStatus != 0){
            var list9000Index = 0;
            List<int> avgTimeList = new List<int>();
            // streakList.Add(0);
            streakList.Clear();
        var lastUnix = newDayUnixTimestamp;
        if(allSavedStrings.Count > 3){
        for(int x = allSavedStrings.Count - 1; x > 2; x--){
            avgTimeList.Add(GetIntFromString(allSavedStrings[x], 2));
            newTimeStatus = GetTimeStatusFromString(allSavedStrings[x]);
            
            if(newTimeStatus == 1){attemptedCount += 1;}
            if(newTimeStatus == 2){completedCount += 1;}
            if((lastUnix != GetUnixTimestampFromString(allSavedStrings[x]) + 86400) && x != allSavedStrings.Count -1){
                streakList.Add(list9000Index);
                list9000Index = 0;
            }
            list9000Index += 1;
            lastUnix = GetUnixTimestampFromString(allSavedStrings[x]);
        }
        }
        streakList.Add(list9000Index);

        var combinedCount = (attemptedCount + completedCount);
        if(combinedCount <= 0){
            DeactivateStarCountObjects();
        }
        if(combinedCount > 0){
            ActivateStarCountObjects();
        }
        var getTime = 0;
        for(int x = 0; x< avgTimeList.Count; x++){
            if(avgTimeList[x] != 0){
                getTime += avgTimeList[x];
            }
        }
        getTime = getTime/avgTimeList.Count;
        //
        // UnityEngine.Debug.Log("GET TIME: " + getTime);

        countTMP_Text.text = "" + streakList[0];
        UpdateStats(combinedCount, getTime);
    }

    public void UpdateStats(int combinedCount, int getTime){
        allStatTMP_Texts[0].text = ""+combinedCount;
        //
        float percent = ((float)completedCount / combinedCount) * 100f;
        percent = Mathf.Round(percent * 10f) / 10f;
        UnityEngine.Debug.Log($"{completedCount}, {combinedCount}, {percent}");
        allStatTMP_Texts[1].text = $"{percent}%";
        //
        int minutes = (int)(getTime / 60);
        int seconds = (int)(getTime % 60);
        string timerTextString = $"{minutes}:{seconds:D2}";
        allStatTMP_Texts[2].text = $"{timerTextString}";
        //
        allStatTMP_Texts[3].text = $"{streakList[0]}";
        //
        int largestValue = streakList[0];
        int largestValueIndex = 0;
        for (int i = 1; i < streakList.Count; i++){
            if (streakList[i] > largestValue){
                largestValue = streakList[i];
                largestValueIndex = i;
            }
        }
        allStatTMP_Texts[4].text = $"{streakList[largestValueIndex]}";
        //
        allStatTMP_Texts[5].text = ""+allGlobalCounts[0];
        allStatTMP_Texts[6].text = ""+allGlobalCounts[1];
        allStatTMP_Texts[7].text = ""+allGlobalCounts[2];
    }

    public void DeactivateStarCountObjects(){
        foreach(GameObject newObj in allStarCountPanelObjects){
                newObj.SetActive(false);
        }
    }
    public void ActivateStarCountObjects(){
        foreach(GameObject newObj in allStarCountPanelObjects){
                newObj.SetActive(true);
        }
    }


    public void CloseStatsIfOpen(){
        if(toggleStreak){
            ToggleStreak();
        }
    }
    public void ToggleStreak(){
        toggleStreak = !toggleStreak;
        var combinedCount = (attemptedCount + completedCount);
        if(!toggleStreak){
            StatsScreenDown();
            // countTMP_Text.fontSize = 52f;
            // countTMP_Text.text = "" + combinedCount;
            //
            allStarImgObjects[0].GetComponent<Image>().color = Color.white;
            allStarImgObjects[1].GetComponent<Image>().color = Color.black;
            allStarImgObjects[1].GetComponent<Image>().sprite = allStarSprites[0];
        }
        if(toggleStreak){
            StatsScreenUp();
            // countTMP_Text.fontSize = 32f;
            // countTMP_Text.text = $"<u>Attempted:</u> {attemptedCount} <u>Complete:</u> {completedCount} <u>Total:</u> {combinedCount}";
            //
            allStarImgObjects[0].GetComponent<Image>().color = Color.black;
            allStarImgObjects[1].GetComponent<Image>().color = Color.white;
            allStarImgObjects[1].GetComponent<Image>().sprite = allStarSprites[0];
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate( countTMP_Text.transform.parent.gameObject.GetComponent<RectTransform>());
    }

    public void UpdateCount(int countIndex){
        allGlobalCounts[countIndex] += 1;
        allSavedStrings[0] = $"Digipick-Save-File {allGlobalCounts[0]} {allGlobalCounts[1]} {allGlobalCounts[2]}";
        WriteLinesToFile();
    }

    public void StatsScreenUp(){
        statsScreenObject.SetActive(true);
        CheckPreviousEntriesFromStringList();
    }

    public void StatsScreenDown(){
        statsScreenObject.SetActive(false);
    }
}
