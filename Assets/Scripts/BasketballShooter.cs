using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BasketballShooter : MonoBehaviour
{
    [Header("Scene References")]
    public Camera gameplayCamera;
    public Rigidbody ball;
    public Transform shootPoint;
    public Transform aimCenter;
    public Transform targetPoint;
    public LineRenderer aimArc;
    public ScoreTimerLevelManager gameManager;

    [Header("Mouse Aim Area")]
    public float aimWidth = 3f;
    public float aimHeight = 2f;
    public bool allowDesktopMouseAim = true;
    public bool allowDesktopKeyboardShoot = true;

    [Header("Shot Settings")]
    public float arcHeight = 4f;
    public float maxArcHeight = 6.5f;
    public float resetAfterSeconds = 3f;

    [Header("Ball Respawn")]
    public float startReloadDelay = 1f;
    public float reloadDropPerLevel = 0.15f;
    public float minReloadDelay = 0.4f;

    [Header("Aim Arc")]
    public int arcSegments = 30;
    public float arcPreviewTime = 2f;

    [Header("Aim Dashes")]
    public Material dashMaterial;
    public Vector3 dashSize = new Vector3(0.1f, 0.1f, 0.3f);
    public float dashSpacingTime = 0.12f;
    public float dashSpeed = 0.5f;

    private readonly List<Transform> dashes = new List<Transform>();

    private bool canShoot = true;
    private readonly List<Rigidbody> flyingBalls = new List<Rigidbody>();
    private bool inputEnabled = true;

    public bool IsBallReady => canShoot;
    public bool IsInputEnabled => inputEnabled;

    void Start()
    {
        if (gameplayCamera == null)
        {
            gameplayCamera = Camera.main;
        }

        ResetBall();
    }

    void Update()
    {
        if (!CanUseGameplayInput())
        {
            if (aimArc != null)
            {
                aimArc.enabled = false;
            }
            SetDashesVisible(0);
            return;
        }

        if (allowDesktopMouseAim && Input.touchCount == 0)
        {
            SetAimFromScreenPosition(Input.mousePosition);
        }

        DrawAimArc();

        if (allowDesktopKeyboardShoot && Input.touchCount == 0 && Input.GetKeyDown(KeyCode.Space))
        {
            TryShoot();
        }
    }

    public void SetAimFromScreenPosition(Vector2 screenPosition)
    {
        if (gameplayCamera == null || aimCenter == null || targetPoint == null)
        {
            return;
        }

        Vector3 screenPoint = new Vector3(screenPosition.x, screenPosition.y, 0f);
        Vector3 mouseViewportPosition = gameplayCamera.ScreenToViewportPoint(screenPoint);
        mouseViewportPosition.x = Mathf.Clamp01(mouseViewportPosition.x);
        mouseViewportPosition.y = Mathf.Clamp01(mouseViewportPosition.y);

        float xOffset = Mathf.Lerp(-aimWidth * 0.5f, aimWidth * 0.5f, mouseViewportPosition.x);
        float yOffset = Mathf.Lerp(-aimHeight * 0.5f, aimHeight * 0.5f, mouseViewportPosition.y);

        targetPoint.position = aimCenter.position
            + aimCenter.right * xOffset
            + aimCenter.up * yOffset;
    }

    public bool TryShoot()
    {
        return TryShoot(0f);
    }

    public bool TryShoot(float loft01)
    {
        if (!canShoot || !CanUseGameplayInput())
        {
            return false;
        }

        ShootBall(Mathf.Lerp(arcHeight, maxArcHeight, Mathf.Clamp01(loft01)));
        return true;
    }

    void ShootBall(float height)
    {
        canShoot = false;

        Rigidbody shotBall = ball;
        flyingBalls.Add(shotBall);

        shotBall.isKinematic = false;
        shotBall.useGravity = true;
        shotBall.linearVelocity = CalculateLaunchVelocity(shootPoint.position, targetPoint.position, height);

        if (aimArc != null)
        {
            aimArc.enabled = false;
        }

        StartCoroutine(ShotRoutine(shotBall));
    }

    public void SetInputEnabled(bool isEnabled)
    {
        inputEnabled = isEnabled;

        if (aimArc != null)
        {
            aimArc.enabled = isEnabled && IsRoundActive() && canShoot;
        }
    }

    float GetReloadDelay()
    {
        int level = gameManager != null ? gameManager.Level : 1;
        float delay = startReloadDelay - (level - 1) * reloadDropPerLevel;
        return Mathf.Max(minReloadDelay, delay);
    }

    IEnumerator ShotRoutine(Rigidbody shotBall)
    {
        float reloadDelay = GetReloadDelay();
        yield return new WaitForSeconds(reloadDelay);

        SpawnNextBall(shotBall);
        canShoot = true;

        if (aimArc != null)
        {
            aimArc.enabled = IsRoundActive();
        }

        yield return new WaitForSeconds(Mathf.Max(0f, resetAfterSeconds - reloadDelay));

        flyingBalls.Remove(shotBall);

        if (shotBall != null)
        {
            Destroy(shotBall.gameObject);
        }
    }

    void SpawnNextBall(Rigidbody shotBall)
    {
        Rigidbody newBall = Instantiate(shotBall, shootPoint.position, Quaternion.identity);

        newBall.linearVelocity = Vector3.zero;
        newBall.angularVelocity = Vector3.zero;
        newBall.useGravity = false;
        newBall.isKinematic = true;

        Collider newCollider = newBall.GetComponentInChildren<Collider>();

        if (newCollider != null)
        {
            for (int i = 0; i < flyingBalls.Count; i++)
            {
                if (flyingBalls[i] == null)
                {
                    continue;
                }

                Collider otherCollider = flyingBalls[i].GetComponentInChildren<Collider>();

                if (otherCollider != null)
                {
                    Physics.IgnoreCollision(newCollider, otherCollider);
                }
            }
        }

        ball = newBall;
    }
    void ResetBall()
    {
        ball.isKinematic = true;
        ball.useGravity = false;
        ball.linearVelocity = Vector3.zero;
        ball.angularVelocity = Vector3.zero;

        ball.transform.position = shootPoint.position;
        ball.transform.rotation = Quaternion.identity;

        canShoot = true;

        if (aimArc != null)
        {
            aimArc.enabled = IsRoundActive();
        }
    }

    bool CanUseGameplayInput()
    {
        return inputEnabled && IsRoundActive() && Time.timeScale > 0f;
    }

    bool IsRoundActive()
    {
        return gameManager == null || gameManager.RoundActive;
    }

    void DrawAimArc()
    {
        if (aimArc == null || shootPoint == null || targetPoint == null)
        {
            return;
        }

        aimArc.positionCount = 0;

        if (!aimArc.enabled)
        {
            SetDashesVisible(0);
            return;
        }

        Vector3 start = shootPoint.position;
        Vector3 velocity = CalculateLaunchVelocity(start, targetPoint.position, arcHeight);

        float offset = Mathf.Repeat(Time.time * dashSpeed, dashSpacingTime);
        int count = Mathf.FloorToInt((arcPreviewTime - offset) / dashSpacingTime) + 1;

        for (int i = 0; i < count; i++)
        {
            float t = offset + i * dashSpacingTime;
            Vector3 position = start + velocity * t + 0.5f * Physics.gravity * t * t;
            Vector3 direction = velocity + Physics.gravity * t;

            Transform dash = GetDash(i);
            dash.position = position;
            dash.rotation = Quaternion.LookRotation(direction);
        }

        SetDashesVisible(count);
    }

    Vector3 CalculateLaunchVelocity(Vector3 start, Vector3 target, float height)
    {
        float gravity = Mathf.Abs(Physics.gravity.y);
        float verticalDifference = target.y - start.y;
        Vector3 horizontalDifference = new Vector3(target.x - start.x, 0f, target.z - start.z);

        float peakHeight = Mathf.Max(height, verticalDifference + 0.5f);
        float timeUp = Mathf.Sqrt(2f * peakHeight / gravity);
        float timeDown = Mathf.Sqrt(2f * Mathf.Max(peakHeight - verticalDifference, 0.1f) / gravity);
        float totalTime = timeUp + timeDown;

        Vector3 horizontalVelocity = horizontalDifference / totalTime;
        Vector3 verticalVelocity = Vector3.up * Mathf.Sqrt(2f * gravity * peakHeight);

        return horizontalVelocity + verticalVelocity;
    }

    Transform GetDash(int index)
    {
        while (dashes.Count <= index)
        {
            GameObject dash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dash.name = "AimDash";

            Collider dashCollider = dash.GetComponent<Collider>();
            dashCollider.enabled = false;
            Destroy(dashCollider);

            MeshRenderer dashRenderer = dash.GetComponent<MeshRenderer>();
            dashRenderer.sharedMaterial = dashMaterial != null ? dashMaterial : aimArc.sharedMaterial;
            dashRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            dash.transform.localScale = dashSize;
            dashes.Add(dash.transform);
        }

        return dashes[index];
    }

    void SetDashesVisible(int visibleCount)
    {
        for (int i = 0; i < dashes.Count; i++)
        {
            dashes[i].gameObject.SetActive(i < visibleCount);
        }
    }
}
