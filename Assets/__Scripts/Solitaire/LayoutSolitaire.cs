// (Unity3D) New monobehaviour script that includes regions for common sections, and supports debugging.
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SolSlotDef
{
    public float x = 0;
    public float y = 0;
    public bool faceUp = false;
    public string layerName = "Default";
    public int layerID = 0;
    public int id = 0;
    public int hiddenBy = 0;
    public string type = "slot";
    public Vector2 stagger = Vector2.zero;
    public Vector3 pos = Vector3.zero;
}

public class LayoutSolitaire : MonoBehaviour
{
    #region GlobalVareables
    #region DefaultVareables
    public bool isDebug = false;
    private string debugScriptName = "LayoutSolitaire";
    #endregion

    #region Static

    #endregion

    #region Public
    public PT_XMLReader xmlr = null;
    public PT_XMLHashtable xml = null;
    public Vector2 multiplier = Vector2.zero;

    public List<SolSlotDef> slotDefs = new List<SolSlotDef>();
    public SolSlotDef drawPile = null;
    public SolSlotDef discardPile = null;
    public List<SolSlotDef> aces = new List<SolSlotDef>();
    #endregion

    #region Private

    #endregion
    #endregion

    #region CustomFunction
    #region Static

    #endregion

    #region Public
    public void ReadLayout(string xmlText)
    {
        xmlr = new PT_XMLReader();
        xmlr.Parse(xmlText);
        xml = xmlr.xml["xml"][0];

        multiplier.x = float.Parse(xml["multiplier"][0].att("x"));
        multiplier.y = float.Parse(xml["multiplier"][0].att("y"));

        SolSlotDef tSD;
        PT_XMLHashList slotsX = xml["slot"];

        for(int i = 0; i < slotsX.Count; i++)
        {
            tSD = new SolSlotDef();
            if (slotsX[i].HasAtt("type")) tSD.type = slotsX[i].att("type");
            else tSD.type = "slot";

            tSD.x = float.Parse(slotsX[i].att("x"));
            tSD.y = float.Parse(slotsX[i].att("y"));
            tSD.pos = new Vector3(tSD.x * multiplier.x, tSD.y * multiplier.y, 0);

            tSD.layerID = int.Parse(slotsX[i].att("layer"));
            tSD.layerName = tSD.layerID.ToString();

            switch(tSD.type)
            {
                case "slot":
                    tSD.faceUp = slotsX[i].att("faceup") == "1";
                    tSD.id = int.Parse(slotsX[i].att("id"));
                    if (slotsX[i].HasAtt("hiddenby")) tSD.hiddenBy = int.Parse(slotsX[i].att("hiddenby"));
                    slotDefs.Add(tSD);
                    break;
                case "drawpile":
                    tSD.stagger.x = float.Parse(slotsX[i].att("xstagger"));
                    drawPile = tSD;
                    break;
                case "discardpile":
                    discardPile = tSD;
                    break;
                case "aces":
                    aces.Add(tSD);
                    break;
            }
        }
    }
    #endregion

    #region Private

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
    }
    // Start is called on the frame when a script is enabled just before any of the Update methods is called the first time.
    void Start()
    {

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