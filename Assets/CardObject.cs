using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CardObject : MonoBehaviour {
    Card card = null;
    
    public TextMeshPro SuitText = null;
    public TextMeshPro RankText = null;

    public void SetCard(Card card) {
        this.card = card;
        if(card != null) {
            SuitText.text = "Suit: " + card.suit;
            RankText.text = "Rank: " + card.rank;
        } else {
            gameObject.SetActive(false);
        }
    }
    
    public Card GetCard() {
        return card;
    }
}
