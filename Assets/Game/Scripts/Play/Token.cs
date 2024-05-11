using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Token : MonoBehaviour
{
    [SerializeField] private Transform Base;
    [SerializeField] private bool isInBase;
    [SerializeField] private bool isInHome;
    [SerializeField] private GameObject selector;
    public List<Tile> finalPath = new List<Tile>();
    private Vector3 largeScale = new Vector3(1.3f, 1.3f, 1.3f);
    private Vector3 orignalScale = Vector3.one;
    [SerializeField] private int tokenIndex = -1;
    public bool isMyturn;
    public int tokenID;
    public int pathTravelledIndex = -1;
    int previusPathIndex = -1;
    private int rolledDiceNo = 0;
    private bool restrictOrignalSize;
    private GameManager manager;
    private SoundManager soundManager;
    private int defaultSortingOrder;
    private int innerPathStartIndex = 51;
    private void Awake()
    {
        InitialSetup();
    }
    private void Start()
    {
        manager = GameManager.instance;
        soundManager = SoundManager.instance;
        defaultSortingOrder = this.transform.GetChild(1).GetComponent<SpriteRenderer>().sortingOrder;
    }
    public Transform GetBase()
    {
        return Base;
    }
    public void SetRolledDiceNo(int diceNo)
    {
        rolledDiceNo = diceNo;
    }
    public int GetTokenIndex()
    {
        return tokenIndex;
    }
    public void SetTokenIndex(int index)
    {
        tokenIndex = index;
    }
    public void ActiveDeactiveSelector(bool active)
    {
        selector.SetActive(active);
        SetTokenScale(active ? largeScale : orignalScale);
        if (active)
        {
            AvoidOverlap();
            return;
        }
        Rescale();
    }
    private bool IsTokenSitOnTheLeftSideOfTheBoard()
    {
        if (manager.pathManager.GetOuterPath().IndexOf(finalPath[pathTravelledIndex]) > manager.pathManager.GetBoardLeftSideStartingIndex())
        {
            return true;
        }
        return false;
    }
    private void AvoidOverlap()
    {
        if (pathTravelledIndex > 0)
        {
            if (finalPath[pathTravelledIndex + 1].isOccupied)
            {
                foreach (var temp in finalPath[pathTravelledIndex + 1].occupiedTokensList)
                {
                    temp.transform.GetChild(1).GetComponent<SpriteRenderer>().sortingOrder = IsTokenSitOnTheLeftSideOfTheBoard() ? defaultSortingOrder - 1 : defaultSortingOrder + 1;
                }
            }
            if (finalPath[pathTravelledIndex - 1].isOccupied)
            {
                foreach (var temp in finalPath[pathTravelledIndex - 1].occupiedTokensList)
                {
                    temp.transform.GetChild(1).GetComponent<SpriteRenderer>().sortingOrder = IsTokenSitOnTheLeftSideOfTheBoard() ? defaultSortingOrder + 1 : defaultSortingOrder - 1;
                }
            }
        }
        else if (pathTravelledIndex == 0)
        {
            Tile secondLastTile = finalPath[(finalPath.Count - 1) - 6];
            int index = manager.pathManager.GetOuterPath().IndexOf(secondLastTile);
            Tile lastTile = manager.pathManager.GetOuterPath()[index + 1];
            if (lastTile)
            {
                if (lastTile.isOccupied)
                {
                    foreach (var temp in lastTile.occupiedTokensList)
                    {
                        temp.transform.GetChild(1).GetComponent<SpriteRenderer>().sortingOrder = IsTokenSitOnTheLeftSideOfTheBoard() ? defaultSortingOrder + 1 : defaultSortingOrder - 1;
                    }
                }
            }
        }

    }

    private void Rescale()
    {
        foreach (var path in finalPath)
        {
            if (path.isOccupied)
            {
                path.ResizeToken();
            }
        }
    }

    private bool IsSelectorActive()
    {
        return selector.activeSelf;
    }
    private void OnMouseDown()
    {
        if (!isMyturn || isInHome || !IsSelectorActive())
            return;

        HideAllSelector(manager.players[manager.currentPlayerIndex]);
        StartCoroutine(Move((repeat) =>
        {
            if (manager.IsPlayerMatchCompleted(manager.players[manager.currentPlayerIndex]))
            {
                soundManager.PlayCelebrationSound();
                manager.CheckRankAndDisplayWin();
                manager.PassTurn();
            }
            else
            {
                Rescale();
                if (repeat)
                {
                    StartCoroutine(manager.GetTurn());
                }
                else
                {
                    manager.PassTurn();
                }
            }
        }));
    }
    public bool CheckTokenIsWillingToKillAnotherToken()
    {
        if(pathTravelledIndex>=innerPathStartIndex)
        {
            return true;
        }
        Token killabletoken = finalPath[pathTravelledIndex + rolledDiceNo].GetExistingToken(this);
        if(killabletoken)
        {
            return true;
        }
        return false;
    }
    public void MoveToken()
    {
        Debug.Log("Move " + tokenIndex);
        ActiveDeactiveSelector(false);
        if(!manager.IsHumanPlayer(manager.players[manager.currentPlayerIndex]))
        {
            HideAllSelector(manager.players[manager.currentPlayerIndex]);
        }
        StartCoroutine(Move((repeat) =>
        {
            if (manager.IsPlayerMatchCompleted(manager.players[manager.currentPlayerIndex]))
            {
                //this player win...
                soundManager.PlayCelebrationSound();
                manager.CheckRankAndDisplayWin();
                manager.PassTurn();
            }
            else
            {
                Rescale();
                if (repeat)
                {
                    StartCoroutine(manager.GetTurn());
                }
                else
                {
                    manager.PassTurn();
                }
            }
        }));
    }
    public void HideAllSelector(Player player)
    {
        foreach (var token in player.tokens)
        {
            token.ActiveDeactiveSelector(false);
        }
    }
    private IEnumerator Move(System.Action<bool> complete)
    {
        bool isKillingOtherToken = false;
        float speed = 5f;
        bool repeatTurn = false;
        this.transform.GetChild(1).GetComponent<SpriteRenderer>().sortingOrder = defaultSortingOrder;
        if (isInBase)
        {
            pathTravelledIndex = 0;
            previusPathIndex = pathTravelledIndex;
            transform.position = Base.transform.position;
            Vector3 endPos = finalPath[pathTravelledIndex].transform.position;
            while (MoveToNext(endPos, speed)) { yield return null; }
            isInBase = false;
            soundManager.PlayTokenBeepSound();
        }
        else
        {
            int steps = rolledDiceNo;
            previusPathIndex = pathTravelledIndex;
            RescaleTokensInaCurrentPath();
            //travel through the path on given steps...
            while (steps > 0)
            {
                transform.position = finalPath[pathTravelledIndex].transform.position;
                pathTravelledIndex++;
                Vector3 endPos = finalPath[pathTravelledIndex].transform.position;
                while (MoveToNext(endPos, speed)) { yield return null; }
                soundManager.PlayTokenBeepSound();
                yield return new WaitForSeconds(0.3f);
                steps--;
            }
        }
        //check token is reached to the home or not...
        if (pathTravelledIndex == finalPath.Count - 1)
        {
            isInHome = true;
            manager.players[manager.currentPlayerIndex].tokensInHome++;
            repeatTurn = true;
        }
        else
        {
            //check other token in the same path... if exists then remove other token place this token...
            Token opponentToken = finalPath[pathTravelledIndex].GetExistingToken(this);
            if (opponentToken != null)
            {
                repeatTurn = true;
                isKillingOtherToken = true;
                StartCoroutine(opponentToken.ReturnToBaseDelay(() =>
                {
                    isKillingOtherToken = false;
                }));
            }
            //check the if we get 6 then repeat our turn...
            if (rolledDiceNo == 6)
            {
                repeatTurn = true;
            }
        }
        //remove token from previous path...
        finalPath[previusPathIndex].RemoveTokenFromPath(this);
        //add token to the new path...
        finalPath[pathTravelledIndex].TokenArrived(this);
        if (pathTravelledIndex == finalPath.Count - 1)
        {
            yield return new WaitForSeconds(0.125f);
            soundManager.PlayTokenArrivedAtHome();
        }
        //complete the travel..
        while (isKillingOtherToken) { yield return null; }
        complete?.Invoke(repeatTurn);
    }
    private void RescaleTokensInaCurrentPath()
    {
        foreach (var t in finalPath[pathTravelledIndex].occupiedTokensList)
        {
            t.transform.localScale = largeScale;
        }
    }
    public IEnumerator ReturnToBaseDelay(System.Action complete)
    {
        float speed = 5;
        previusPathIndex = pathTravelledIndex;
        soundManager.PlayKillTokenSound();
        while (pathTravelledIndex > 0)
        {
            SetTokenScale(orignalScale);
            transform.position = finalPath[pathTravelledIndex].transform.position;
            pathTravelledIndex--;
            Vector3 endPos = finalPath[pathTravelledIndex].transform.position;
            while (MoveToNext(endPos, speed)) { yield return null; }
        }
        transform.position = finalPath[pathTravelledIndex].transform.position;
        Vector3 end = Base.position;
        while (MoveToNext(end, speed)) { yield return null; }
        finalPath[previusPathIndex].RemoveTokenFromPath(this);
        ResetToken();
        complete?.Invoke();
    }
    private void SetTokenScale(Vector3 scale)
    {
        transform.localScale = scale;
    }
    private bool MoveToNext(Vector3 nextPos, float speed)
    {
        return nextPos != (transform.position = Vector3.MoveTowards(transform.position, nextPos, speed * Time.deltaTime));
    }
    public bool IsTravelPossible()
    {
        if ((finalPath.Count - (pathTravelledIndex + 1) >= rolledDiceNo))
        {
            return true;
        }
        return false;
    }
    public void ActivatePlayer(bool active)
    {
        gameObject.SetActive(active);
        ResetToken();
    }
    public void ResetToken()
    {
        isInBase = true;
        isInHome = false;
        isMyturn = false;
        pathTravelledIndex = -1;
        previusPathIndex = -1;
        rolledDiceNo = 0;
        SetTokenScale(orignalScale);
    }
    public void DeactivateTurn()
    {
        isMyturn = false;
    }

    public void ActivateTurn()
    {
        isMyturn = true;
    }

    private void InitialSetup()
    {
        transform.position = Base.position;
        isInBase = true;
    }
    public bool IsInHome()
    {
        return isInHome;
    }
    public bool IsInBase()
    {
        return isInBase;
    }
}