using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TokenColour : MonoBehaviour
{
    public int playerID;
    private MenuManger manager;
    public UnityEngine.Events.UnityEvent selectTokenEvent;
    public GameObject checkMark;
    public GameObject inputField;
    private void Start()
    {
        manager = MenuManger.instance;
    }
    public void SelectToken(bool isSelected)
    {
        if(manager.selectedtokenList.Count>=manager.selectedPlayType)
        {
            checkMark.SetActive(!isSelected);
            inputField.SetActive(!isSelected);
            if(manager.selectedtokenList.Exists(temp => temp == this))
                selectTokenEvent.Invoke();
            return;
        }
        else {
            if (checkMark.activeSelf)
            {
                checkMark.SetActive(!isSelected);
                inputField.SetActive(!isSelected);
            }
            else {
                checkMark.SetActive(isSelected);
                inputField.SetActive(isSelected);
            }
            selectTokenEvent.Invoke();
        }
    }
   
}
