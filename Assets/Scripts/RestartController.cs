using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class RestartController : MonoBehaviour
{ 
    [SerializeField]
    GameObject gameOverScreen;


    public void OnClickButton()
    {
        if (gameOverScreen.transform.GetChild(1).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text == "Continue")
        {
            ContinueGame();
        }
        else
        {
            ExitGame();
        }
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
