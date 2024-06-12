using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigation : MonoBehaviour
{
    List<string> Scenes = new(){"Menu", "Blackjack", "Pisti", "Poker"};

    public void LoadMenuScene()
    {
        SceneManager.LoadScene(Scenes[0]);
    }

    public void LoadBlackjackScene()
    {
        SceneManager.LoadScene(Scenes[1]);
    }

    public void LoadPistiScene()
    {
        SceneManager.LoadScene(Scenes[2]);
    }

    public void LoadPokerScene()
    {
        SceneManager.LoadScene(Scenes[3]);
    }
}
