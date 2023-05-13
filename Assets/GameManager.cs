using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public enum Rank {
    Ace = 1,
    Two = 2, 
    Three = 3,
    Four = 4, 
    Five = 5, 
    Six = 6, 
    Seven = 7, 
    Eight = 8, 
    Nine = 9, 
    Jack = 10, 
    Queen = 11, 
    King = 12,
}

public enum Suit {
    Diamonds,
    Clubs,
    Hearts,
    Spades,
}

public class Card: IComparable, ICloneable {
    public Rank rank;
    public Suit suit;
    
    public Card(Rank rank, Suit suit) {
        this.rank = rank;
        this.suit = suit;
    }
    
    public override String ToString() {
        return "Suit: " + suit + " " + "Rank: " + rank;
    }
    
    public int GetValue() {
        return (int)rank;
    }
    
    public int CompareTo(object other) {
        if (this.GetType() != other.GetType())
            throw new ArgumentException("`other` is not a `Card`");
        
        Card otherCard = (Card)other;
        int comparison = this.suit.CompareTo(otherCard.suit);
        if(comparison == 0) 
            return this.rank.CompareTo(otherCard.rank);
        
        return comparison;
    }
    
    public object Clone() {
        return new Card(this.rank, this.suit);
    }
}

public class GameState: ICloneable {
    public const int CARDS_PER_HAND = 7;
    
    private Stack discardPile = new Stack();
    private Stack stockPile = new Stack();
    
    private List<Card> player0Hand = new List<Card>();
    private List<Card> player1Hand = new List<Card>();
    
    private int turn = 0;
    
    public static List<Card> GenerateNewDeck() {
        List<Card> deck = new List<Card>();
        
         // https://stackoverflow.com/questions/972307/how-to-loop-through-all-enum-values-in-c
        foreach (Suit suit in Enum.GetValues(typeof(Suit))) {            
            foreach (Rank rank in Enum.GetValues(typeof(Rank))) {
                deck.Add(new Card(rank, suit));
            }
        }
        
        GameState.ShuffleDeck(deck);
        
        return deck;
    }
    
    // https://stackoverflow.com/questions/273313/randomize-a-listt
    private static void ShuffleDeck(List<Card> deck) {
        int n = deck.Count;
        while (n > 1) {
            n--;  
            int k = UnityEngine.Random.Range(0, n);
            Card v = deck[k];  
            deck[k] = deck[n];  
            deck[n] = v;  
        }
    }
    
    public GameState() {        
        
    }
    
    public void GenerateNew() {
        List<Card> deck = GameState.GenerateNewDeck();
        
        for(int i = 0; i < CARDS_PER_HAND; i++) {
            {
                int index = deck.Count - 1;
                player0Hand.Add(deck[index]);
                deck.RemoveAt(index);
            }
            
            {
                int index = deck.Count - 1;
                player1Hand.Add(deck[index]);
                deck.RemoveAt(index);
            }
        }
        
        {
            int index = deck.Count - 1;
            discardPile.Push(deck[index]);
            deck.RemoveAt(index);
        }
        
        
        while(deck.Count > 0) {
            int index = deck.Count - 1;
            stockPile.Push(deck[index]);
            deck.RemoveAt(index);
        }
        
        turn = 0;
    }
    
    public Card PeekStockPile() {
        return stockPile.Count > 0 ? (Card)stockPile.Peek() : null;
    }
    
    public Card PopStockPile() {
        return (Card)stockPile.Pop();
    }
    
    public Card PeekDiscardPile() {
        return discardPile.Count > 0 ? (Card)discardPile.Peek() : null;
    }
    
    public Card PopDiscardPile() {
        return (Card)discardPile.Pop();
    }
    
    public void PushDiscardPile(Card card) {
        discardPile.Push(card);
    }
    
    public Card GetPlayerCard(int player, int index) {
        if(player == 0) {
            return player0Hand[index];
        } else if (player == 1) {
            return player1Hand[index];
        } else {
            throw new ArgumentOutOfRangeException("player not 0 or 1");
        }
    }
    
    public Card GetCurrentPlayerCard(int index) {
        return GetPlayerCard(turn, index);
    }
    
    public void DiscardPlayerCard(int player, int index) {
        Card card = null;
        if(player == 0) {
            card = player0Hand[index];
            player0Hand.RemoveAt(index);
            player0Hand.Sort();
        } else if (player == 1) {
            card = player1Hand[index];
            player1Hand.RemoveAt(index);
            player1Hand.Sort();
        } else {
            throw new ArgumentOutOfRangeException("player not 0 or 1");
        }
        
        PushDiscardPile(card);
    }
    
    public void DiscardCurrentPlayerCard(int index) {
        DiscardPlayerCard(turn, index);
    }
    
    public void AddPlayerCard(int player, Card card) {
        if(player == 0) {
            player0Hand.Add(card);
            player0Hand.Sort();
        } else if (player == 1) {
            player1Hand.Add(card);
            player1Hand.Sort();
        } else {
            throw new ArgumentOutOfRangeException("player not 0 or 1");
        }
    }
    
    public void AddCurrentPlayerCard(Card card) {
        AddPlayerCard(turn, card);
    }
    
    public int GetTurn() {
        return turn;
    }
    
    public void NextTurn() {
        turn = 1 - turn;
    }
    
    public (int, GameState) MiniMax(int depth) {
        if(depth == 0) {
            int score0 = 0;
            int score1 = 0;
            for(int i = 0; i < GameState.CARDS_PER_HAND; i++) {
                score0 += -GetPlayerCard(0, i).GetValue();
                score1 += GetPlayerCard(1, i).GetValue();
            }
            
            return (score0 + score1, this);
        }
        
        List<GameState> newStates = new List<GameState>();
        
        // Draw discard
        GameState discardState = (GameState)Clone();
        Card discardCard = discardState.PopDiscardPile();
        for(int i = 0; i < GameState.CARDS_PER_HAND; i++) {
            GameState tempState = (GameState)discardState.Clone();
            
            tempState.DiscardCurrentPlayerCard(i);
            tempState.AddCurrentPlayerCard(discardCard);
            tempState.NextTurn();
            
            newStates.Add(tempState);
        }
        discardState.PushDiscardPile(discardCard);
        discardState.NextTurn();
        newStates.Add(discardState);
        
        // Draw stock
        GameState stockState = (GameState)Clone();
        Card stockCard = stockState.PopStockPile();
        for(int i = 0; i < GameState.CARDS_PER_HAND; i++) {
            GameState tempState = (GameState)stockState.Clone();
            
            tempState.DiscardCurrentPlayerCard(i);
            tempState.AddCurrentPlayerCard(stockCard);
            tempState.NextTurn();
            
            newStates.Add(tempState);
        }
        stockState.PushDiscardPile(stockCard);
        stockState.NextTurn();
        newStates.Add(stockState);
        
        int bestScore = -999999;
        int bestIndex = -1;
        int scoreMultiplier = stockState.GetTurn() == 0 ? 1 : -1;
        for(int i = 0; i < newStates.Count; i++) {
            (int currentScore, GameState currentState) = newStates[i].MiniMax(depth - 1);
            if(currentScore * scoreMultiplier >= bestScore) {
                bestScore = currentScore;
                bestIndex = i;
            }
        }
        
        return (bestScore, newStates[bestIndex]);
    }
    
    public object Clone() {
        GameState clone = new GameState();

        object[] discardPileArray = discardPile.ToArray();
        for(int i = discardPileArray.Length - 1; i >= 0; i--) {
            clone.discardPile.Push(((Card)discardPileArray[i]).Clone());
        }
        
        object[] stockPileArray = stockPile.ToArray();
        for(int i = stockPileArray.Length - 1; i >= 0; i--) {
            clone.stockPile.Push(((Card)stockPileArray[i]).Clone());
        }
        
        for(int i = 0; i < GameState.CARDS_PER_HAND; i++) {
            clone.player0Hand.Add((Card)player0Hand[i].Clone());
            clone.player1Hand.Add((Card)player1Hand[i].Clone());
        }
        
        clone.turn = turn;
        
        return clone;
    }
}

public class GameManager : MonoBehaviour {
    GameManager Instance = null;
    
    private void Awake() {
        if(Instance != null && Instance != this) {
            Destroy(this);
            return;
        }
        Instance = this;
    }
        
    GameState gameState = null;
    
    public GameObject CardObjectPrefab = null;
    public TextMeshPro ScoreText = null;
    
    bool chooseDiscard = false;
    
    int minimaxDepth = 4;
    
    // Card displays
    List<CardObject> player0CardObjects = new List<CardObject>();
    List<CardObject> player1CardObjects = new List<CardObject>();
    CardObject discardPileObject = null;
    CardObject stockPileObject = null;
    CardObject tempHandObject = null;
    
    void Start() {     
        gameState = new GameState();
        gameState.GenerateNew();
        
        float cardSpacingX = 2.5f;
        float cardOffsetX = -7.5f;
        float cardOffsetY = 3.2f;
        for(int i = 0; i < GameState.CARDS_PER_HAND; i++) {
            {
                CardObject cardObject = Instantiate(CardObjectPrefab).GetComponent<CardObject>();
                cardObject.transform.position += new Vector3((float)i * cardSpacingX + cardOffsetX, -cardOffsetY, 0.0f);
                player0CardObjects.Add(cardObject);
            }
            {
                CardObject cardObject = Instantiate(CardObjectPrefab).GetComponent<CardObject>();
                cardObject.transform.position += new Vector3((float)i * cardSpacingX + cardOffsetX, cardOffsetY, 0.0f);
                player1CardObjects.Add(cardObject);
            }
        }
        
        float pileOffsetX = 6.0f;
        
        discardPileObject = Instantiate(CardObjectPrefab).GetComponent<CardObject>();
        discardPileObject.transform.position += new Vector3(-pileOffsetX, 0.0f, 0.0f);
        
        stockPileObject = Instantiate(CardObjectPrefab).GetComponent<CardObject>();
        stockPileObject.transform.position += new Vector3(pileOffsetX, 0.0f, 0.0f);
        
        tempHandObject = Instantiate(CardObjectPrefab).GetComponent<CardObject>();
        
        chooseDiscard = false;
        RedrawBoard();
    }
    
    void RedrawBoard() {
        for(int i = 0; i < GameState.CARDS_PER_HAND; i++) {            
            player0CardObjects[i].SetCard(gameState.GetPlayerCard(0, i));
            player1CardObjects[i].SetCard(gameState.GetPlayerCard(1, i));
        }
        
        {
            Card card = gameState.PeekDiscardPile();
            bool show = !chooseDiscard && card != null;
            discardPileObject.gameObject.SetActive(show);
            if(show) {
                discardPileObject.SetCard(card);
            }
        }
        
        {
            Card card = gameState.PeekStockPile();
            bool show = !chooseDiscard && card != null;
            stockPileObject.gameObject.SetActive(show);
            if(show) {
                stockPileObject.SetCard(card);
            }
        }
        
        tempHandObject.gameObject.SetActive(chooseDiscard);
    }
    
    void SetScore(int score) {
        ScoreText.text = "Score: " + score;
    }
    
    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            RaycastHit hit;
            
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
            if (Physics.Raycast(ray, out hit)) {
                CardObject cardObject = hit.transform.gameObject.GetComponent<CardObject>();
                if(cardObject != null) {
                    Card card = cardObject.GetCard();
                    
                    if(card == gameState.PeekStockPile()) {
                        // Drawing from stock pile
                        tempHandObject.SetCard(card);
                        gameState.PopStockPile();
                        
                        chooseDiscard = true;
                        RedrawBoard();
                    } else if(card == gameState.PeekDiscardPile()) {
                        // Drawing from discard pile
                        tempHandObject.SetCard(card);
                        gameState.PopDiscardPile();
                        
                        chooseDiscard = true;
                        RedrawBoard();
                    } else if(card == tempHandObject.GetCard()) {
                        // Discard the temporary card
                        gameState.PushDiscardPile(card);
                        
                        chooseDiscard = false;
                        gameState.NextTurn();
                        
                        (int minimaxScore, GameState minimaxState) = gameState.MiniMax(minimaxDepth);
                        SetScore(minimaxScore);
                        this.gameState = minimaxState;
                        
                        RedrawBoard();
                    } else if(chooseDiscard) {
                        // Discard from hand
                        
                        int cardIndex = -1;
                        for(int i = 0; i < GameState.CARDS_PER_HAND; i++) {
                            if(card == gameState.GetCurrentPlayerCard(i)) {
                                cardIndex = i;
                                break;
                            }
                        }
                        if(cardIndex != -1) {
                            gameState.DiscardCurrentPlayerCard(cardIndex);
                            gameState.AddCurrentPlayerCard(tempHandObject.GetCard());
                            
                            chooseDiscard = false;
                            gameState.NextTurn();
                            
                            (int minimaxScore, GameState minimaxState) = gameState.MiniMax(minimaxDepth);
                            SetScore(minimaxScore);
                            this.gameState = minimaxState;
                            
                            RedrawBoard();
                        }
                    }
                }
            }
        }
    }
}
