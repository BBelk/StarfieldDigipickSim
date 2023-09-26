using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputManager : MonoBehaviour
{
    public GameManager GameManager;
    public OptionsManager OptionsManager;
    public bool keyIsHeld;
    public bool firstLInput;
    public bool firstRInput;
    public Coroutine PressCo;

    public List<GameObject> allControlPanelObjects;
    

    void Update(){
        timePassed += Time.deltaTime;
        if (!GameManager.canInput){ return;}
        float horizontalAxis = Input.GetAxisRaw("Horizontal");

        // Check for left input press
        if (horizontalAxis < 0 && !firstLInput){
            firstLInput = true;
            InputDir(0);
            if (PressCo != null) { StopCoroutine(PressCo); PressCo = null; }
            PressCo = StartCoroutine(DirectionPressIEnum(0));
        }

        // Check for left input release
        if (horizontalAxis >= 0 && firstLInput){
            firstLInput = false;
            if (PressCo != null) { StopCoroutine(PressCo); PressCo = null; }
        }

        // Check for right input press
        if (horizontalAxis > 0 && !firstRInput){
            firstRInput = true;
            InputDir(1);
            if (PressCo != null) { StopCoroutine(PressCo); PressCo = null; }
            PressCo = StartCoroutine(DirectionPressIEnum(1));
        }

        // Check for right input release
        if (horizontalAxis <= 0 && firstRInput){
            firstRInput = false;
            if (PressCo != null) { StopCoroutine(PressCo); PressCo = null; }
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E)){
            GameManager.SubmitButton();
        }
        if (Input.GetKeyDown(KeyCode.R)){
            GameManager.UndoButton();
        }
        if (Input.GetKeyDown(KeyCode.T)){
            GameManager.AutoSlot();
        }
        if (Input.GetKeyDown(KeyCode.Q)){
            // Debug.Log("Q");
            GameManager.FindNextPickManager(-1);
        }
        if (Input.GetKeyDown(KeyCode.W)){
            // Debug.Log("W");
            GameManager.FindNextPickManager(1);
        }
        // if (Input.GetKeyDown(KeyCode.P)){
        //     PlayerPrefs.DeleteAll();
        //     Debug.Log("PLAYER PREFS DELETED");
        // }
    }

    public bool buttonHeld;
    public void ScreenDirectionButtonDown(int newDir){
        buttonHeld = true;
        InputDir(newDir);
        if (PressCo != null) { StopCoroutine(PressCo); PressCo = null; }
        PressCo = StartCoroutine(DirectionPressIEnum(newDir));
    }
    
    public void ScreenDirectionButtonUp(){
        buttonHeld = false;
        if (PressCo != null) { StopCoroutine(PressCo); PressCo = null; }
    }

    public IEnumerator DirectionPressIEnum(int dir)
    {
        //dir 0 == left, 1 == right
        var delayFloat = 0.115f;
        yield return new WaitForSeconds(delayFloat);
        InputDir(dir);
        PressCo = StartCoroutine(DirectionPressIEnum(dir));
    }

    public float timePassed;
    public void InputDir(int dir){
        timePassed = 0f;
        if(!GameManager.allPickManagers[GameManager.chosenPickManagerInt].inPlay){return;}
        GameManager.OptionsManager.PlayAudioClip(1);
        GameManager.NewInput(dir);
    }
    public List<Button> allButtons;
    public List<ColorBlock> allCBs;

    public void DifficultySelection(int difficultyIndex){
        //changed this from fake-uninteractability to bold text so its more obvious you can replay the same difficulty
        GameManager.OptionsManager.PlayAudioClip(5);
        GameManager.DifficultySelection(difficultyIndex);
        foreach(Button newButt in allButtons){
            var getString = newButt.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text;
            if(getString.Contains("<b>")){
               getString = getString.Remove(0,3);
                newButt.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = getString;
                }
        }
        allButtons[difficultyIndex].transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = "<b>" + allButtons[difficultyIndex].transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text;
    }

    public void DeBoldAllDifficultyText(){
        foreach(Button newButt in allButtons){
            var getString = newButt.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text;
            if(getString.Contains("<b>")){
               getString = getString.Remove(0,3);
                newButt.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = getString;
                }
        }
    }

    public List<Color> allUndoAutoColors; //0 white 1 black 2 gray
    public List<GameObject> standardUndoAutoButtonObjects;
    public List<GameObject> mobileUndoAutoButtonObjects;
    private List<GameObject> selectedUndoAutoButtonObjects;

    public List<GameObject> selectedList;

    public List<Coroutine> allFillAmountCos = new List<Coroutine>(); 

    public void SelectUndoAutoButtonObjects(int selectedIndex){
        foreach(GameObject newObj in allControlPanelObjects){
            newObj.SetActive(false);
        }
        allControlPanelObjects[selectedIndex].SetActive(true);
        if(selectedIndex == 0){selectedUndoAutoButtonObjects = standardUndoAutoButtonObjects;}
        if(selectedIndex == 1){selectedUndoAutoButtonObjects = mobileUndoAutoButtonObjects;}
    }

    public List<bool> isInteractable;

    public void SetUndoAutoButtonInfo(int buttonIndex, int newAmount, float newFillAmount){
        selectedList = new List<GameObject>();
        EndFillAmountCo(buttonIndex);
        if(buttonIndex == 0){
            //undo
            for(int x = 0; x < selectedUndoAutoButtonObjects.Count/2; x++){
                selectedList.Add(selectedUndoAutoButtonObjects[x]);
            }
        }
        if(buttonIndex == 1){
            //auto
            for(int x = selectedUndoAutoButtonObjects.Count/2; x < selectedUndoAutoButtonObjects.Count; x++){
                selectedList.Add(selectedUndoAutoButtonObjects[x]);
            }
        }
        // if(newAmount <= 0){isInteractable[buttonIndex] = false;}
        // if(newAmount > 0){isInteractable[buttonIndex] = true;}
        // 0 button, 1 button text, 2 fill amount image, 3 cover image, 4 amount text
        if(newAmount <= 0){
            isInteractable[buttonIndex] = false;
            selectedList[0].GetComponent<Button>().interactable = false;
            selectedList[2].GetComponent<Image>().fillAmount = 0f;
            selectedList[3].GetComponent<Image>().color = allUndoAutoColors[2];
            selectedList[4].GetComponent<TMP_Text>().text = "";
        }
        if(newAmount > 0){
            isInteractable[buttonIndex] = true;
            selectedList[0].GetComponent<Button>().interactable = true;
            // selectedList[2].GetComponent<Image>().fillAmount = newFillAmount;
            // EndFillAmountCo(buttonIndex);
            allFillAmountCos[buttonIndex] = StartCoroutine(FillAmountIEnum(selectedList[2].GetComponent<Image>(), newFillAmount));
            selectedList[3].GetComponent<Image>().color = allUndoAutoColors[0];
            selectedList[4].GetComponent<TMP_Text>().text = "" + newAmount;
        }
    }

    public IEnumerator FillAmountIEnum(Image fillAmountImage, float newFillAmount){
        float elapsedTime = 0f;
        float loadTime = 0.5f;
        float startFillAmount = fillAmountImage.fillAmount;
        float targetFillAmount = newFillAmount;

        while (elapsedTime < loadTime){
            float progress = elapsedTime / loadTime;
            fillAmountImage.fillAmount = Mathf.Lerp(startFillAmount, targetFillAmount, progress);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // If the target fill amount is greater than or equal to 1, continue filling from 0
        while (targetFillAmount >= 1f){
            targetFillAmount -= 1f;
            startFillAmount = 0f;
            elapsedTime = 0f;
            loadTime = loadTime /2f;
            while (elapsedTime < loadTime){
                float progress = elapsedTime / loadTime;
                fillAmountImage.fillAmount = Mathf.Lerp(startFillAmount, targetFillAmount, progress);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }

    public void EndFillAmountCo(int buttonIndex){
        if(allFillAmountCos[buttonIndex] != null){
            StopCoroutine(allFillAmountCos[buttonIndex]);
            allFillAmountCos[buttonIndex] = null;
        }
    }

    void Start(){
        allFillAmountCos.Add(null);
        allFillAmountCos.Add(null);
    }

    //NOTHING TO SEE HERE
    //MOVE ALONG
    public CanvasGroup todd;
    public void HeComes(){todd.alpha += 0.001f;if(todd.alpha >= 0.025f){GameManager.DebugModeActivate();}}
    public void HeGoes(){todd.alpha = 0f;GameManager.DebugModeDeactivate();}

}
