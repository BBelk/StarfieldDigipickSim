using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class SegmentManager : MonoBehaviour
{
    public GameManager GameManager;
    public List<GameObject> allSegments;
    public List<int> segmentStatus;
    public List<int> availableIndices;
    public void Generator(){
        for(int x = 1; x < 32; x++){
            var newSegment = Instantiate(allSegments[0], allSegments[0].transform.parent);
            newSegment.transform.SetSiblingIndex(allSegments[0].transform.parent.childCount - 2);
            newSegment.transform.localEulerAngles = new Vector3(0f, 0f, -11.25f * x);
            allSegments[x] = newSegment;
        }
        // List<int> segmentStatus = new List<int>(32);
        for(int i=0;i<32;i++){segmentStatus.Add(0);}
        
        
        Debug.Log("GENERATED " + segmentStatus.Count);
        // PokeHoles(Random.Range(4, 5));
        WipeSegments();
        // InvokeRepeating("TestHolePoker", 2f, 2f);
    }

    public void TestHolePoker(){
        WipeSegments();
        // PokeHoles(Random.Range(8, 12));
    }
    public void WipeSegments(){
        foreach(GameObject newObj in allSegments){
            newObj.SetActive(false);
        }
        for(int x = 0; x < segmentStatus.Count; x++){
            segmentStatus[x] = 0;
        }
    }

    public void PokeHoles(int difficultyIndex, int segmentInt){
        var holeAmount = 0;
    var pickerAmount = 2;
    // if (difficultyIndex == 0) { holeAmount = 4;}
    Debug.Log("DIFFICULTY " + difficultyIndex);
    var minMax = new Vector2Int(2, 4);
    if (difficultyIndex == 1) { minMax = new Vector2Int(2, 5);}
    if (difficultyIndex >= 2) { minMax = new Vector2Int(1, 5);}

    foreach (GameObject newObj in allSegments){
        newObj.SetActive(true);
    }

    availableIndices = GameManager.AvailableIndices();

    List<List<int>> allChosenIndexes = new List<List<int>>();
    var lastPick = 0;
    for (int picker = 0; picker < pickerAmount; picker++){
        List<int> chosenIndexes = new List<int>();
        var pickAmount = UnityEngine.Random.Range(minMax.x, minMax.y);
        //some fine tuning
        var fineTune = 0;
        if(difficultyIndex == 0 && pickAmount == 3){
            fineTune = UnityEngine.Random.Range(0, 2);
            if(fineTune == 0){pickAmount = 2;}
        }
        if(difficultyIndex == 1 && pickAmount == 4){
            fineTune = UnityEngine.Random.Range(0, 2);
            if(fineTune == 0){pickAmount = 2;}
        }
        if(difficultyIndex >= 2 && pickAmount == 1 && lastPick == 1){
            pickAmount = UnityEngine.Random.Range(2, 4);
        }

        lastPick = pickAmount;

        for (int i = 0; i < pickAmount; i++){
            int randomIndex = Random.Range(0, availableIndices.Count);
            int selectedIndex = availableIndices[randomIndex];
            chosenIndexes.Add(selectedIndex);
            segmentStatus[selectedIndex] = 1;
            availableIndices.RemoveAt(randomIndex);
        }

        GameManager.ShuffleList(chosenIndexes);
        allChosenIndexes.Add(chosenIndexes);

        for (int p = 0; p < chosenIndexes.Count; p++){
            Debug.Log($"Picker {picker + 1}, List {p + 1}: {chosenIndexes[p]}");
        }
    }
    for(int z = 0; z < allSegments.Count; z++){
        if(segmentStatus[z] == 1){allSegments[z].SetActive(false);}
    }

    for(int x = 0; x < allChosenIndexes.Count; x++){
        var getPickManager = GameManager.ReturnRandomPickManager();
        getPickManager.gameObject.SetActive(true);
        getPickManager.AcquirePicks(allChosenIndexes[x], segmentInt);
    }
    
    }
    

public void FillIn(List<int> segmentSpots){
    for(int x = 0; x < segmentSpots.Count; x++){
        if(segmentSpots[x] == 1){
        allSegments[x].gameObject.SetActive(true);
        segmentStatus[x] = 0;
        }
    }
}

public void UndoSegnmentPokeHole(List<int> segmentSpots){
    for(int x = 0; x < segmentSpots.Count; x++){
        if(segmentSpots[x] == 1){
        allSegments[x].gameObject.SetActive(false);
        segmentStatus[x] = 1;
        }
    }
}
}
