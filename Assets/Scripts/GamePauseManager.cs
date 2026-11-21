using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum Difficulty
{
    Easy,
    Normal,
    Hard
}

public class GamePauseManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject pauseMenu;
    public Button pauseButton;
    public Button resumeButton;
    public Button restartButton;
    public TMP_Dropdown difficultyDropdown;
    public GameObject difficultyMenu;

    [Header("Gameplay References")]
    public MonoBehaviour shooterScript;
    public SimpleMovingBackboard movingHoop;
    public ScoreTimerLevelManager scoreTimer;

    [Header("Difficulty")]
    public Difficulty currentDifficulty = Difficulty.Normal;
    public float easyHoopSpeedMultiplier = 0.6f;
    public float normalHoopSpeedMultiplier = 1f;
    public float hardHoopSpeedMultiplier = 1.5f;
    public float easyTimerMultiplier = 1f;
    public float normalTimerMultiplier = 1f;
    public float hardTimerMultiplier = 1f;
    public float easyScoreTargetMultiplier = 1f;
    public float normalScoreTargetMultiplier = 1f;
    public float hardScoreTargetMultiplier = 1f;

    [Header("Arcade Font")]
    public TMP_FontAsset arcadeFont;
    public TMP_Text[] pauseMenuTexts;
    public bool includePauseMenuChildTexts = true;

    public bool IsPaused { get; private set; }
    private static Difficulty savedDifficulty = Difficulty.Normal;
    private static bool hasSavedDifficulty;

    private BasketballShooter Shooter => shooterScript as BasketballShooter;

    void Awake()
    {
        Time.timeScale = 1f;

        if (hasSavedDifficulty)
        {
            currentDifficulty = savedDifficulty;
        }

        WireButtons();
        HidePauseMenu();
        ApplyArcadeFont();
        ApplyCurrentDifficulty();
    }

    void OnDestroy()
    {
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(PauseGame);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(ResumeGame);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartGame);
        }

        if (difficultyDropdown != null)
        {
            difficultyDropdown.onValueChanged.RemoveListener(SetDifficulty);
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (difficultyMenu != null && difficultyMenu.activeSelf)
            {
                CloseDifficultyMenu();
            }
            else if (IsPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }
    public void PauseGame()
    {
        IsPaused = true;
        Time.timeScale = 0f;

        if (pauseMenu != null)
        {
            pauseMenu.SetActive(true);
        }

        if (difficultyMenu != null)
        {
            difficultyMenu.SetActive(false);
        }

        SetShooterInput(false);
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        HidePauseMenu();
        SetShooterInput(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SetDifficulty(int difficultyIndex)
    {
        currentDifficulty = (Difficulty)Mathf.Clamp(difficultyIndex, 0, 2);
        savedDifficulty = currentDifficulty;
        hasSavedDifficulty = true;
        ApplyCurrentDifficulty();
    }
    public void OpenDifficultyMenu()
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }

        if (difficultyMenu != null)
        {
            difficultyMenu.SetActive(true);
        }
    }

    public void CloseDifficultyMenu()
    {
        if (difficultyMenu != null)
        {
            difficultyMenu.SetActive(false);
        }

        if (pauseMenu != null)
        {
            pauseMenu.SetActive(true);
        }
    }

    public void ChooseDifficulty(int difficultyIndex)
    {
        SetDifficulty(difficultyIndex);
        CloseDifficultyMenu();
    }


    public void ApplyArcadeFont()
    {
        if (arcadeFont == null)
        {
            return;
        }

        ApplyFontToPauseText();
    }

    void WireButtons()
    {
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(PauseGame);
            pauseButton.onClick.AddListener(PauseGame);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(ResumeGame);
            resumeButton.onClick.AddListener(ResumeGame);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartGame);
            restartButton.onClick.AddListener(RestartGame);
        }

        if (difficultyDropdown != null)
        {
            difficultyDropdown.onValueChanged.RemoveListener(SetDifficulty);
            difficultyDropdown.onValueChanged.AddListener(SetDifficulty);
            EnsureDifficultyOptions();
            difficultyDropdown.value = (int)currentDifficulty;
            difficultyDropdown.RefreshShownValue();
        }
    }

    void EnsureDifficultyOptions()
    {
        if (difficultyDropdown.options.Count >= 3)
        {
            return;
        }

        difficultyDropdown.ClearOptions();
        difficultyDropdown.AddOptions(new List<string>
        {
            "Easy",
            "Normal",
            "Hard"
        });
    }

    void ApplyCurrentDifficulty()
    {
        float hoopSpeedMultiplier = normalHoopSpeedMultiplier;
        float timerMultiplier = normalTimerMultiplier;
        float scoreTargetMultiplier = normalScoreTargetMultiplier;

        if (currentDifficulty == Difficulty.Easy)
        {
            hoopSpeedMultiplier = easyHoopSpeedMultiplier;
            timerMultiplier = easyTimerMultiplier;
            scoreTargetMultiplier = easyScoreTargetMultiplier;
        }
        else if (currentDifficulty == Difficulty.Hard)
        {
            hoopSpeedMultiplier = hardHoopSpeedMultiplier;
            timerMultiplier = hardTimerMultiplier;
            scoreTargetMultiplier = hardScoreTargetMultiplier;
        }

        if (scoreTimer != null)
        {
            scoreTimer.ApplyDifficulty(timerMultiplier, hoopSpeedMultiplier, scoreTargetMultiplier);
        }

        if (movingHoop != null)
        {
            movingHoop.SetDifficultySpeedMultiplier(hoopSpeedMultiplier);
        }
    }

    void HidePauseMenu()
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }

        if (difficultyMenu != null)
        {
            difficultyMenu.SetActive(false);
        }
    }

    void SetShooterInput(bool isEnabled)
    {
        if (Shooter != null)
        {
            Shooter.SetInputEnabled(isEnabled);
        }
    }

    void ApplyFontToPauseText()
    {
        if (pauseMenuTexts != null)
        {
            for (int i = 0; i < pauseMenuTexts.Length; i++)
            {
                ApplyFont(pauseMenuTexts[i]);
            }
        }

        if (!includePauseMenuChildTexts || pauseMenu == null)
        {
            return;
        }

        TMP_Text[] childTexts = pauseMenu.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < childTexts.Length; i++)
        {
            ApplyFont(childTexts[i]);
        }
    }

    void ApplyFont(TMP_Text text)
    {
        if (text != null)
        {
            text.font = arcadeFont;
        }
    }
}
