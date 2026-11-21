using TMPro;
using UnityEngine;

public class ScoreTimerLevelManager : MonoBehaviour
{
    [Header("Round")]
    public float roundLength = 60f;
    public float levelBonusTime = 30f;

    [Header("Level Targets")]
    public int[] scoreTargets = { 5, 10, 20, 50 };

    [Header("Hoop Movement")]
    public SimpleMovingBackboard movingBackboard;
    public float startingHoopSpeed = 1f;
    public float speedIncreasePerLevel = 1.5f;

    [Header("UI")]
    public TMP_Text scoreText;
    public TMP_Text timerText;
    public TMP_Text levelText;
    public TMP_Text nextLevelText;
    public TMP_Text gameOverText;
    public GameObject gameOverPanel;
    public TMP_FontAsset arcadeFont;
    public TMP_Text[] extraTextToStyle;

    private int score;
    private int level = 1;
    private float timeRemaining;
    private bool roundActive;
    private float timerMultiplier = 1f;
    private float scoreTargetMultiplier = 1f;
    private float hoopSpeedMultiplier = 1f;

    public bool RoundActive => roundActive;
    public int Level => level;

    void Awake()
    {
        ApplyArcadeFont();
        StartRound();
    }

    void Update()
    {
        if (!roundActive)
        {
            return;
        }

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            roundActive = false;
            StopGame();

            if (gameOverText != null)
            {
                gameOverPanel.SetActive(true);
                gameOverText.text = "TIME!";
            }
        }

        UpdateUI();
    }

    public void StartRound()
    {
        score = 0;
        level = 1;
        timeRemaining = GetLevelTime();
        roundActive = true;

        if (movingBackboard != null)
        {
            movingBackboard.SetMoveSpeed(startingHoopSpeed);
            movingBackboard.SetDifficultySpeedMultiplier(hoopSpeedMultiplier);
            movingBackboard.SetLevel(level);
            movingBackboard.SetMovementActive(true);
        }

        if (gameOverText != null)
        {
            gameOverPanel.SetActive(false);
        }

        UpdateUI();
    }

    public void AddScore(int points)
    {
        if (!roundActive)
        {
            return;
        }

        score += points;
        CheckForLevelUp();
        UpdateUI();
    }

    void CheckForLevelUp()
    {
        while (score >= GetCurrentLevelTarget())
        {
            level++;
            timeRemaining += levelBonusTime;
            UpdateHoopSpeed();
            UpdateHoopMovement();
        }
    }

    float GetLevelTime()
    {
        return roundLength * timerMultiplier;
    }

    int GetCurrentLevelTarget()
    {
        int targetIndex = level - 1;
        int baseTarget;

        if (targetIndex < scoreTargets.Length)
        {
            baseTarget = scoreTargets[targetIndex];
        }
        else
        {
            int lastTarget = scoreTargets[scoreTargets.Length - 1];
            int extraLevels = targetIndex - scoreTargets.Length + 1;
            baseTarget = lastTarget * (int)Mathf.Pow(2, extraLevels);
        }

        return Mathf.Max(1, Mathf.CeilToInt(baseTarget * scoreTargetMultiplier));
    }

    void UpdateHoopSpeed()
    {
        if (movingBackboard == null)
        {
            return;
        }

        float newSpeed = startingHoopSpeed + (level - 1) * speedIncreasePerLevel;
        movingBackboard.SetMoveSpeed(newSpeed);
        movingBackboard.SetDifficultySpeedMultiplier(hoopSpeedMultiplier);
    }

    void UpdateHoopMovement()
    {
        if (movingBackboard == null)
        {
            return;
        }

        movingBackboard.SetLevel(level);
    }

    void StopGame()
    {
        if (movingBackboard != null)
        {
            movingBackboard.SetMovementActive(false);
        }
    }

    public void ApplyDifficulty(float newTimerMultiplier, float newHoopSpeedMultiplier, float newScoreTargetMultiplier)
    {
        float oldTimerMultiplier = timerMultiplier;

        timerMultiplier = Mathf.Max(0.1f, newTimerMultiplier);
        hoopSpeedMultiplier = Mathf.Max(0.1f, newHoopSpeedMultiplier);
        scoreTargetMultiplier = Mathf.Max(0.1f, newScoreTargetMultiplier);

        if (movingBackboard != null)
        {
            movingBackboard.SetDifficultySpeedMultiplier(hoopSpeedMultiplier);
        }

        if (roundActive)
        {
            timeRemaining *= timerMultiplier / oldTimerMultiplier;
            CheckForLevelUp();
        }

        UpdateUI();
    }

    public void SetArcadeFont(TMP_FontAsset fontAsset)
    {
        arcadeFont = fontAsset;
        ApplyArcadeFont();
    }

    public void ApplyArcadeFont()
    {
        if (arcadeFont == null)
        {
            return;
        }

        ApplyFontTo(scoreText);
        ApplyFontTo(timerText);
        ApplyFontTo(levelText);
        ApplyFontTo(nextLevelText);

        if (extraTextToStyle == null)
        {
            return;
        }

        for (int i = 0; i < extraTextToStyle.Length; i++)
        {
            ApplyFontTo(extraTextToStyle[i]);
        }
    }

    void ApplyFontTo(TMP_Text text)
    {
        if (text != null)
        {
            text.font = arcadeFont;
        }
    }

    void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString("00");
        }

        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(timeRemaining).ToString("00");
        }

        if (levelText != null)
        {
            levelText.text = "LVL " + level;
        }

        if (nextLevelText != null)
        {
            nextLevelText.text = GetCurrentLevelTarget().ToString();
        }
    }
}
