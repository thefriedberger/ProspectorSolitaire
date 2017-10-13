// (Unity3D) New monobehaviour script that includes regions for common sections, and supports debugging.
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public enum ScoreEvent
{
    draw,
    mine,
    mineGold,
    gameWin,
    gameLoss
}

public class Prospector : MonoBehaviour
{
    #region GlobalVareables
    #region DefaultVareables
    public bool isDebug = false;
    private string debugScriptName = "Prospector";
    #endregion

    #region Static
    public static Prospector S;
    public static int SCORE_FROM_PREV_ROUND = 0;
    public static int HIGH_SCORE = 0;
    #endregion

    #region Public
    public Deck deck;
    public TextAsset deckXML;

    public Layout layout;
    public TextAsset layoutXML;
    public Vector3 layoutCenter;
    public float xOffset = 3;
    public float yOffset = -2.5f;
    public Transform layoutAnchor;

    public CardProspector target;
    public List<CardProspector> tableau;
    public List<CardProspector> discardPile;

    public List<CardProspector> drawPile;

    public int chain = 0;
    public int scoreRun = 0;
    public int score = 0;

    public Vector3 fsPosMid = new Vector3(.5f, .9f, 0);
    public Vector3 fsPosRun = new Vector3(.5f, .75f, 0);
    public Vector3 fsPosMid2 = new Vector3(.5f, .5f, 0);
    public Vector3 fsPosEnd = new Vector3(1f, .65f, 0);
    public FloatingScore fsRun;

    public float reloadDelay = 1f;

    public GUIText gtGameOver = null;
    public GUIText gtRoundResult = null;
    #endregion

    #region Private

    #endregion
    #endregion

    #region CustomFunction
    #region Static

    #endregion

    #region Public
    public void CardClicked(CardProspector cd)
    {
        switch(cd.state)
        {
            case CardState.target:
                break;
            case CardState.drawpile:
                MoveToDiscard(target);
                MoveToTarget(Draw());
                UpdateDrawPile();
                ScoreManager(ScoreEvent.draw);
                break;
            case CardState.tableau:
                bool validMatch = true;
                if (!cd.FaceUp) validMatch = false;
                if (!AdjacentRank(cd, target)) validMatch = false;
                if (!validMatch) return;
                tableau.Remove(cd);
                MoveToTarget(cd);
                SetTableauFaces();
                ScoreManager(ScoreEvent.mine);
                break;
        }

        CheckForGameOver();
    }

    public bool AdjacentRank(CardProspector c0, CardProspector c1)
    {
        if (!c0.FaceUp || !c1.FaceUp) return false;

        if (Mathf.Abs(c0.rank - c1.rank) == 1) return true;
        if (c0.rank == 1 && c1.rank == 13) return true;
        if (c0.rank == 13 && c1.rank == 1) return true;

        return false;
    }
    #endregion

    #region Private
    private List<CardProspector> ConvertListCardsToListCardProspector(List<Card> lCD)
    {
        List<CardProspector> lCP = new List<CardProspector>();
        CardProspector tCP;
        foreach(Card tCD in lCD)
        {
            tCP = tCD as CardProspector;
            lCP.Add(tCP);
        }
        return lCP;
    }

    private CardProspector Draw()
    {
        CardProspector cd = drawPile[0];
        drawPile.RemoveAt(0);
        return cd;
    }

    private void LayoutGame()
    {
        if(layoutAnchor == null)
        {
            GameObject tGO = new GameObject("layoutAnchor");
            layoutAnchor = tGO.transform;
            layoutAnchor.transform.position = layoutCenter;
        }

        CardProspector cp;
        foreach(SlotDef tSD in layout.slotDefs)
        {
            cp = Draw();
            cp.FaceUp = tSD.faceUp;
            cp.transform.parent = layoutAnchor;
            cp.transform.localPosition = new Vector3(layout.multiplier.x * tSD.x, layout.multiplier.y * tSD.y, -tSD.layerID);
            cp.layoutID = tSD.id;
            cp.slotDef = tSD;
            cp.state = CardState.tableau;

            cp.SetSortingLayerName(tSD.layerName);

            tableau.Add(cp);
        }

        foreach(CardProspector tCP in tableau)
        {
            foreach(int hid in tCP.slotDef.hiddenBy)
            {
                cp = FindCardByLayoutID(hid);
                tCP.hiddenBy.Add(cp);
            }
        }

        MoveToTarget(Draw());
        UpdateDrawPile();
    }

    private void MoveToDiscard(CardProspector cd)
    {
        cd.state = CardState.discard;
        discardPile.Add(cd);
        cd.transform.parent = layoutAnchor;
        cd.transform.localPosition = new Vector3(layout.multiplier.x * layout.discardPile.x, layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerID + .5f);
        cd.FaceUp = true;
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(-100 + discardPile.Count);
    }

    private void MoveToTarget(CardProspector cd)
    {
        if (target != null) MoveToDiscard(target);
        target = cd;
        cd.state = CardState.target;
        cd.transform.parent = layoutAnchor;
        cd.transform.localPosition = new Vector3(layout.multiplier.x * layout.discardPile.x, layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerID);
        cd.FaceUp = true;
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(0);
    }

    private void UpdateDrawPile()
    {
        CardProspector cd;
        for(int i = 0; i < drawPile.Count; i++)
        {
            cd = drawPile[i];
            cd.transform.parent = layoutAnchor;
            Vector2 dpStagger = layout.drawPile.stagger;
            cd.transform.localPosition = new Vector3(layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x), layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y), -layout.drawPile.layerID + .1f * i);
            cd.FaceUp = false;
            cd.state = CardState.drawpile;
            cd.SetSortingLayerName(layout.drawPile.layerName);
            cd.SetSortOrder(-10 * i);
        }
    }

    private CardProspector FindCardByLayoutID(int layoutID)
    {
        foreach(CardProspector tCP in tableau)
        {
            if (tCP.layoutID == layoutID) return tCP;
        }

        return null;
    }

    private void SetTableauFaces()
    {
        foreach(CardProspector cd in tableau)
        {
            bool fUp = true;
            foreach(CardProspector cover in cd.hiddenBy)
            {
                if (cover.state == CardState.tableau) fUp = false;
            }
            cd.FaceUp = fUp;
        }
    }

    private void CheckForGameOver()
    {
        if(tableau.Count == 0)
        {
            GameOver(true);
            return;
        }
        if (drawPile.Count > 0) return;
        foreach(CardProspector cd in tableau)
        {
            if (AdjacentRank(cd, target)) return;
        }

        GameOver(false);
    }

    private void GameOver(bool won)
    {
        if (won) ScoreManager(ScoreEvent.gameWin);
        else ScoreManager(ScoreEvent.gameLoss);
        Invoke("ReloadLevel", reloadDelay);
    }
    private void ReloadLevel()
    {
        SceneManager.LoadScene(0);
    }

    private void ScoreManager(ScoreEvent sEvt)
    {
        List<Vector3> fsPts;
        switch(sEvt)
        {
            case ScoreEvent.draw:
            case ScoreEvent.gameWin:
            case ScoreEvent.gameLoss:
                chain = 0;
                score += scoreRun;
                scoreRun = 0;
                if(fsRun != null)
                {
                    fsPts = new List<Vector3>();
                    fsPts.Add(fsPosRun);
                    fsPts.Add(fsPosMid2);
                    fsPts.Add(fsPosEnd);
                    fsRun.reportFinishTo = Scoreboard.S.gameObject;
                    fsRun.Init(fsPts, 0, 1);
                    fsRun.fontSizes = new List<float>(new float[] { 28, 36, 4 });
                    fsRun = null;
                }
                break;
            case ScoreEvent.mine:
                chain++;
                scoreRun += chain;
                FloatingScore fs;
                Vector3 p0 = Input.mousePosition;
                p0.x /= Screen.width;
                p0.y /= Screen.height;
                fsPts = new List<Vector3>();
                fsPts.Add(p0);
                fsPts.Add(fsPosMid);
                fsPts.Add(fsPosRun);
                fs = Scoreboard.S.CreateFloatingScore(chain, fsPts);
                fs.fontSizes = new List<float>(new float[] { 4, 50, 28 });
                if (fsRun == null)
                {
                    fsRun = fs;
                    fsRun.reportFinishTo = null;
                }
                else fs.reportFinishTo = fsRun.gameObject;
                break;
        }

        switch(sEvt)
        {
            case ScoreEvent.gameWin:
                gtGameOver.text = "Round Over";
                Prospector.SCORE_FROM_PREV_ROUND = score;
                PrintDebugMsg("You won this round! Round score: " + score);
                gtRoundResult.text = "You won this round!\nRound score: " + score;
                ShowResultGTs(true);
                break;
            case ScoreEvent.gameLoss:
                gtGameOver.text = "Game Over";
                if (Prospector.HIGH_SCORE <= score)
                {
                    PrintDebugMsg("You got the high score! High score: " + score);
                    string sRR = "You got the high score!\nHigh score: " + score;
                    gtRoundResult.text = sRR;
                    Prospector.HIGH_SCORE = score;
                    PlayerPrefs.SetInt("ProspectorHighScore", score);
                }
                else
                {
                    PrintDebugMsg("Your final score for the game was: " + score);
                    gtRoundResult.text = "Your final score was: " + score;
                }
                ShowResultGTs(true);
                break;
            default:
                PrintDebugMsg("Score: " + score + " | Score Run: " + scoreRun + " | Chain: " + chain);
                break;
        }
    }

    private void ShowResultGTs(bool show)
    {
        gtGameOver.gameObject.SetActive(show);
        gtRoundResult.gameObject.SetActive(show);
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

        if (PlayerPrefs.HasKey("ProspectorHighScore")) HIGH_SCORE = PlayerPrefs.GetInt("ProspectorHighScore");
        score += SCORE_FROM_PREV_ROUND;
        SCORE_FROM_PREV_ROUND = 0;

        GameObject go = GameObject.Find("GameOver");
        if (go != null) gtGameOver = go.GetComponent<GUIText>();
        go = GameObject.Find("RoundResult");
        if (go != null) gtRoundResult = go.GetComponent<GUIText>();
        ShowResultGTs(false);

        go = GameObject.Find("HighScore");
        string hScore = "High score: " + Utils.AddCommasToNumber(HIGH_SCORE);
        go.GetComponent<GUIText>().text = hScore;
    }
    // Start is called on the frame when a script is enabled just before any of the Update methods is called the first time.
    void Start()
    {
        Scoreboard.S.Score = score;

        deck = GetComponent<Deck>();
        deck.InitDeck(deckXML.text);
        Deck.Shuffle(ref deck.cards);

        layout = GetComponent<Layout>();
        layout.ReadLayout(layoutXML.text);

        drawPile = ConvertListCardsToListCardProspector(deck.cards);
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