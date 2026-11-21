using UnityEngine;

public class SimpleMovingBackboard : MonoBehaviour
{
    public float moveDistance = 2f;
    public float verticalMoveDistance = 0.75f;
    public float moveSpeed = 1f;

    private Vector3 startPosition;
    private int level = 1;
    private bool movementActive = true;
    private float difficultySpeedMultiplier = 1f;
    private float phase;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (!movementActive)
        {
            return;
        }

        phase += Time.deltaTime * moveSpeed * difficultySpeedMultiplier;
        float time = phase;
        float xMovement = Mathf.Sin(time) * moveDistance;
        float yMovement = 0f;

        if (level >= 2)
        {
            yMovement = Mathf.Sin(time * 1.4f) * verticalMoveDistance;
        }

        if (level >= 4)
        {
            xMovement = Mathf.Sin(time * 1.25f) * moveDistance;
            yMovement = Mathf.Cos(time * 1.75f) * verticalMoveDistance;
        }

        transform.position = startPosition + new Vector3(xMovement, yMovement, 0f);
    }

    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    public void SetLevel(int newLevel)
    {
        level = newLevel;
    }

    public void SetMovementActive(bool isActive)
    {
        movementActive = isActive;
    }

    public void SetDifficultySpeedMultiplier(float multiplier)
    {
        difficultySpeedMultiplier = Mathf.Max(0.1f, multiplier);
    }
}
