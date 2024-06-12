using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PistiManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI myCardsText;

    [SerializeField]
    private TextMeshProUGUI myPistiScore;

    [SerializeField]
    private TextMeshProUGUI tableCardsText;

    [SerializeField]
    private TextMeshProUGUI rivalPistiScore;

    private List<string> CardNames;
    private List<string> MyCards;
    private List<string> TableCards;

    private ARTrackedImageManager _aRTrackedImageManager;
    private bool isGameOver = false;

    [SerializeField]
    GameObject gameOverScreen;

    [SerializeField]
    GameObject choiceScreen;

    private int index = 0;
    private int addToMyCards = 1;
    private bool myTurn = false;
    private int myScore = 0;
    private int rivalScore = 0;
    int lastWin;
    bool allCardsRead = false;
    bool firstView = true;

    List<string> Names = new List<string>{  "2c", "2d", "2h", "2s", 
                                            "3c", "3d", "3h", "3s", 
                                            "4c", "4d", "4h", "4s", 
                                            "5c", "5d", "5h", "5s", 
                                            "6c", "6d", "6h", "6s", 
                                            "7c", "7d", "7h", "7s", 
                                            "8c", "8d", "8h", "8s", 
                                            "9c", "9d", "9h", "9s", 
                                            "ac", "ad", "ah", "as", 
                                            "jc", "jd", "jh", "js", 
                                            "kc", "kd", "kh", "ks", 
                                            "qc", "qd", "qh", "qs", 
                                            "tc", "td", "th", "ts",};
    List<string> Played;

    private void Awake()
    {
        _aRTrackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    private void Start()
    {
        PauseGame("Provide My Cards First");
        _aRTrackedImageManager.trackedImagesChanged += OnImagesChanged;
        CardNames = new List<string>();
        MyCards = new List<string>();
        TableCards = new List<string>();
        Played = new List<string>();
    }

    private void OnDestroy()
    {
        _aRTrackedImageManager.trackedImagesChanged -= OnImagesChanged;
    }

    private void OnImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        if (isGameOver) return;

        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            UpdateTrackedImage(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            UpdateTrackedImage(trackedImage);
        }

        AssignCards();
        DisplayCardStatus();
        if (allCardsRead)
            PerformPistiLogic();
    }

    private void UpdateTrackedImage(ARTrackedImage trackedImage)
    {
        if (trackedImage.trackingState is TrackingState.None)
        {
            return;
        }

        if (!CardNames.Contains(trackedImage.referenceImage.name))
        {
            CardNames.Add(trackedImage.referenceImage.name);
        }
    }

    private void AssignCards()
    {
        if (MyCards.Count == 0){
            allCardsRead = false;
            addToMyCards = 1;
        }

        if (addToMyCards == 1 && CardNames.Count > index)
        {
            if (!Played.Contains(CardNames[index]))
            {
                MyCards.Add(CardNames[index]);
                index++;
            }
            if (MyCards.Count == 4)
            {
                addToMyCards = 0;
            }
        }
        else if (firstView && addToMyCards == 0 && CardNames.Count > index && !Played.Contains(CardNames[index]))
        {
            TableCards.Add(CardNames[index]);
            index++;
            Played.Add(CardNames[index]);
        }

        if (MyCards.Count == 4)
        {
            if (firstView && TableCards.Count > 0){
                allCardsRead = true;
                firstView = false;
            }
            else if (!firstView){
                allCardsRead = true;
            }
        }
    }

    private void DisplayCardStatus()
    {
        myPistiScore.text = $"My Score: {myScore}";
        rivalPistiScore.text = $"Rival Score: {rivalScore}";
        myCardsText.text = string.Join("\n", MyCards);
        tableCardsText.text = string.Join("\n", TableCards);
    }

    private void PerformPistiLogic()
    {
        if (myTurn)
        {
            string cardToPlay = ThrowCard(MyCards);
            if (cardToPlay != null)
            {
                if (!TableCards.Contains(cardToPlay) && !Played.Contains(cardToPlay))
                {
                    MyCards.Remove(cardToPlay);
                    TableCards.Add(cardToPlay);
                    Played.Add(cardToPlay);
                }
                else return;
                CheckForPisti(true);
                myTurn = false;
                PauseGame($"Play {cardToPlay}");
            }
        }
        else
        {
            string rivalCardToPlay = CardNames[index];
            if (rivalCardToPlay != null)
            {
                if (!TableCards.Contains(rivalCardToPlay) && !Played.Contains(rivalCardToPlay))
                {
                    TableCards.Add(rivalCardToPlay);
                    Played.Add(rivalCardToPlay);
                    index++;
                }
                else return;
                CheckForPisti(false);
                myTurn = true;
            }
        }

        DisplayCardStatus();
        if (CardNames.Count == 48)
        {
            DetermineResult();
        }
    }

    private void CheckForPisti(bool isMyTurn)
    {
        if (TableCards.Count >= 2)
        {
            string lastCard = TableCards[TableCards.Count - 1];
            string secondLastCard = TableCards[TableCards.Count - 2];

            if (lastCard[0] == secondLastCard[0] || lastCard[0] == 'j')
            {
                if (TableCards.Count == 2)
                {
                    if (isMyTurn)
                    {
                        myScore += 10;
                        lastWin = 1;
                    }
                    else
                    {
                        rivalScore += 10;
                        lastWin = 0;
                    }
                }
                else{
                    if (isMyTurn)
                    {
                        myScore += CalculateScore(TableCards);
                        lastWin = 1;
                    }
                    else
                    {
                        rivalScore += CalculateScore(TableCards);
                        lastWin = 0;
                    }
                }

                // Player takes all cards
                TableCards.Clear();
                DisplayCardStatus();
            }
        }
    }

    private int CalculateScore(List<string> cards)
    {
        int score = 0;
        foreach (var card in cards)
        {
            switch (card)
            {
                case "ac":
                case "ad":
                case "ah":
                case "as":
                    score += 1; // Aces are worth 1 point
                    break;
                case "tc":
                    score += 3; // 10 of diamonds is worth 3 points
                    break;
                case "2c":
                    score += 2; // 2 of clubs is worth 2 points
                    break;
                case "jc":
                case "jd":
                case "jh":
                case "js":
                    score += 1; // Jacks are worth 1 point
                    break;
            }
        }
        return score;
    }

    private string ThrowCard(List<string> cards)
    {
        // Implement logic for which card to throw
        if (cards.Count == 0) return null;

        if (TableCards.Count > 0)
        {
            string topTableCard = TableCards[TableCards.Count - 1];
            foreach (var card in cards)
            {
                if (card[0] == topTableCard[0] || card[0] == 'j')
                {
                    return card;
                }
            }
        }
        return cards[0]; // If no matching card, throw the first card
    }

    private void DetermineResult()
    {
        isGameOver = true;
        if (lastWin == 1){
            myScore += CalculateScore(TableCards);
        }
        else{
            rivalScore += CalculateScore(TableCards);
        }

        if (myScore > rivalScore)
        {
            EndGame("You Won!");
        }
        else if (myScore < rivalScore)
        {
            EndGame("Rival Won!");
        }
        else
        {
            EndGame("It's a Draw!");
        }
    }

    public void EndGame(string message)
    {
        Time.timeScale = 0;
        gameOverScreen.SetActive(true);
        gameOverScreen.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = message;
        gameOverScreen.transform.GetChild(1).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Exit";
    }

    public void StartGame(string message)
    {
        Time.timeScale = 0;
        gameOverScreen.SetActive(true);
        gameOverScreen.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = message;
        gameOverScreen.transform.GetChild(1).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "OK";
    }

    public void PauseGame(string message)
    {
        Time.timeScale = 0;
        gameOverScreen.SetActive(true);
        gameOverScreen.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = message;
        gameOverScreen.transform.GetChild(1).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Continue";
    }

    public void OnClickButton()
    {
        if (gameOverScreen.transform.GetChild(1).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text == "Continue")
        {
            ContinueGame();
        }
        else if (gameOverScreen.transform.GetChild(1).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text == "OK")
        {
            RestartGame();
        }
        else
        {
            ExitGame();
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("Pisti");
        Time.timeScale = 1;
    }

    public void ContinueGame()
    {
        gameOverScreen.SetActive(false);
        Time.timeScale = 1;
    }

    public void ExitGame()
    {
        SceneManager.LoadScene("Menu");
        Time.timeScale = 1;
    }

    public void Choice(bool start)
    {
        myTurn = start;
        choiceScreen.SetActive(false);
        Time.timeScale = 1;
    }
}
