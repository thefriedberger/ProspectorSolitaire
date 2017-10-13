// (Unity3D) New monobehaviour script that includes regions for common sections, and supports debugging.
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CardStateMatching
{
    GroupOne,
    GroupTwo,
    Matched,
}

public class CardMatching : Card
{
    #region GlobalVareables
    #region Static

    #endregion

    #region Public
    public CardStateMatching state = CardStateMatching.GroupOne;
    public List<CardMatching> hiddenBy = new List<CardMatching>();
    public int layoutID;
    public SlotDef slotDef;
    #endregion

    #region Private

    #endregion
    #endregion

    #region CustomFunction
    #region Static

    #endregion

    #region Public
    public override void OnMouseUpAsButton()
    {
        Matching.S.CardClicked(this);
        base.OnMouseUpAsButton();
    }
    #endregion

    #region Private

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