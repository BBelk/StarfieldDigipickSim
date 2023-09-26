using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using TMPro;

public class TouchControlScript : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public InputManager InputManager;
    public float variableRadius;

    public Vector2 centerPosition;
    public bool isDragging = false;
    public float baseAngle = 0f;
    public float deltaAngle;
    public float minAngle = 11.25f;
    public TMP_InputField testInput;
    public float clickTime;
    public float clickMax = 0.1f;

    private void Start(){
    }

    public void OnPointerDown(PointerEventData eventData){
        centerPosition = this.transform.position;
        isDragging = true;
        clickTime = 0f;
        baseAngle = CalculateAngle(eventData.position);
    }

    public void OnDrag(PointerEventData eventData){
        if (isDragging){
            float currentAngle = CalculateAngle(eventData.position);
            float deltaAngle = currentAngle - baseAngle;
            
            deltaAngle = (deltaAngle + 180f) % 360f - 180f;

            int direction = 0;
            if (Mathf.Abs(deltaAngle) > minAngle){
                if (deltaAngle > 0){direction = 0;}
                else{direction = 1;}
                InputManager.InputDir(direction);
                baseAngle = currentAngle;
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData){
        isDragging = false;
        if(clickTime < clickMax){
            InputManager.GameManager.SubmitButton();
        }
    }

    public void GetNewMinAngle(){
        minAngle = float.Parse(testInput.text);
    }
    
    private float CalculateAngle(Vector2 position)
    {
        Vector2 direction = position - centerPosition;
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    void Update(){
        if(isDragging){
            clickTime += Time.deltaTime;
        }
    }
}