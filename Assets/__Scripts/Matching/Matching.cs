// (Unity3D) New monobehaviour script that includes regions for common sections, and supports debugging.
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameState
{
    GroupOne,
    GroupTwo,
    GameEnd
}

public enum TurnPhaseMatching
{
    PlayerOne,
    PlayerTwo
}

public class Matching : MonoBehaviour
{
    #region GlobalVareables
    #region DefaultVareables
    public bool isDebug = false;
    private string debugScriptName = "Prospector";
    #endregion

    #region Static
    public static Matching S;
    #endregion

    #region Public
    public Deck deck;
    public TextAsset deckXML;

    public MatchingLayout layout;
    public TextAsset layoutXML;
    public Vector3 layoutCenter;
    public float xOffset = 3;
    public float yOffset = -2.5f;
    public Transform layoutAnchor;
    public GameObject playerOneMatchesAnchor;
    public GameObject playerTwoMatchesAnchor;
    public float groupTwoCenterX = 22.9f;
    public float matchCheckDelay = 1.5f;
    public float reloadDelay = 3;
    public bool singlePlayer = false;
    public bool playerTwoIsAI = true;

    public List<CardMatching> cmDeck;
    public List<CardMatching> cardGroupOne;
    public List<CardMatching> cardGroupTwo;
    public List<CardMatching> playerOneMatches;
    public List<CardMatching> playerTwoMatches;

    public GameState state = GameState.GroupOne;
    public TurnPhaseMatching turnState = TurnPhaseMatching.PlayerOne;

    public CardMatching pickOne = null;
    public CardMatching pickTwo = null;
    int randOne = -1;
    int randTwo = -1;
    public bool aiChose = false;

    public int playerOneMatchAttempts = 0;
    public int playerTwoMatchAttempts = 0;
    
    public Text playerOneScoreText = null;
    public Text playerTwoScoreText = null;
    public Text victoryText = null;
    #endregion

    #region Private

    #endregion
    #endregion

    #region CustomFunction
    #region Static

    #endregion

    #region Public
    public void CardClicked(CardMatching cd)
    {
        if(state != GameState.GameEnd && cd.state != CardStateMatching.Matched && pickTwo == null)
        {
            if (pickOne == null)
            {
                pickOne = cd;
                pickOne.FaceUp = true;
            }
            else if (pickOne != cd && CardMatch(pickOne, cd))
            {
                pickTwo = cd;
                pickTwo.FaceUp = true;
                Invoke("CardMatchSuccessful", matchCheckDelay);
            }
            else if (pickOne != cd)
            {
                pickTwo = cd;
                pickTwo.FaceUp = true;
                Invoke("CardMatchUnsuccessful", matchCheckDelay);
            }
        }
    }

    public bool CardMatch(CardMatching c0, CardMatching c1)
    {
        if (c0 == c1) return false;
        if (c0.rank == c1.rank) return true;
        //if (c0.suit == c1.suit) return true;

        return false;
    }
    #endregion

    #region Private
    private List<CardMatching> ConvertListCardsToListCardMatching(List<Card> lCD)
    {
        List<CardMatching> lCP = new List<CardMatching>();
        CardMatching tCP;
        foreach(Card tCD in lCD)
        {
            tCP = tCD as CardMatching;
            lCP.Add(tCP);
        }
        return lCP;
    }

    private void LayoutGame()
    {
        if(layoutAnchor == null)
        {
            GameObject tGO = new GameObject("layoutAnchor");
            layoutAnchor = tGO.transform;
            layoutAnchor.transform.position = layoutCenter;
        }

        playerOneMatchesAnchor = new GameObject("PlayerOneMatchesAnchor");
        playerOneMatchesAnchor.transform.position = new Vector2(layout.playerOneMatches.x, layout.playerOneMatches.y);
        playerOneMatchesAnchor.transform.parent = Camera.main.transform;
        playerTwoMatchesAnchor = new GameObject("PlayerTwoMatchesAnchor");
        playerTwoMatchesAnchor.transform.position = new Vector2(layout.playerTwoMatches.x, layout.playerTwoMatches.y);
        playerTwoMatchesAnchor.transform.parent = Camera.main.transform;

        for (int i = 0; i < layout.cardGroupOne.Count; i++)
        {
            cardGroupOne.Add(cmDeck[i]);
            cmDeck.Remove(cmDeck[i]);
            foreach(CardMatching tCM in cmDeck)
            {
                if(CardMatch(tCM, cardGroupOne[cardGroupOne.Count - 1]))
                {
                    cardGroupOne.Add(tCM);
                    cmDeck.Remove(tCM);
                    i++;
                    break;
                }
            }
        }
        for (int i = 0; i < layout.cardGroupTwo.Count; i++) cardGroupTwo = cmDeck;
        Shuffle(ref cardGroupOne);
        Shuffle(ref cardGroupTwo);

        for (int i = 0; i < layout.cardGroupOne.Count; i++)
        {
            cardGroupOne[i].transform.position = new Vector2(layout.cardGroupOne[i].x, layout.cardGroupOne[i].y);
            cardGroupOne[i].FaceUp = false;
        }
        for (int i = 0; i < layout.cardGroupTwo.Count; i++)
        {
            cardGroupTwo[i].transform.position = new Vector2(layout.cardGroupTwo[i].x, layout.cardGroupTwo[i].y);
            cardGroupTwo[i].FaceUp = false;
        }
    }

    private void MoveToMatches(CardMatching cd)
    {
        cd.transform.parent = layoutAnchor;

        if (cd.state != CardStateMatching.Matched)
        {
            if (turnState == TurnPhaseMatching.PlayerOne)
            {
                playerOneMatches.Add(cd);
                cd.transform.localPosition = new Vector3(layout.playerOneMatches.x, layout.playerOneMatches.y, -layout.playerOneMatches.layerID + .5f);
                cd.SetSortingLayerName(layout.playerOneMatches.layerName);
                cd.SetSortOrder(-100 + playerOneMatches.Count);
            }
            else
            {
                playerTwoMatches.Add(cd);
                cd.transform.localPosition = new Vector3(layout.playerTwoMatches.x, layout.playerTwoMatches.y, -layout.playerTwoMatches.layerID + .5f);
                cd.SetSortingLayerName(layout.playerTwoMatches.layerName);
                cd.SetSortOrder(-100 + playerTwoMatches.Count);
            }

            if (cd.state == CardStateMatching.GroupOne) cardGroupOne.Remove(cd);
            else cardGroupTwo.Remove(cd);

            cd.state = CardStateMatching.Matched;
        }
    }

    private void ReloadLevel()
    {
        SceneManager.LoadScene("matchingGame");
    }

    private void Shuffle(ref List<CardMatching> oCards)
    {
        List<CardMatching> tCards = new List<CardMatching>();

        int ndx;
        tCards = new List<CardMatching>();
        while (oCards.Count > 0)
        {
            ndx = Random.Range(0, oCards.Count);
            tCards.Add(oCards[ndx]);
            oCards.RemoveAt(ndx);
        }
        oCards = tCards;
    }

    private void CardMatchSuccessful()
    {
        PrintDebugMsg("Match successful!");
        if (turnState == TurnPhaseMatching.PlayerOne) playerOneMatchAttempts++;
        else playerTwoMatchAttempts++;

        if (turnState == TurnPhaseMatching.PlayerOne)
        {
            pickOne.transform.position = playerOneMatchesAnchor.transform.position;
            pickTwo.transform.position = playerOneMatchesAnchor.transform.position;
            pickOne.transform.parent = playerOneMatchesAnchor.transform;
            pickTwo.transform.parent = playerOneMatchesAnchor.transform;
        }
        else
        {
            pickOne.transform.position = playerTwoMatchesAnchor.transform.position;
            pickTwo.transform.position = playerTwoMatchesAnchor.transform.position;
            pickOne.transform.parent = playerTwoMatchesAnchor.transform;
            pickTwo.transform.parent = playerTwoMatchesAnchor.transform;
        }

        switch (state)
        {
            case GameState.GroupOne:
                cardGroupOne.Remove(pickOne);
                cardGroupOne.Remove(pickTwo);

                if (turnState == TurnPhaseMatching.PlayerOne)
                {
                    playerOneMatches.Add(pickOne);
                    playerOneMatches.Add(pickTwo);
                }
                else
                {
                    playerTwoMatches.Add(pickOne);
                    playerTwoMatches.Add(pickTwo);
                }
                break;
            case GameState.GroupTwo:
                cardGroupTwo.Remove(pickOne);
                cardGroupTwo.Remove(pickTwo);
                if (turnState == TurnPhaseMatching.PlayerOne)
                {
                    playerOneMatches.Add(pickOne);
                    playerOneMatches.Add(pickTwo);
                }
                else
                {
                    playerTwoMatches.Add(pickOne);
                    playerTwoMatches.Add(pickTwo);
                }
                break;
        }

        pickOne.state = CardStateMatching.Matched;
        pickTwo.state = CardStateMatching.Matched;
        pickOne = null;
        pickTwo = null;
        
        UpdateUI();
        CheckWin();
        if (playerTwoIsAI) PlayAI();
    }
    private void CardMatchUnsuccessful()
    {
        PrintDebugMsg("Match unsuccessful!");
        if(turnState == TurnPhaseMatching.PlayerOne) playerOneMatchAttempts++;
        else playerTwoMatchAttempts++;

        pickOne.FaceUp = false;
        pickTwo.FaceUp = false;

        pickOne = null;
        pickTwo = null;

        UpdateUI();
        if (turnState == TurnPhaseMatching.PlayerOne && !singlePlayer)
        {
            turnState = TurnPhaseMatching.PlayerTwo;
            if (playerTwoIsAI) PlayAI();
        }
        else turnState = TurnPhaseMatching.PlayerOne;
    }

    private void CheckWin()
    {
        switch(state)
        {
            case GameState.GroupOne:
                if (cardGroupOne.Count == 0) SwitchToGroupTwo();
                break;
            case GameState.GroupTwo:
                if (cardGroupTwo.Count == 0) Victory();
                break;
        }
    }

    private void SwitchToGroupTwo()
    {
        PrintDebugMsg("Switching to group two...");
        state = GameState.GroupTwo;
        Camera.main.transform.position = new Vector3(groupTwoCenterX, Camera.main.transform.position.y, Camera.main.transform.position.z);
    }
    private void Victory()
    {
        state = GameState.GameEnd;

        if (playerOneMatches.Count > playerTwoMatches.Count)
        {
            PrintDebugMsg("Player one won!");
            victoryText.text = "Player one won!";
        }
        else if (playerOneMatches.Count < playerTwoMatches.Count)
        {
            PrintDebugMsg("Player Two won!");
            victoryText.text = "Player two won!";
        }
        else
        {
            PrintDebugMsg("Draw!");
            victoryText.text = "Draw!";
        }

        victoryText.gameObject.SetActive(true);
        Invoke("ReloadLevel", reloadDelay);
    }

    private void UpdateUI()
    {;
        playerOneScoreText.text = playerOneMatches.Count / 2 + " / " + playerOneMatchAttempts;
        if(!singlePlayer) playerTwoScoreText.text = playerTwoMatches.Count / 2 + " / " + playerTwoMatchAttempts;
        else playerTwoScoreText.text = "";
    }

    private void PlayAI()
    {
        if (playerTwoIsAI && turnState == TurnPhaseMatching.PlayerTwo)
        {
            if (state == GameState.GroupOne)
            {
                randOne = Random.Range(0, cardGroupOne.Count);
                randTwo = Random.Range(0, cardGroupOne.Count);
                while (randTwo == randOne) randTwo = Random.Range(0, cardGroupOne.Count);
                CardClicked(cardGroupOne[randOne]);
                CardClicked(cardGroupOne[randTwo]);
            }
            else if(state == GameState.GroupTwo)
            {
                randOne = Random.Range(0, cardGroupTwo.Count);
                randTwo = Random.Range(0, cardGroupTwo.Count);
                while (randTwo == randOne) randTwo = Random.Range(0, cardGroupTwo.Count);
                CardClicked(cardGroupTwo[randOne]);
                CardClicked(cardGroupTwo[randTwo]);
            }

            PrintDebugMsg("AI chose: " + randOne + " and " + randTwo);
            randOne = -1;
            randTwo = -1;
        }
    }
    #endregion

    #region Debug
    private void PrintDebugMsg(string msg)
    {
        if (isDebug) Debug.Log(debugScriptName + "(" + this.gameObject.name + "): " + msg);
    }
    private void PrintWarningDebugMsg(string msg)
    {
        Debug.LogWarning(debugScriptName + "(" + this.gameObject.name + "): " + msg);
    }
    private void PrintErrorDebugMsg(string msg)
    {
        Debug.LogError(debugScriptName + "(" + this.gameObject.name + "): " + msg);
    }
    #endregion

    #region Getters_Setters

    #endregion
    #endregion

    #region UnityFunctions

    #endregion

    #region Start_Update
    // Awake is called when the script instance is being loaded.
    void Awake()
    {
        PrintDebugMsg("Loaded.");

        S = this;
    }
    // Start is called on the frame when a script is enabled just before any of the Update methods is called the first time.
    void Start()
    {
        deck = GetComponent<Deck>();
        deck.InitDeck(deckXML.text);
        Deck.Shuffle(ref deck.cards);

        layout = GetComponent<MatchingLayout>();
        layout.ReadLayout(layoutXML.text);

        cmDeck = ConvertListCardsToListCardMatching(deck.cards);
        LayoutGame();

        UpdateUI();
    }
    // This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    void FixedUpdate()
    {

    }
    // Update is called every frame, if the MonoBehaviour is enabled.
    void Update()
    {
        
    }
    // LateUpdate is called every frame after all other update functions, if the Behaviour is enabled.
    void LateUpdate()
    {

    }
    #endregion
}