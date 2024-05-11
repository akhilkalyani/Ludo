using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManger : MonoBehaviour
{
    public static MenuManger instance;
    public Toggle[] playTypes;
    public Dictionary<int, Toggle> playTypeDictionary = new Dictionary<int, Toggle>();
    public List<TokenColour> selectedtokenList = new List<TokenColour>();
    public Dictionary<int, string> playerNamesList = new Dictionary<int, string>();
    public int selectedPlayType;
    public GameObject warningText;
    public bool isPlayWithComputer;
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        int i = 2;
        foreach (var play in playTypes)
        {
            playTypeDictionary.Add(i, play);
            i++;
        }
    }
    public void RegisterComputerPlay()
    {
        isPlayWithComputer = true;
        selectedPlayType = 1;
    }
    public void RegisterPlayType()
    {
        Toggle t = System.Array.Find<Toggle>(playTypes, p => p.isOn == true);
        selectedPlayType = GetKeyFromDictionary(playTypeDictionary, t);
    }

    private int GetKeyFromDictionary(Dictionary<int,Toggle> playTypesDict,Toggle value) 
    {
        if (playTypesDict.ContainsValue(value))
        {
            foreach (var key in playTypesDict.Keys)
            {
                if(playTypesDict[key]==value)
                {
                    return key;
                }
            }
        }
        return 0;
    }
    public void AddTokenInTheList(TokenColour token)
    {
        if (selectedtokenList.Count>=0 && selectedtokenList.Count <selectedPlayType)
        {
            if (!selectedtokenList.Contains(token))
            {
                if (token.checkMark.activeSelf)
                {
                    selectedtokenList.Add(token);
                }
            }
            else
            {
                if (!token.checkMark.activeSelf)
                {
                    selectedtokenList.Remove(token);
                }
            }
        }
        else
        {
            if (selectedtokenList.Contains(token))
            {
                if (!token.checkMark.activeSelf)
                {
                    selectedtokenList.Remove(token);
                }
            }
        }
    }
    private int GetOpponentComputerPlayerID()
    {
        int ID = default;
        switch(selectedtokenList[0].playerID)
        {
            case 1:
                ID = 3;
                break;
            case 2:
                ID = 4;
                break;
            case 3:
                ID = 1;
                break;
            case 4:
                ID = 2;
                break;
        }
        return ID;
    }
    /*
        if(dictCnt>0)
        {
            player.playerName.text = playerNamesList[player.ID];
            GameManager.instance.ActivePlayesIDs.Add(player.ID);
            player.playerType = Ludo.PlayerType.CPU;
            Debug.Log("player  " + player.Name+" type "+player.playerType);
        }else
        {
            player.playerName.text = playerNamesList[player.ID];
            GameManager.instance.ActivePlayesIDs.Add(player.ID);
            player.playerType = Ludo.PlayerType.HUMAN;
        }
     */
    public void StartMatch()
    {
        int textCount = 0;
        foreach (var token in selectedtokenList)
        {
            if (!string.IsNullOrEmpty(token.inputField.GetComponent<TMPro.TMP_InputField>().text))
            {
                textCount++;
            }
        }
        if (textCount == selectedPlayType)
        {
            foreach (var token in selectedtokenList)
            {
                playerNamesList.Add(token.playerID, token.inputField.GetComponent<TMPro.TMP_InputField>().text);
            }
            if (isPlayWithComputer)
            {
                playerNamesList.Add(GetOpponentComputerPlayerID(),"Computer");
            }
            selectedtokenList.Clear();
            StartCoroutine(LoadMatch(() => {
                GameManager.instance.ActivePlayesIDs.Clear();
                foreach (var player in GameManager.instance.players)
                {
                    if (playerNamesList.ContainsKey(player.ID))
                    {
                        if (isPlayWithComputer)
                        {
                            if (playerNamesList[player.ID] == "Computer")
                            {
                                player.playerName.text = playerNamesList[player.ID];
                                GameManager.instance.ActivePlayesIDs.Add(player.ID);
                                player.playerType = Ludo.PlayerType.CPU;
                            }
                            else
                            {
                                player.playerName.text = playerNamesList[player.ID];
                                GameManager.instance.ActivePlayesIDs.Add(player.ID);
                                player.playerType = Ludo.PlayerType.HUMAN;
                            }
                        }
                        else
                        {
                            player.playerName.text = playerNamesList[player.ID];
                            GameManager.instance.ActivePlayesIDs.Add(player.ID);
                        }
                    }
                }
                GameManager.instance.StartMatch();
            }));
        }
        else
        {
            StartCoroutine(ShowWarning());
            return;
        }
    }
    private IEnumerator ShowWarning()
    {
        warningText.SetActive(true);
        yield return new WaitForSeconds(1f);
        warningText.SetActive(false);
    }
    private IEnumerator LoadMatch(System.Action complete)
    {
        AsyncOperation async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Game");
        while (!async.isDone) { yield return null; }
        complete.Invoke();
    }
}
