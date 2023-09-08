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

    public GameObject testObject;


    void Update(){
        if(!GameManager.canInput){return;}
        //mousewheel
        bool leftPress = Input.GetAxis("Horizontal") < 0;
        if ((leftPress && !firstLInput)){
            firstLInput = true;
            InputDir(0);
            if(PressCo != null){StopCoroutine(PressCo);PressCo = null;}
            PressCo = StartCoroutine(CallTestMethodAfterDelay(0));
        }

        if ((!leftPress && firstLInput)){
            firstLInput = false;
            if(PressCo != null){StopCoroutine(PressCo);PressCo = null;}
        }


        bool rightPress = Input.GetAxis("Horizontal") > 0;
        if ((rightPress && !firstRInput)){
            firstRInput = true;
            // TestMethod("RIGHT KEY DOWN");
            InputDir(1);
            if(PressCo != null){StopCoroutine(PressCo);PressCo = null;}
            PressCo = StartCoroutine(CallTestMethodAfterDelay(1));
        }

        if ((!rightPress && firstRInput))
        {
            // keyIsHeld = false; // Reset the state when the key is released
            firstRInput = false;
            if(PressCo != null){StopCoroutine(PressCo);PressCo = null;}
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
        var delayFloat = 0.2f;
        bool trueBool = true;
        yield return new WaitForSeconds(delayFloat); // Wait for the initial delay
        InputDir(dir);
        PressCo = StartCoroutine(CallTestMethodAfterDelay(dir));


        // while (trueBool)
        // {
        //     TestMethod("REPEAT"); // Call the method repeatedly
        //     yield return new WaitForSeconds(delayFloat); // Wait for 0.5 seconds between calls
        // }
    }

    public void InputDir(int dir){
        GameManager.NewInput(dir);
    }

    //NOTHING TO SEE HERE
    //MOVE ALONG
    public CanvasGroup todd;
    public void HeComes(){todd.alpha += 0.001f;if(todd.alpha >= 0.025f){GameManager.DebugModeActivate();}}
    public void HeGoes(){todd.alpha = 0f;GameManager.DebugModeDeactivate();}

    public List<Button> allButtons;
    public List<ColorBlock> allCBs;

    public void DifficultySelection(int difficultyIndex){
        GameManager.DifficultySelection(difficultyIndex);
        foreach(Button newButt in allButtons){
            newButt.colors = allCBs[0];
        }
        allButtons[difficultyIndex].colors = allCBs[1];
    }
}
