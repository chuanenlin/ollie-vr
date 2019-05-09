using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour, BluetoothManager.BluetoothKeyListener
{
    private const int COIN_SCORE_AMOUNT = 10;
    private int currentlySelectedButton = -1;

    public static GameManager Instance
    {
        set;
        get;
    }

    public bool IsDead
    {
        set;
        get;
    }
    public bool isGameStarted = false;
    public bool isShopping = false;
    private PlayerMotor motor;
    int collectableAmount;
    int isBlackniteSold;
    int isGhostvoicesSold;
    int isShelterUsed;
    int isBlackniteUsed;
    int isGhostvoicesUsed;
    public GameObject buyShelterButton;
    public GameObject buyBlackniteButton;
    public GameObject buyGhostvoicesButton;
    public AudioSource ShelterAudio;
    public AudioSource BlackniteAudio;
    public AudioSource GhostvoicesAudio;

    // UI and UI fields
    public Animator collectbleAnim;
    public Text scoreText, coinText, modifierText, highscoreText, collectableAmountText, hintText;
    private float score, coinScore, modifierScore;
    private int lastScore;

    // Death menu
    public Animator deathMenuAnim, shopMenuAnim;
    public Text deadScoreText, deadCoinText;

    private void Awake()
    {
        Instance = this;
        modifierScore = 1;
        motor = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMotor>();
        scoreText.text = "score: " + score.ToString("0");
        coinText.text = "crystals: " + coinScore.ToString("0");
        modifierText.text = "speed: x" + modifierScore.ToString("0.0");
        highscoreText.text = PlayerPrefs.GetInt("Highscore").ToString("0");
        collectableAmount = PlayerPrefs.GetInt("Inventory");
        if (PlayerPrefs.GetInt("IsBlackniteUsed") == 1)
        {
            BlackniteAudio.Play();
            currentlySelectedButton = 1;
            EventSystem.current.firstSelectedGameObject = buyBlackniteButton;
        }
        else if (PlayerPrefs.GetInt("IsGhostvoicesUsed") == 1)
        {
            GhostvoicesAudio.Play();
            currentlySelectedButton = 2;
            EventSystem.current.firstSelectedGameObject = buyGhostvoicesButton;
        }
        else
        {
            ShelterAudio.Play();
            currentlySelectedButton = 0;
            EventSystem.current.firstSelectedGameObject = buyShelterButton;
        }
    }

    private void Update()
    {
        // PlayerPrefs.DeleteAll();
        
        if (isGameStarted && !IsDead)
        {
            // Increase score
            score += (Time.deltaTime * modifierScore);
            if (lastScore != (int)score)
            {
                lastScore = (int)score;
                scoreText.text = "score: " + score.ToString("0");
            }
        }

        collectableAmountText.text = "You have " + collectableAmount.ToString() + " crystals";
        isBlackniteSold = PlayerPrefs.GetInt("IsBlackniteSold");
        isGhostvoicesSold = PlayerPrefs.GetInt("IsGhostvoicesSold");
        isShelterUsed = PlayerPrefs.GetInt("IsShelterUsed");
        isBlackniteUsed = PlayerPrefs.GetInt("IsBlackniteUsed");
        isGhostvoicesUsed = PlayerPrefs.GetInt("IsGhostvoicesUsed");

        // Shop checking
        if (isBlackniteSold == 0)
        {
            buyBlackniteButton.GetComponentInChildren<Text>().text = "purchase for\n10 crystals";
            if (collectableAmount < 10)
            {
                buyBlackniteButton.GetComponentInChildren<Text>().text = "need 10 crystals";
            }
        }
        if (isGhostvoicesSold == 0)
        {
            buyGhostvoicesButton.GetComponentInChildren<Text>().text = "purchase for\n10 crystals";
            if (collectableAmount < 10)
            {
                buyGhostvoicesButton.GetComponentInChildren<Text>().text = "need 10 crystals";
            }
        }
    }

    public void ExitShop()
    {

        PlayerPrefs.SetInt("Inventory", collectableAmount);
        isShopping = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
    }

    public void GetCoin()
    {
        collectbleAnim.SetTrigger("Collect");
        coinScore++;
        coinText.text = "crystals: " + coinScore.ToString("0");
        score += COIN_SCORE_AMOUNT;
        scoreText.text = "score: " + score.ToString("0");

    }

    public void UpdateModifier(float modifierAmount)
    {
        modifierScore = 1.0f + modifierAmount;
        modifierText.text = "speed: x" + modifierScore.ToString("0.0");
    }

    public void OnPlayButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
    }

    public void OnDeath()
    {
        IsDead = true;
        // FindObjectOfType<BarrierSpawner>().IsScrolling = false;
        deadScoreText.text = score.ToString("0");
        // deadCoinText.text = "     x " + coinScore.ToString("0");
        deathMenuAnim.SetTrigger("Dead");

        // Highscore
        if (score > PlayerPrefs.GetInt("Highscore"))
        {
            float s = score;
            if (s % 1 == 0)
            {
                s += 1;
            }
            PlayerPrefs.SetInt("Highscore", (int)s);
        }

        // Inventory of collectables
        PlayerPrefs.SetInt("Inventory", collectableAmount + (int)coinScore);
        deadCoinText.text = "     " + PlayerPrefs.GetInt("Inventory").ToString("0");

    }

    public void onUp()
    {
        if (!isGameStarted && !isShopping)
        {
            isGameStarted = true;
            hintText.text = "";
            motor.StartRunning();
            // FindObjectOfType<BarrierSpawner>().IsScrolling = true;
        }

        if (!isGameStarted && isShopping)
        {
            isShopping = false;
            hintText.text = "↑ = start game   |   ↓ = open shop";
            shopMenuAnim.SetTrigger("ExitShop");
        }
    }

    public void onDown()
    {
        if (!isGameStarted && !isShopping)
        {
            isShopping = true;
            hintText.text = "";
            shopMenuAnim.SetTrigger("Shop");
            selectButton(currentlySelectedButton);
        }

        if (IsDead)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
        }

        // Buying and using
        if (EventSystem.current.currentSelectedGameObject == buyShelterButton && isShopping)
        {
            print("use shelter");
            PlayerPrefs.SetInt("IsShelterUsed", 1);
            PlayerPrefs.SetInt("IsBlackniteUsed", 0);
            PlayerPrefs.SetInt("IsGhostvoicesUsed", 0);
            buyShelterButton.GetComponentInChildren<Text>().text = "♫ using ♫";
            if (buyBlackniteButton.GetComponentInChildren<Text>().text == "♫ using ♫")
            {
                buyBlackniteButton.GetComponentInChildren<Text>().text = "use";
            }
            if (buyGhostvoicesButton.GetComponentInChildren<Text>().text == "♫ using ♫")
            {
                buyGhostvoicesButton.GetComponentInChildren<Text>().text = "use";
            }
            if (BlackniteAudio.isPlaying)
            {
                BlackniteAudio.Stop();
            }
            if (GhostvoicesAudio.isPlaying)
            {
                GhostvoicesAudio.Stop();
            }
            ShelterAudio.Play();
        }
        if (EventSystem.current.currentSelectedGameObject == buyBlackniteButton && isShopping)
        {
            if (isBlackniteSold == 0)
            {
                if (collectableAmount >= 10)
                {
                    print("buy blacknite");
                    collectableAmount -= 10;
                    PlayerPrefs.SetInt("IsBlackniteSold", 1);
                    buyBlackniteButton.GetComponentInChildren<Text>().text = "use";
                }
            }
            else
            {
                print("use blacknite");
                PlayerPrefs.SetInt("IsShelterUsed", 0);
                PlayerPrefs.SetInt("IsBlackniteUsed", 1);
                PlayerPrefs.SetInt("IsGhostvoicesUsed", 0);
                buyBlackniteButton.GetComponentInChildren<Text>().text = "♫ using ♫";
                if (buyShelterButton.GetComponentInChildren<Text>().text == "♫ using ♫")
                {
                    buyShelterButton.GetComponentInChildren<Text>().text = "use";
                }
                if (buyGhostvoicesButton.GetComponentInChildren<Text>().text == "♫ using ♫")
                {
                    buyGhostvoicesButton.GetComponentInChildren<Text>().text = "use";
                }
                if (ShelterAudio.isPlaying)
                {
                    ShelterAudio.Stop();
                }
                if (GhostvoicesAudio.isPlaying)
                {
                    GhostvoicesAudio.Stop();
                }
                BlackniteAudio.Play();
            }
        }
        if (EventSystem.current.currentSelectedGameObject == buyGhostvoicesButton && isShopping)
        {
            if (isGhostvoicesSold == 0)
            {
                if (collectableAmount >= 10)
                {
                    print("buy ghost voices");
                    collectableAmount -= 10;
                    PlayerPrefs.SetInt("IsGhostvoicesSold", 1);
                    buyGhostvoicesButton.GetComponentInChildren<Text>().text = "use";
                }
            }
            else
            {
                print("use ghost voices");
                PlayerPrefs.SetInt("IsShelterUsed", 0);
                PlayerPrefs.SetInt("IsBlackniteUsed", 0);
                PlayerPrefs.SetInt("IsGhostvoicesUsed", 1);
                buyGhostvoicesButton.GetComponentInChildren<Text>().text = "♫ using ♫";
                if (buyShelterButton.GetComponentInChildren<Text>().text == "♫ using ♫")
                {
                    buyShelterButton.GetComponentInChildren<Text>().text = "use";
                }
                if (buyBlackniteButton.GetComponentInChildren<Text>().text == "♫ using ♫")
                {
                    buyBlackniteButton.GetComponentInChildren<Text>().text = "use";
                }
                if (ShelterAudio.isPlaying)
                {
                    ShelterAudio.Stop();
                }
                if (BlackniteAudio.isPlaying)
                {
                    BlackniteAudio.Stop();
                }
                GhostvoicesAudio.Play();
            }
        }
    }

    public void onLeft()
    {
        if (isShopping)
        {
            currentlySelectedButton = Mathf.Max(0, currentlySelectedButton - 1);
            selectButton(currentlySelectedButton);
        }
    }

    public void onRight()
    {
        if (isShopping)
        {
            currentlySelectedButton = Mathf.Min(2, currentlySelectedButton + 1);
            selectButton(currentlySelectedButton);
        }
    }

    private void selectButton(int index)
    {
        switch(index)
        {
            case 0:
                buyShelterButton.GetComponent<Button>().Select();
                break;
            case 1:
                buyBlackniteButton.GetComponent<Button>().Select();
                break;
            case 2:
                buyGhostvoicesButton.GetComponent<Button>().Select();
                break;
        }
    }
}
