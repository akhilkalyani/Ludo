using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class GameManager : MonoBehaviour
{
    public Player[] players;
    [SerializeField] Sprite[] diceSprites;
    public static GameManager instance;
    [HideInInspector]public int currentPlayerIndex = 0;
    [HideInInspector]public List<int> ActivePlayesIDs = new List<int>();
    public bool isPlaying;
    public PathManager pathManager;
    public int no_of_times_six = 0;
    public int rolledNo = 0;
    public int rank = 0;
    [SerializeField]private Sprite idleDiceSprite;
    private int lastTurnIndex = 0;
    // Start is called before the first frame update
    private void Awake()
    {
        instance = this;
        pathManager = GetComponent<PathManager>();
    }
    public void StartMatch()
    {
        pathManager.CreatePath();
        ActivePlayesIDs.Sort();
        SetTurnInfo();
    }

    private void SetTurnInfo()
    {
        //set players here...
        foreach (var player in players)
        {
            if (ActivePlayesIDs.Exists(id => id == player.ID))
            {
                int i = 0;
                foreach (var token in player.tokens)
                {
                    token.tokenID = player.ID;
                    token.SetTokenIndex(i);
                    token.ActivatePlayer(true);
                    i++;
                }
                player.isActivated = true;
            }
        }
        //set player turn here...
        currentPlayerIndex = ActivePlayesIDs[0]-1;
        SoundManager.instance.PlayBackGroundMusic();
        StartCoroutine(GetTurn());
    }
    public IEnumerator GetTurn()
    {
        yield return new WaitForSeconds(1.5f);
        players[lastTurnIndex].dice.GetComponent<SpriteRenderer>().sprite = idleDiceSprite;
        Debug.Log("Current Player:" + players[currentPlayerIndex].Name);
        players[currentPlayerIndex].turnIndicator.SetActive(true);
        isPlaying = true;
        if(IsHumanPlayer(players[currentPlayerIndex]))
        {
            players[currentPlayerIndex].dice.GetTurn();
        }else{
            StartCoroutine(RolldiceDelay());
        }
    }
    public bool IsHumanPlayer(Player player)
    {
        if(player.playerType==Ludo.PlayerType.HUMAN)
        {
            return true;
        }
        return false;
    }
    public IEnumerator RolldiceDelay()
    {
        if(!IsHumanPlayer(players[currentPlayerIndex]))
        {
            players[currentPlayerIndex].turnIndicator.SetActive(true);
            yield return new WaitForSeconds(1f);
        }
        //play dice rolling animation..
        rolledNo = UnityEngine.Random.Range(1, 7);  
        players[currentPlayerIndex].turnIndicator.SetActive(false);
        yield return new WaitForSeconds(1.5f);
        RollDice();
    }
    public bool IsPlayerMatchCompleted(Player player)
    {
        player.isMatchCompleted = player.tokensInHome == 4 ? true : false;
        return player.isMatchCompleted;
    }
    public void CheckRankAndDisplayWin()
    {
        foreach (var player in players)
        {
            if(player.isMatchCompleted)
            {
                if (player.ID == players[currentPlayerIndex].ID)
                {
                    rank++;
                    player.playerWinText.text = player.playerName.text + " win Rank-" + rank;
                }
            }   
        }
    }
    public void PassTurn()
    {
        lastTurnIndex = currentPlayerIndex;
        no_of_times_six = 0;
        bool ishuman = IsHumanPlayer(players[currentPlayerIndex]);
        foreach (var _token in players[currentPlayerIndex].tokens)
        {
            _token.ActiveDeactiveSelector(false);
            if (ishuman)
                _token.DeactivateTurn();
        }
        currentPlayerIndex++;
        currentPlayerIndex %= players.Length;
        if (players[currentPlayerIndex].isMatchCompleted || !players[currentPlayerIndex].isActivated)
        {
            PassTurn();
        } else
        {
            int activePlayerCount = 0;
            foreach (var player in players)
            {
                if (!player.isMatchCompleted && player.isActivated)
                    activePlayerCount++;
            }
            if (activePlayerCount >= 2)
            {
                StartCoroutine(GetTurn());
            }
            else
            {
                Debug.Log("Match completed...!");
            }
        }
        GameManager.instance.isPlaying = false;
    }
    private void RollDice()
    {
        Debug.Log("rolledNo:-" + rolledNo);
        players[currentPlayerIndex].dice.GetComponent<SpriteRenderer>().sprite = diceSprites[rolledNo - 1];
        players[currentPlayerIndex].dice.PassTurn();
        bool ishuman = IsHumanPlayer(players[currentPlayerIndex]);
        foreach (var temp in players[currentPlayerIndex].tokens)
        {
            temp.SetRolledDiceNo(rolledNo);
        }
        if (rolledNo == 6)
        {
            no_of_times_six++;
            if (no_of_times_six > 2)
            {
                no_of_times_six = 0;
                PassTurn();
                return;
            }
            //check available tokens..
            if (ishuman)
            {
                if (!CheckBaseTokens())
                {
                    if (CheckActiveTokens() == 0)
                    {
                        PassTurn();
                        return;
                    }
                    else if (CheckActiveTokens() == 1)
                    {
                        Token t = GetSingleToken();
                        StartCoroutine(MoveSingleToken(t));
                    }
                }
                foreach (var _token in players[currentPlayerIndex].tokens)
                {
                    _token.ActivateTurn();
                }
            }
            else {
                //check Base tokens.. if basetoken available then pick one randomly for moving to startpos...
                List<Token> baseTokens = new List<Token>(GetBaseTokensForCPU(players[currentPlayerIndex]));
                if (baseTokens.Count > 0)
                {
                    int randomIndex = UnityEngine.Random.Range(0, baseTokens.Count);
                    Token t = baseTokens[randomIndex];
                    StartCoroutine(MoveSingleToken(t));
                }
                else{
                    if(CheckActiveTokens()==0)
                    {
                        PassTurn();
                        return;
                    }else if(CheckActiveTokens() == 1)
                    {
                        Token t = GetSingleToken();
                        StartCoroutine(MoveSingleToken(t));
                    }else if(CheckActiveTokens()>1)
                    {
                        MoveActiveTokenForCPU();
                    }
                }
            }
        }
        else
        {
            no_of_times_six = 0;
            if(ishuman)
            {
                if (CheckActiveTokens() == 0)
                {
                    PassTurn();
                    return;
                } else if (CheckActiveTokens() == 1)
                {
                    Token t = GetSingleToken();
                    StartCoroutine(MoveSingleToken(t));
                }
                foreach (var _token in players[currentPlayerIndex].tokens)
                {
                    _token.ActivateTurn();
                }
            }else {
                if (CheckActiveTokens() == 0)
                {
                    PassTurn();
                    return;
                }
                else if (CheckActiveTokens() == 1)
                {
                    Token t = GetSingleToken();
                    StartCoroutine(MoveSingleToken(t));
                }
                else if(CheckActiveTokens()>1)
                {
                    MoveActiveTokenForCPU();
                }
            }
        }
    }

    private void MoveActiveTokenForCPU()
    {
        List<Token> activeTokens = new List<Token>(GetActiveTokenList(players[currentPlayerIndex]));
        Token movableTokken = null;
        foreach (var activeToken in activeTokens)
        {
            if(activeToken.CheckTokenIsWillingToKillAnotherToken())
            {
                movableTokken = activeToken;
                break;
            }
        }
        if(movableTokken==null)
        {
            int randomIndex = UnityEngine.Random.Range(0, activeTokens.Count);
            movableTokken = activeTokens[randomIndex];
        }
        StartCoroutine(MoveSingleToken(movableTokken));
        activeTokens.Clear();
    }

    private List<Token> GetActiveTokenList(Player player)
    {
        List<Token> tempActiveTokenList = new List<Token>();
        tempActiveTokenList.Clear();
        foreach (var token in player.tokens)
        {
            if(!token.IsInBase() && !token.IsInHome())
            {
                if(token.IsTravelPossible())
                {
                    token.ActiveDeactiveSelector(true);
                    tempActiveTokenList.Add(token);
                }
            }
        }
        return tempActiveTokenList;
    }

    private IEnumerator MoveSingleToken(Token t)
    {
        yield return new WaitForSeconds(0.25f);
        t.MoveToken();
    }

    private Token GetSingleToken()
    {
        Token temp = null;
        foreach (var token in players[currentPlayerIndex].tokens)
        {
            if (!token.IsInBase() && !token.IsInHome())
            {
                if (token.IsTravelPossible())
                {
                    temp = token;
                    temp.ActiveDeactiveSelector(true);
                    break;
                }
            }
        }
        return temp;
    }
    private List<Token> GetBaseTokensForCPU(Player player)
    {
        List<Token> baseList = new List<Token>();
        baseList.Clear();
        foreach (var token in player.tokens)
        {
            if(token.IsInBase() && !token.IsInHome())
            {
                token.ActiveDeactiveSelector(true);
                baseList.Add(token);
            }
        }
        return baseList;
    }
    private bool CheckBaseTokens()
    {
        bool hasBaseToken = false;
        foreach (var token in players[currentPlayerIndex].tokens)
        {
            if (token.IsInBase() && !token.IsInHome())
            {
                token.ActiveDeactiveSelector(true);
                hasBaseToken = true;
            }
            if (!token.IsInBase() && !token.IsInHome())
            {
                if (token.IsTravelPossible())
                {
                    token.ActiveDeactiveSelector(true);
                }
            }
        }
        return hasBaseToken;
    }
    private int CheckActiveTokens()
    {
        int activeTokens = 0;
        foreach (var token in players[currentPlayerIndex].tokens)
        {
            if (!token.IsInBase() && !token.IsInHome())
            {
                if (token.IsTravelPossible())
                {
                    token.ActiveDeactiveSelector(true);
                    activeTokens++;
                }
            }
        }
        return activeTokens;
    }
    // Update is called once per frame
    void Update()
    {

    }
}
    [System.Serializable]
    public class Player
    {
        public string Name;
        public TMP_Text playerName;
        public TMP_Text playerWinText;
        public int ID;
        public Ludo.PlayerType playerType;
        public Token[] tokens;
        public Dice dice;
        public GameObject turnIndicator;
        public bool isMatchCompleted;
        public bool isActivated;
        public int tokensInHome = 0;
    }
