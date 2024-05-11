using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice : MonoBehaviour
{
    [SerializeField]private bool active;
    [SerializeField] private bool isClicked=false;
    public void PassTurn()
    {
        active = false;
        isClicked = true;
    }
    public void GetTurn()
    {
        active = true;
        isClicked = false;
    }
    private void OnMouseDown()
    {
        if (!GameManager.instance.isPlaying || !active)
            return;
        if (isClicked)
            return;
        StartCoroutine(GameManager.instance.RolldiceDelay());
        Debug.Log("Rolled");
        isClicked = true;
    }
}
