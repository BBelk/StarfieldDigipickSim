using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public StreakManager StreakManager;
    public Text debugText;
    public bool doBeTesting;
    void Start(){
        Invoke("StartDelay", 0.05f);
    }

    public void StartDelay(){
        CanvasGroupAlphaCos.Add(null);
        Generator();
        ResetAll();
        textDirectObj.SetActive(true);

        // if(Application.platform == RuntimePlatform.WebGLPlayer){
        //     Debug.Log("WEBGL");
        //     debugText.text = debugText.text + ": WEBGL";
        // }

        if(Application.isMobilePlatform || doBeTesting){
            // Debug.Log("MOBILE MODE");
            SetUIToAnchors(1);
            OptionsManager.MobileMode();
            InputManager.SelectUndoAutoButtonObjects(1);
            // return;
        }
        if(!Application.isMobilePlatform && !doBeTesting){
            // Debug.Log("DESKTOP MODE");
            OptionsManager.ToggleInfoPanel();
            InputManager.SelectUndoAutoButtonObjects(0);
            SetUIToAnchors(0);
        } 

        var doInitialUndoAuto = PlayerPrefs.GetInt("intialUndoAuto");
        if(doInitialUndoAuto == 0){
            InitialUndoAuto();
        }
        if(doInitialUndoAuto != 0){
            LoadUndoAuto();
        }

    }

    public OptionsManager OptionsManager;
    public List<GameObject> allSegments;
    public PickManager mainPickManager;
    public InputManager InputManager;

    public List<PickManager> allPickManagers;
    public List<SegmentManager> allSegmentManagers;
    public int chosenSegmentManagerInt;
    public int chosenPickManagerInt;
    public List<int> pickStatus;

    public GameObject ringObject;
    public Vector3[] ringScales;
    public int segmentCurrentCount;
    public int segmentMaxCount;
    public List<int> previousPickManagers;

    public Coroutine RingScalerCo;
    public bool canInput;

    public List<Coroutine> CanvasGroupAlphaCos = new List<Coroutine>(); 

    public GameObject textDirectObj;
    public GameObject dailyContinueButtonObject;

    public List<RectTransform> allUIRectTransforms;
    public List<RectTransform> allUIAnchorRectTransforms; 

    public List<int> allUndoAutoAmounts;
    public List<float> allUndoAutoFillAmounts;
    public void Generator(){

        foreach(SegmentManager newSM in allSegmentManagers){
            newSM.Generator();
        }

        mainPickManager.Generator();
        allPickManagers[0].Generator();

        for(int y = 1; y < 12; y++){
            var newPM = Instantiate(allPickManagers[0], allPickManagers[0].transform.parent);
            allPickManagers[y] = newPM;
            newPM.myIndex = y;
            newPM.UpdateRing();
        }
        allPickManagers[0].gameObject.transform.parent.gameObject.SetActive(false);
    }

    public void NewInput(int dir){
        allPickManagers[chosenPickManagerInt].MovePicks(dir);
        mainPickManager.MovePicks(dir);
    }

    public void NewPickChosen(int pickManagerIndex){
        chosenPickManagerInt = pickManagerIndex;
        mainPickManager.CopyPicks(allPickManagers[chosenPickManagerInt]);
        DeselectOtherPMs(chosenPickManagerInt);

    }

    public void DeselectOtherPMs(int skipIndex){
        for(int x = 0; x < allPickManagers.Count; x++){
            if(x == skipIndex){continue;}
            allPickManagers[x].Deselected();
        }
    }

    public void DifficultySelection(int difficultyIndex, long seed = 0){
        // novice 0, advanced 1, expert 2, master 3
        ResetAll();
        OptionsManager.EndTimer();
        allPickManagers[0].gameObject.transform.parent.gameObject.SetActive(true);
        textDirectObj.SetActive(false);
        canInput = true;

        if(seed > 0){
            SetUndoAuto();
            isDaily = false;
        }

        if(seed != 0){
            //is daily
            int intSeed = (int)(seed % int.MaxValue);
            Debug.Log("SEEED: " + intSeed);
        // Set the seed for the Random Number Generator
        UnityEngine.Random.InitState(intSeed);
        }
        
        chosenSegmentManagerInt = 0;
        chosenPickManagerInt = -1;
        SetAllCanvasGroupsAlphaZero();
        
        if(difficultyIndex == 0){
            //novice
            segmentMaxCount = 2;
            for(int x = 0; x < segmentMaxCount; x++){allSegmentManagers[x].PokeHoles(0, x);}
        }
        if(difficultyIndex == 1){
            //advanced
            segmentMaxCount = 2;
            for(int x = 0; x < segmentMaxCount; x++){allSegmentManagers[x].PokeHoles(1, x);}
            MakeRandomPicks(2, new Vector2Int(2, 4));
        }
        if(difficultyIndex == 2){
            //expert
            segmentMaxCount = 3;
            for(int x = 0; x < segmentMaxCount; x++){allSegmentManagers[x].PokeHoles(2, x);}
            MakeRandomPicks(3, new Vector2Int(1, 4));
        }
        if(difficultyIndex == 3){
            //master
            segmentMaxCount = 4;
            for(int x = 0; x < segmentMaxCount; x++){allSegmentManagers[x].PokeHoles(2, x);}
            MakeRandomPicks(4, new Vector2Int(1, 4));
        }
        OptionsManager.StartTimer();
        ActivateSegmentManagers(0, segmentMaxCount);
        RandomizePickManagers();
        FindNextPickManager(1);
    }

    public void MakeRandomPicks(int randomAmount, Vector2 randomRange){
        for(int x = 0; x < randomAmount; x++){
        var chooseRange = UnityEngine.Random.Range(randomRange.x, randomRange.y);
        var getPickManager = ReturnRandomPickManager();
        List<int> availableIndices = AvailableIndices();
        List<int> chosenIndexes = new List<int>();
        for (int i = 0; i < chooseRange; i++){
            int randomIndex = UnityEngine.Random.Range(0, availableIndices.Count);
            int selectedIndex = availableIndices[randomIndex];
            chosenIndexes.Add(selectedIndex);
            availableIndices.RemoveAt(randomIndex);
        }
        getPickManager.gameObject.SetActive(true);
        getPickManager.AcquirePicks(chosenIndexes, -1);
        }
    }

    public List<int> AvailableIndices(){
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < 32; i++){
            if (i % 2 == 0){
                availableIndices.Add(i);
            }
        }
        return availableIndices;
    }

    public void ActivateSegmentManagers(int indexToStart, int amountToActivate){
        foreach(SegmentManager newSM in allSegmentManagers){
            newSM.gameObject.SetActive(false);
        }
        var newAlpha = 0.8f;
        CanvasGroupAlphaCos.Clear();
        
        EndCanvasGroupAlphaCos();
        for(int x = indexToStart; x < amountToActivate; x++){
            var getObj = allSegmentManagers[x].gameObject; 
            getObj.SetActive(true);
            var newCGAC = StartCoroutine(CanvasGroupAlphaCoroutine(x, newAlpha));
            CanvasGroupAlphaCos.Add(newCGAC);
            newAlpha *= 0.4f;
        }
    }

    public void SetAllCanvasGroupsAlphaZero(){
        foreach(SegmentManager newSM in allSegmentManagers){
            newSM.GetComponent<CanvasGroup>().alpha = 0f;
        }
    }

    public IEnumerator CanvasGroupAlphaCoroutine(int segmentToAlpha, float newAlpha){
        var elapsedTime = 0f;
        var loadTime = 1f;
    	while(elapsedTime < loadTime){
            var getCG = allSegmentManagers[segmentToAlpha].GetComponent<CanvasGroup>(); 
            getCG.alpha = Mathf.Lerp(getCG.alpha, newAlpha, elapsedTime / loadTime);
    		elapsedTime += Time.deltaTime;
        	yield return new WaitForEndOfFrame();
    	}
    }

    public void EndCanvasGroupAlphaCos(){
        for (int x = 0; x < CanvasGroupAlphaCos.Count; x++){
            StopCoroutine(CanvasGroupAlphaCos[x]);
        }
    }

    public void RandomizePickManagers(){
        var getParent = allPickManagers[0].gameObject.transform.parent;
         int childCount = getParent.childCount;

        List<Transform> childrenList = new List<Transform>();

        for (int i = 0; i < childCount; i++){
            childrenList.Add(getParent.GetChild(i));
        }

        ShuffleList(childrenList);
        for (int i = 0; i < childCount; i++){
            childrenList[i].SetSiblingIndex(i);
        }
        for (int i = 0; i < childCount; i++){
            allPickManagers[i] = childrenList[i].GetComponent<PickManager>();
            allPickManagers[i].myIndex = i;
        }
    }

    public void ResetAll(){
        dailyContinueButtonObject.SetActive(false);
        foreach(SegmentManager newSM in allSegmentManagers){
            newSM.WipeSegments();
        }
        foreach(PickManager newPM in allPickManagers){
            newPM.WipeRing();
            newPM.gameObject.SetActive(false);
        }
        
        if(RingScalerCo != null){StopCoroutine(RingScalerCo);RingScalerCo = null;}
        ringObject.transform.localScale = ringScales[0];
        previousPickManagers.Clear();
        
    }

    public IEnumerator RingScalerCoroutine(int newScaleIndex){
        var elapsedTime = 0f;
        var loadTime = 1f;
    	while(elapsedTime < loadTime){
    		ringObject.transform.localScale = Vector3.Lerp(ringObject.transform.localScale, ringScales[newScaleIndex], (elapsedTime / loadTime));
    		elapsedTime += Time.deltaTime;
        	yield return new WaitForEndOfFrame();
    	}
    }
    public PickManager ReturnRandomPickManager(){
        var randomPM = allPickManagers[UnityEngine.Random.Range(0, allPickManagers.Count)];
        if(randomPM.inPlay == true){randomPM = ReturnRandomPickManager();}
        return randomPM;
    }

    public void SubmitButton(){
        if(!allPickManagers[chosenPickManagerInt].inPlay){return;}
        for(int x = 0; x < allPickManagers[chosenPickManagerInt].pickStatus.Count; x++){
            if(allPickManagers[chosenPickManagerInt].pickStatus[x] == 1 && allSegmentManagers[chosenSegmentManagerInt].segmentStatus[x] == 0){
                return;
            }
        }
        OptionsManager.PlayAudioClip(3);

        allSegmentManagers[chosenSegmentManagerInt].FillIn(allPickManagers[chosenPickManagerInt].pickStatus);
        allPickManagers[chosenPickManagerInt].FilledInSpots(allPickManagers[chosenPickManagerInt].pickStatus, chosenSegmentManagerInt);
        
        previousPickManagers.Add(chosenPickManagerInt);

        bool segmentComplete = true;
        for(int x = 0; x < allSegmentManagers[chosenSegmentManagerInt].segmentStatus.Count; x++){
            if(allSegmentManagers[chosenSegmentManagerInt].segmentStatus[x] == 1){segmentComplete = false;}
        }
        if(segmentComplete){
            chosenSegmentManagerInt += 1;
            if(chosenSegmentManagerInt == segmentMaxCount){               
                OptionsManager.DelayAudio(2, .1f); 
                allPickManagers[0].gameObject.transform.parent.gameObject.SetActive(false);
                textDirectObj.SetActive(true);
                if(!isDaily){
                    textDirectObj.GetComponent<Text>().text = "Great Job!\nSelect a difficulty to begin.";
                    IncreaseFillAmount();
                }
                if(isDaily){
                    textDirectObj.GetComponent<Text>().text = $"Great Job!\nYour time was {OptionsManager.timerText.text}.\n\nTry again tomorrow!";
                    StreakManager.SaveInput(2);
                }
                OptionsManager.EndTimer();
                InputManager.HeGoes();
                canInput = false;
                mainPickManager.HideAllPicks();

                return;
            }
            else{
                ToNextSegment();
            }
        }
        FindNextPickManager(1);
    }

    public void ToNextSegment(){
        allSegmentManagers[chosenSegmentManagerInt - 1].gameObject.SetActive(false);
        allSegmentManagers[chosenSegmentManagerInt].gameObject.SetActive(true);
        ActivateSegmentManagers(chosenSegmentManagerInt, segmentMaxCount);
        RingScalerCo = StartCoroutine(RingScalerCoroutine(chosenSegmentManagerInt));
    }

    public void ShuffleList<T>(List<T> list){
    int n = list.Count;
    for (int i = n - 1; i > 0; i--)
    {
        int j = UnityEngine.Random.Range(0, i + 1);
        T temp = list[i];
        list[i] = list[j];
        list[j] = temp;
    }
}
public void FindNextPickManager(int dirInt){
    OptionsManager.PlayAudioClip(0);
    var iteratorCount = 0;
    chosenPickManagerInt += dirInt;
    if (chosenPickManagerInt >= allPickManagers.Count){chosenPickManagerInt = 0;}
    if(chosenPickManagerInt < 0){chosenPickManagerInt = allPickManagers.Count - 1;}
    var nextPM = allPickManagers[chosenPickManagerInt];
    while (!nextPM.inPlay && iteratorCount < allPickManagers.Count){
        nextPM = CheckNextPM(dirInt);
        iteratorCount += 1;
    }

    if (iteratorCount >= allPickManagers.Count){return;}
    nextPM.Selected();
}

public PickManager CheckNextPM(int dirInt){
    chosenPickManagerInt += dirInt;
    if (chosenPickManagerInt >= allPickManagers.Count){chosenPickManagerInt = 0;}
    if (chosenPickManagerInt < 0){chosenPickManagerInt = allPickManagers.Count -1;}
    return allPickManagers[chosenPickManagerInt];
}

public void UndoButton(){
    if(previousPickManagers.Count <= 0){return;}
    if(allUndoAutoAmounts[0] <= 0){return;}
    if(!InputManager.isInteractable[0]){return;}
    OptionsManager.PlayAudioClip(4);
    var getPM = allPickManagers[previousPickManagers[previousPickManagers.Count -1]];
    allPickManagers[chosenPickManagerInt].Deselected();
    chosenPickManagerInt = getPM.myIndex;
    allSegmentManagers[getPM.segmentIndex].UndoSegnmentPokeHole(getPM.segmentSpotsFilledIn);
    getPM.UndoPickManager();
    getPM.Selected();
    SubtractUndoAutoAmount(0);
    previousPickManagers.RemoveAt(previousPickManagers.Count -1);
    if(getPM.segmentIndex != chosenSegmentManagerInt){
        chosenSegmentManagerInt = getPM.segmentIndex;
        ActivateSegmentManagers(chosenSegmentManagerInt, segmentMaxCount);
        allSegmentManagers[chosenSegmentManagerInt].gameObject.SetActive(true);
        if(RingScalerCo != null){StopCoroutine(RingScalerCo);RingScalerCo = null;}
        ringObject.transform.localScale = ringScales[chosenSegmentManagerInt];
    }
}

    public bool debugMode;
    public void DebugModeActivate(){debugMode = true;}   
    public void DebugModeDeactivate(){debugMode = false;}   

    public List<PickManager> goodChoiceList;

    public void AutoSlot(){
        if(allUndoAutoAmounts[1] <= 0){return;}
        if(!InputManager.isInteractable[1]){return;}
        var chosenPM = -1;
        var pmTurns = 0;
        goodChoiceList = new List<PickManager>();
        for(int x = 0; x < allPickManagers.Count; x++){
            if(!allPickManagers[x].inPlay){continue;}
            pmTurns = allPickManagers[x].RotateToFitPick(chosenSegmentManagerInt);
            if(pmTurns != -1){goodChoiceList.Add(allPickManagers[x]);}
        }
        if (goodChoiceList.Count > 0){
            var segmentStatusCount = allSegmentManagers[chosenSegmentManagerInt].CountStatus();
            for (int a = goodChoiceList.Count - 1; a >= 0; a--){
                var pmCount = goodChoiceList[a].CountStatus();
                if (pmCount > segmentStatusCount + 1){
                    goodChoiceList.RemoveAt(a);
                }
            }

            goodChoiceList = goodChoiceList.OrderByDescending(pm => pm.CountStatus()).ToList();

            for (int a = 0; a < goodChoiceList.Count; a++){
                if (goodChoiceList[a].segmentBelongsTo == chosenSegmentManagerInt){
                    var entryToMove = goodChoiceList[a];
                    goodChoiceList.RemoveAt(a);
                    goodChoiceList.Insert(0, entryToMove);
                    break;
                }
            }
        }
            
        if(goodChoiceList.Count > 0){
            if(goodChoiceList[0].segmentBelongsTo == chosenSegmentManagerInt){
                pmTurns = goodChoiceList[0].RotateToFitPick(chosenSegmentManagerInt, true);
                if(pmTurns != -1){
                    goodChoiceList[0].SetToInitialPickStatus();
                    // check AGAIN, in case the initial != segmentStatus BUT can fit another (normally just 1 pick)
                    var xtraPMTurns = goodChoiceList[0].RotateToFitPick(chosenSegmentManagerInt, true);
                    if(xtraPMTurns != 0){
                        for(int y = -1; y < pmTurns; y++){
                            goodChoiceList[0].MovePicks(1);
                        }
                    }
                    goodChoiceList[0].Selected();
                    SubtractUndoAutoAmount(1);
                    return;
                }
            }
            //doing this again, either this or I sort a list of the previous calculated rotations, this isn't the "right" way but Im lazy :D
            pmTurns = goodChoiceList[0].RotateToFitPick(chosenSegmentManagerInt);
            for(int y = -1; y < pmTurns; y++){
                goodChoiceList[0].MovePicks(1);
            }
            goodChoiceList[0].Selected();
            SubtractUndoAutoAmount(1);
            return;
        }
        // Debug.Log("CANNOT FIND");
    }

    public void SetUIToAnchors(int anchorIndex){
        // 0 standard, 1 mobile portrait
        var anchorIndexBase = 0;
        if(anchorIndex == 1){ anchorIndexBase += allUIRectTransforms.Count;}
        for(int x = 0; x < allUIRectTransforms.Count; x++){
            allUIRectTransforms[x].transform.SetParent(allUIAnchorRectTransforms[x + anchorIndexBase], false);
        }
    }

    public void InitialUndoAuto(){
        allUndoAutoAmounts[0] = 5;
        allUndoAutoFillAmounts[0] = 0f;

        allUndoAutoAmounts[1] = 5;
        allUndoAutoFillAmounts[1] = 0f;
        
        SetUndoAuto();
        PlayerPrefs.SetInt("intialUndoAuto", 1);
        SaveUndoAuto();
    }

    public void LoadUndoAuto(){
        allUndoAutoAmounts[0] = PlayerPrefs.GetInt("undoAmount");
        allUndoAutoAmounts[1] = PlayerPrefs.GetInt("autoAmount");

        allUndoAutoFillAmounts[0] = PlayerPrefs.GetFloat("undoFillAmount");
        allUndoAutoFillAmounts[1] = PlayerPrefs.GetFloat("autoFillAmount");

        SetUndoAuto();
    }

    public void SaveUndoAuto(){
        PlayerPrefs.SetInt("undoAmount", allUndoAutoAmounts[0]);
        PlayerPrefs.SetInt("autoAmount", allUndoAutoAmounts[1]);

        PlayerPrefs.SetFloat("undoFillAmount", allUndoAutoFillAmounts[0]);
        PlayerPrefs.SetFloat("autoFillAmount", allUndoAutoFillAmounts[1]);
    }

    public void SetUndoAuto(bool setZero = false){
        if(setZero){
            for(int x = 0; x < allUndoAutoAmounts.Count; x++){
                InputManager.SetUndoAutoButtonInfo(x, 0, 0f);
            }
            return;
        }
        for(int x = 0; x < allUndoAutoAmounts.Count; x++){
                InputManager.SetUndoAutoButtonInfo(x, allUndoAutoAmounts[x], allUndoAutoFillAmounts[x]);
            }
    }

    public void SubtractUndoAutoAmount(int undoAutoIndex){
        allUndoAutoAmounts[undoAutoIndex] -= 1;
        SetUndoAuto();
        SaveUndoAuto();
    }

    public void IncreaseFillAmount(){
        var newAmount = (1f / segmentMaxCount) + 0.1f;
        for(int x = 0; x < allUndoAutoFillAmounts.Count; x++){
            var forUndo = 0f;
            var newIntAmount = 0;
            if(x == 0){forUndo = 0.35f;}
            allUndoAutoFillAmounts[x] = allUndoAutoFillAmounts[x] + newAmount + forUndo;
            if(allUndoAutoFillAmounts[x] >= 1f){
                allUndoAutoAmounts[x] += 1;
                allUndoAutoFillAmounts[x] -= 1f;
            }
            InputManager.SetUndoAutoButtonInfo(x, allUndoAutoAmounts[x], allUndoAutoFillAmounts[x]);
        }
        SaveUndoAuto();
    }

    public void PreDaily(){
        allPickManagers[0].gameObject.transform.parent.gameObject.SetActive(false);
        textDirectObj.SetActive(true);
        textDirectObj.GetComponent<Text>().text = "Daily mode is a master level lock that can be attempted once per day.\nYou cannot use Undo or Auto, the rings will not highlight.";
        dailyContinueButtonObject.SetActive(true);
    }

    public bool isDaily;
    public void StartDaily(){
        DifficultySelection(3, StreakManager.currentDayUnixTimestamp);
        StreakManager.SaveInput(1);
        SetUndoAuto(true);
        isDaily = true;
    }

}