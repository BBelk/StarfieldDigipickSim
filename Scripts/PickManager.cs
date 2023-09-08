using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using Mono.Cecil.Cil;
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

    public void Selected(){
        if(used || !inPlay){return;}
        highlightCircleObj.SetActive(true);
        GameManager.NewPickChosen(myIndex);
    }

    public void Deselected(){
        if(used){return;}
        highlightCircleObj.SetActive(false);
    }

    private List<int> availableIndices;

    public void AcquirePicks(List<int> newPicks, int segmentInt){
        used = false;
        inPlay = true;
        for(int x = 0; x < pickStatus.Count; x++){
            for(int y = 0; y < newPicks.Count; y++){
                if(x == newPicks[y]){pickStatus[x] = 1;}
            }
        }
        for(int y = 0; y < UnityEngine.Random.Range(0, 32); y++){
            MovePicks(0);
        }
        UpdateRing();
        debugText.text = "";
        //shows which ring the pick should go on, because my friend thought my code was broke BUT ITS NOT DENNIE
        if(segmentInt != -1 && GameManager.debugMode){debugText.text = "" + (segmentInt + 1);}
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

}