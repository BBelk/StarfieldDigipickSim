using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    public GameManager GameManager;
    public OptionsManager OptionsManager;
    public bool keyIsHeld;
    public bool firstLInput;
    public bool firstRInput;
    public Coroutine PressCo;

    void Update(){
        if (!GameManager.canInput){ return;}
        float horizontalAxis = Input.GetAxisRaw("Horizontal");

        // Check for left input press
        if (horizontalAxis < 0 && !firstLInput){
            firstLInput = true;
            InputDir(0);
            if (PressCo != null) { StopCoroutine(PressCo); PressCo = null; }
            PressCo = StartCoroutine(CallTestMethodAfterDelay(0));
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
            PressCo = StartCoroutine(CallTestMethodAfterDelay(1));
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
        if (Input.GetKeyDown(KeyCode.Q)){
            Debug.Log("Q");
            GameManager.FindNextPickManager(-1);
        }
        if (Input.GetKeyDown(KeyCode.W)){
            Debug.Log("W");
            GameManager.FindNextPickManager(1);
        }

    }

    public IEnumerator CallTestMethodAfterDelay(int dir)
    {
        //dir 0 == left, 1 == right
        var delayFloat = 0.125f;
        yield return new WaitForSeconds(delayFloat);
        InputDir(dir);
        PressCo = StartCoroutine(CallTestMethodAfterDelay(dir));
    }

    public void InputDir(int dir){
        GameManager.NewInput(dir);
    }
    public List<Button> allButtons;
    public List<ColorBlock> allCBs;

    public void DifficultySelection(int difficultyIndex){
        GameManager.DifficultySelection(difficultyIndex);
        foreach(Button newButt in allButtons){
            newButt.colors = allCBs[0];
        }
        allButtons[difficultyIndex].colors = allCBs[1];
    }

    //NOTHING TO SEE HERE
    //MOVE ALONG
    public CanvasGroup todd;
    public void HeComes(){todd.alpha += 0.001f;if(todd.alpha >= 0.025f){GameManager.DebugModeActivate();}}
    public void HeGoes(){todd.alpha = 0f;GameManager.DebugModeDeactivate();}

}
