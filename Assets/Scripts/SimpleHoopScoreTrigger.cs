using UnityEngine;

public class SimpleHoopScoreTrigger : MonoBehaviour
{
    public ScoreTimerLevelManager gameManager;
    public Rigidbody ball;
    public int pointsPerBasket = 1;
    public bool requireBallMovingDown = true;
    public float scoreCooldown = 0.5f;

    private float nextAllowedScoreTime;

    void OnTriggerEnter(Collider other)
    {
        Rigidbody enteringBody = other.attachedRigidbody;

        if (enteringBody == null || !enteringBody.CompareTag("Basketball"))
        {
            return;
        }

        if (requireBallMovingDown && enteringBody.linearVelocity.y > 0f)
        {
            return;
        }

        if (Time.time < nextAllowedScoreTime)
        {
            return;
        }

        nextAllowedScoreTime = Time.time + scoreCooldown;

        if (gameManager != null)
        {
            gameManager.AddScore(pointsPerBasket);
        }
    }
}