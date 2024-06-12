using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;

public class BlackjackManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI myCardsText;

    [SerializeField]
    private TextMeshProUGUI myCardsScore;

    [SerializeField]
    private TextMeshProUGUI dealersCardsText;

    [SerializeField]
    private TextMeshProUGUI dealersCardsScore;

    private List<string> CardNames;
    private List<string> MyCards;
    private List<string> DealersCards;

    private ARTrackedImageManager _aRTrackedImageManager;
    private bool isGameOver = false;

    [SerializeField]
    GameObject gameOverScreen;

    private int lastSizeNames = 0;

    private int index = 0;
    private int addToMyCards = 1;
    private string lastCommand = "";

    private int myAceCount = 0;

    private void Awake(){
        _aRTrackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    private void Start(){
        _aRTrackedImageManager.gameObject.SetActive(false);
        if(myCardsText.text != ""){}
        PauseGame("Provide My Cards First");
        _aRTrackedImageManager.gameObject.SetActive(true);
        _aRTrackedImageManager.trackedImagesChanged += OnImagesChanged;
        CardNames = new List<string>();
        MyCards = new List<string>();
        DealersCards = new List<string>();
        myCardsText.text = "";
        myCardsScore.text = "Score: 0";
        dealersCardsText.text = "";
        dealersCardsScore.text = "Score: 0";
    }

    private void OnDestroy(){
        _aRTrackedImageManager.trackedImagesChanged -= OnImagesChanged;
    }

    private void OnImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    { 
        if (isGameOver) return;

        foreach(ARTrackedImage trackedImage in eventArgs.added){
            UpdateTrackedImage(trackedImage);
        }

        foreach(ARTrackedImage trackedImage in eventArgs.updated){
            UpdateTrackedImage(trackedImage);
        }

        foreach(ARTrackedImage trackedImage in eventArgs.removed){
            //if (CardNames.Contains(trackedImage.referenceImage.name)){
            //    CardNames.Remove(trackedImage.referenceImage.name);
            //}
        }

        if (lastSizeNames == CardNames.Count) return;

        AssignCards();
        DisplayCardStatus();
        PerformBlackjackLogic();
    }

    private void UpdateTrackedImage(ARTrackedImage trackedImage){
        if(trackedImage.trackingState is /*TrackingState.Limited or*/ TrackingState.None){
            return;
        }

        if (!CardNames.Contains(trackedImage.referenceImage.name)){
            CardNames.Add(trackedImage.referenceImage.name);
        }
    }

    private void AssignCards()
    {
        if (addToMyCards == 1 && CardNames.Count > index)
        {
            MyCards.Add(CardNames[index]);
            index++;
            if (MyCards.Count > 1){
                if (DealersCards.Count < 1)
                    addToMyCards = 0;
                else
                    addToMyCards = -1;
            }
        }
        else if (addToMyCards == 0 && CardNames.Count > index)
        {
            DealersCards.Add(CardNames[index]);
            index++;
            if (DealersCards.Count > 0){
                addToMyCards = -1;
            }
        }
    }

    private void DisplayCardStatus()
    {
        int myScore = CalculateScore(MyCards);
        int dealerScore = CalculateScore(DealersCards);
        myCardsScore.text = $"Score: {myScore}";
        dealersCardsScore.text = $"Score: {dealerScore}";
        myCardsText.text = string.Join("\n", MyCards);
        dealersCardsText.text = string.Join("\n", DealersCards);
    }

    private void PerformBlackjackLogic()
    {
        if (MyCards.Count >= 2 && DealersCards.Count >= 1)
        {
            int dealerScore = CalculateScore(DealersCards);
            int myScore = CalculateScore(MyCards);

            string result = DetermineResult(myScore, dealerScore);
            lastSizeNames = CardNames.Count;

            if (result == "Hit")
            {
                PauseGame(result);
                addToMyCards = 1;
            }
            else if (result == "Stand")
            {
                if (lastCommand != "Stand"){
                    PauseGame(result);
                }
                addToMyCards = 0;
            }
            else if (result == "Dealer")
            {
                addToMyCards = 0;
            }
            else if (result == "You Lost")
            {
                isGameOver = true;
                EndGame(result);
            }
            else if (result == "You Win")
            {
                isGameOver = true;
                EndGame(result);
            }
            else if (result == "Draw")
            {
                isGameOver = true;
                EndGame(result);
            }
            else if (result == "Unknown")
            {
                isGameOver = true;
                EndGame(result);
            }
            lastCommand = result;
            DisplayCardStatus();
        }
    }

    private int CalculateScore(List<string> cards)
    {
        int score = 0;
        int aceCount = 0;

        foreach (string card in cards)
        {
            int cardValue = GetCardValue(card);
            if (cardValue == 11) aceCount++;
            score += cardValue;
        }

        while (score > 21 && aceCount > 0)
        {
            score -= 10;
            aceCount--;
        }
        myAceCount = aceCount;
        return score;
    }

    private int GetCardValue(string card)
    {
        
        switch (card[0])
        {
            case '2': return 2;
            case '3': return 3;
            case '4': return 4;
            case '5': return 5;
            case '6': return 6;
            case '7': return 7;
            case '8': return 8;
            case '9': return 9;
            case 't': return 10;
            case 'j': return 10;
            case 'q': return 10;
            case 'k': return 10;
            case 'a': return 11;
            default: return 0;
        }
    }

    private string DetermineResult(int myScore, int dealerScore)
    {
        if (DealersCards.Count == 1){
            if (myAceCount > 0){
                if (myScore < 18) return "Hit";
                else if (myScore == 18) {
                    if (dealerScore >= 9) return "Hit";
                    else if (dealerScore < 9) return "Stand";
                }
                else if (myScore > 18) return "Stand";
            }
            else {
                if (myScore > 21) return "You Lost";
                else if (myScore >= 17) return "Stand";
                else if (myScore > 12){
                    if (dealerScore <= 6) return "Stand";
                    else if (dealerScore > 6) return "Hit";
                }
                else if (myScore <= 12) return "Hit";
            }
        }
        else{
            if (dealerScore > 21) return "You Win";
            else {
                if (myScore > dealerScore) return "Dealer";
                else if (myScore == dealerScore) return "Draw";
                else if (myScore < dealerScore) return "You Lost";
            }   
        }
        return "Unknown";
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
        SceneManager.LoadScene("Blackjack");
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
}
