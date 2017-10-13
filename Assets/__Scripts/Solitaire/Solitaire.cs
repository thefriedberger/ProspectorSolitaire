// (Unity3D) New monobehaviour script that includes regions for common sections, and supports debugging.
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Solitaire : MonoBehaviour
{
    #region GlobalVareables
    #region DefaultVareables
    public bool isDebug = false;
    private string debugScriptName = "Solitaire";
    #endregion

    #region Static
    public static Solitaire singleton = null;
    #endregion

    #region Public
    public TextAsset deckXML = null;
    public TextAsset layoutXML = null;
    public Vector3 layoutCenter = Vector3.zero;

    public LayoutSolitaire layout = null;
    public Transform layoutAnchor = null;

    [Header("For debug view only!")]
    public Deck deck = null;
    public List<CardSolitaire> drawPile = new List<CardSolitaire>();
    public List<CardSolitaire> discardPile = new List<CardSolitaire>();
    #endregion

    #region Private

    #endregion
    #endregion

    #region CustomFunction
    #region Static

    #endregion

    #region Public

    #endregion

    #region Private
    private List<CardSolitaire> UpgradeCardsList(List<Card> lCD)
    {
        List<CardSolitaire> lCS = new List<CardSolitaire>();
        foreach (Card tCD in lCD) lCS.Add(tCD as CardSolitaire);
        return lCS;
    }

    private void LayoutGame()
    {
        if (layoutAnchor == null)
        {
            GameObject tGO = new GameObject("LayoutAnchor");
            layoutAnchor = tGO.transform;
            layoutAnchor.transform.position = layoutCenter;
        }

        SolSlotDef currSlot = null;
        GameObject currCard = null;
        int topCard = 0;
        int currColumnAdd = 2;
        //int currSortOrder = 0;
        for (int i = 0; i < layout.slotDefs.Count; i++)
        {
            currSlot = layout.slotDefs[i];
            currCard = deck.cards[i].gameObject;

            currCard.gameObject.transform.position = new Vector3(currSlot.x, currSlot.y, 0);
            currCard.GetComponent<CardSolitaire>().SetSortOrder(i);

            if (topCard == i)
            {
                topCard += currColumnAdd;
                currColumnAdd += 1;
            }
            else currCard.GetComponent<CardSolitaire>().FaceUp = false;
        }

        for(int i = layout.slotDefs.Count; i < deck.cards.Count; i++)
        {
            currSlot = layout.drawPile;
            currCard = deck.cards[i].gameObject;

            currCard.gameObject.transform.position = new Vector3(currSlot.x, currSlot.y, 0);
            currCard.GetComponent<CardSolitaire>().FaceUp = false;
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

        if (Solitaire.singleton == null) singleton = this;
        else PrintErrorDebugMsg("Two or more \"Solitaire\" singletons detected!");
    }
    // Start is called on the frame when a script is enabled just before any of the Update methods is called the first time.
    void Start()
    {
        deck = GetComponent<Deck>();
        deck.InitDeck(deckXML.text);
        Deck.Shuffle(ref deck.cards);

        layout = GetComponent<LayoutSolitaire>();
        layout.ReadLayout(layoutXML.text);

        drawPile = UpgradeCardsList(deck.cards);

        LayoutGame();
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