using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class GameUI : MonoBehaviour
{
    public GameObject startScreen;
    public TextMeshProUGUI scoreLabel;
    public TextMeshProUGUI coinsLabel;
    public TextMeshProUGUI timeLabel;
    public TextMeshProUGUI livesLabel;
    public PlayerController playerController;
    public Flagpole flagpole;
    public AudioClip beep;
    public float startScreenLength = 3.0f;
    public float winScoreIncreaseInterval = 0.01f;

    public event Action OnLevelStart;

    AudioSource sfx;

    bool levelComplete = false;
    float winScoreIncreaseTimer = 0.0f;

    void Start()
    {
        sfx = GetComponent<AudioSource>();

        sfx.PlayDelayed(startScreenLength);
        startScreen.SetActive(true);
        flagpole.OnFlagpoleTouched += _ => { sfx.Stop(); };
        playerController.OnLevelComplete += () => { levelComplete = true; };
    }

    void Update()
    {
        scoreLabel.SetText($"{PlayerStats.score:D6}");
        coinsLabel.SetText($"{PlayerStats.coins:D2}");
        timeLabel.SetText($"{(int)PlayerStats.timeRemaining:D3}");
        livesLabel.SetText($"{PlayerStats.lives}");

        if (levelComplete)
        {
            if (PlayerStats.timeRemaining > 0.0f)
            {
                winScoreIncreaseTimer -= Time.deltaTime;
                while (winScoreIncreaseTimer <= 0.0f)
                {
                    winScoreIncreaseTimer += winScoreIncreaseInterval;
                    PlayerStats.score++;
                    PlayerStats.timeRemaining -= 1.0f;
                    sfx.PlayOneShot(beep);
                }
            }
            else
            {
                SceneManager.LoadScene("MainMenu");
            }
        }
        else
        {
            if (startScreenLength <= 0.0f && startScreen.activeInHierarchy)
            {
                startScreen.SetActive(false);
                OnLevelStart?.Invoke();
            }
            else
            {
                startScreenLength -= Time.deltaTime;
            }

            if (playerController.deathJumpDelay < 0.5f)
            {
                sfx.Stop();
            }
        }
    }
}
