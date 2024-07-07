using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager instance;
    [SerializeField] GameObject panel;
    [SerializeField] TextMeshProUGUI coinText;

    private int totalCoins;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }
    private void Start()
    {
        SetPanel(false);
    }
    private void Update()
    {
        SetCoinText();
    }
    public void ReloadBtn()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentScene);
        SetPanel(false);
    }

    public void SetPanel(bool isActive)
    {
        panel.SetActive(isActive);
    }


    private void SetCoinText()
    {
        coinText.text = $"Score : {totalCoins}";
    }
    public int AddCoins(int coins)
    {
        totalCoins += coins;
        return totalCoins;
    }
}
