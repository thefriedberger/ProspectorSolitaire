﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum PlayerType {
    human,
    ai
}

[System.Serializable]
public class Player {
    public PlayerType type = PlayerType.ai;
    public int playerNum;

    public List<CardBartok> hand;

    public SlotDef handSlotDef;

    public CardBartok AddCard(CardBartok eCB) {
        if (hand == null) hand = new List<CardBartok>();

        hand.Add(eCB);
        return (eCB);
    }

    public CardBartok RemoveCard(CardBartok cb) {
        hand.Remove(cb);
        return (cb);
    }
}
