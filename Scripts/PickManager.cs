using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PickManager : MonoBehaviour
{

    public GameManager GameManager;
    public List<GameObject> allPicks;
    public List<int> pickStatus;
    public GameObject highlightCircleObj;
    public int myIndex;
    public bool inPlay;
    public bool used;
    
    public List<int> segmentSpotsFilledIn;
    public int segmentIndex;
    public Text debugText;
    public int segmentBelongsTo;

    public List<int> initialPickStatus;
    public void Generator(){
        for(int x = 1; x < 32; x++){
            var newPick = Instantiate(allPicks[0], allPicks[0].transform.parent);
            newPick.transform.localEulerAngles = new Vector3(0f, 0f, -11.25f * x);
            allPicks[x] = newPick;
            newPick.SetActive(false);
        }
        if(highlightCircleObj != null){highlightCircleObj.SetActive(false);}
        for(int i=0;i<32;i++){pickStatus.Add(0);}
        UpdateRing();
    }

    public void MovePicks(int dir){
        if (dir == 1){ //right
            int lastValue = pickStatus[pickStatus.Count - 1];
            for (int i = pickStatus.Count - 1; i >= 1; i--){
                pickStatus[i] = pickStatus[i - 1];
            }
            pickStatus[0] = lastValue;
        }
        else if (dir == 0){ //left
            int firstValue = pickStatus[0];
            for (int i = 0; i < pickStatus.Count - 1; i++){
                pickStatus[i] = pickStatus[i + 1];
            }
            pickStatus[pickStatus.Count - 1] = firstValue;
        }
        UpdateRing();
    }

    public List<int> tempPickStatus;

    public int RotateToFitPick(int newSegmentManagerIndex, bool useInitial = false){
        tempPickStatus = new List<int>();
        if(!useInitial){tempPickStatus = new List<int>(pickStatus);}
        if(useInitial){tempPickStatus = new List<int>(initialPickStatus);}
        
        var canMatch = false;

        for (int x = 0; x < tempPickStatus.Count; x++){
            int lastValue = tempPickStatus[tempPickStatus.Count - 1];
            for (int i = tempPickStatus.Count - 1; i >= 1; i--){
                tempPickStatus[i] = tempPickStatus[i - 1];
            }
            tempPickStatus[0] = lastValue;
            canMatch = true;

            for (int y = 0; y < tempPickStatus.Count; y++){
                if (tempPickStatus[y] == 1){
                    if (tempPickStatus[y] == 1 && GameManager.allSegmentManagers[newSegmentManagerIndex].segmentStatus[y] == 0){
                        canMatch = false;
                        break;
                    }
                }
            }

            if (canMatch){
                // Debug.Log("CAN MATCH");
                return x;
                break;
            }
        }

        if (!canMatch){
            // Debug.Log("NOT A MATCH");
            return -1;
        }
        return -1;
    }

    public void SetToInitialPickStatus(){
        pickStatus = new List<int>(initialPickStatus);
        UpdateRing();
    }

    public void UpdateRing(){
        foreach(GameObject newObj in allPicks){
            newObj.SetActive(false);
        }

        for(int x = 0; x < pickStatus.Count; x++){
            if(pickStatus[x] == 1){allPicks[x].SetActive(true);}
        }
    }

    public void CopyPicks(PickManager pmToCopy){
        for(int x = 0; x < pickStatus.Count; x++){
            pickStatus[x] = pmToCopy.pickStatus[x];
        }
        UpdateRing();
    }

    public void HideAllPicks(){
        foreach(GameObject newObj in allPicks){
            newObj.SetActive(false);
        }
        if(highlightCircleObj !=null){
            highlightCircleObj.SetActive(false);
            used = true;
        }
    }

    public void FilledInSpots(List<int> newSegmentSpots, int newSegmentIndex){
        segmentSpotsFilledIn = newSegmentSpots;
        segmentIndex = newSegmentIndex;
        inPlay = false;
        HideAllPicks();
    }

    public void WipeRing(){
        foreach(GameObject newObj in allPicks){
            newObj.SetActive(false);
        }
        for(int x = 0; x < pickStatus.Count; x++){
            pickStatus[x] = 0;
        }
        inPlay = false;
        used = false;
    }

    public void ExtraSubmit(){
        if(GameManager.chosenPickManagerInt == myIndex && highlightCircleObj.activeSelf == true){GameManager.SubmitButton();}
    }

    public void Selected(){
        if(used || !inPlay){return;}
        highlightCircleObj.SetActive(true);
        GameManager.NewPickChosen(myIndex);
        
        var newSegmentManagerIndex = GameManager.chosenSegmentManagerInt;
        var segmentMaxCount = GameManager.segmentMaxCount;

        for(int x = newSegmentManagerIndex; x < segmentMaxCount; x++){
            var newSMI = x;
            var pmTurns = RotateToFitPick(newSMI);
            if(pmTurns != -1){GameManager.allSegmentManagers[newSMI].ChangeColor(0);}
            else{
                GameManager.allSegmentManagers[newSMI].ChangeColor(1);
            }
        }
    }

    public void Deselected(){
        if(used){return;}
        highlightCircleObj.SetActive(false);
    }

    public void AcquirePicks(List<int> newPicks, int segmentInt){
        used = false;
        inPlay = true;
        for(int x = 0; x < pickStatus.Count; x++){
            for(int y = 0; y < newPicks.Count; y++){
                if(x == newPicks[y]){pickStatus[x] = 1;}
            }
        }
        initialPickStatus = new List<int>(pickStatus);
        for(int y = 0; y < UnityEngine.Random.Range(0, 32); y++){
            MovePicks(0);
        }
        UpdateRing();
        debugText.text = "";
        segmentBelongsTo = segmentInt;
        //shows which ring the pick should go on, because my friend thought my code was broke BUT ITS NOT DENNIE
        if(segmentBelongsTo != -1 && GameManager.debugMode){debugText.text = "" + (segmentBelongsTo + 1);}
    }

    public void UndoPickManager(){
        used = false;
        inPlay = true;
        pickStatus = new List<int>(segmentSpotsFilledIn);
        for(int x = 0; x < segmentSpotsFilledIn.Count; x++){
            segmentSpotsFilledIn[x] = 0;
        }
        UpdateRing();
    }

    public int CountStatus(){
        var toReturn = 0;
        for(int x = 0; x < pickStatus.Count; x++){
            toReturn += pickStatus[x];
        }
        return toReturn;
    }

}