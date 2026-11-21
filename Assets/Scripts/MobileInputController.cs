using UnityEngine;
using UnityEngine.EventSystems;


public class MobileInputController : MonoBehaviour
{
    public BasketballShooter shooter;
    public ScoreTimerLevelManager gameManager;
    public GamePauseManager pauseManager;

    [Header("Touch Aim")]
    public bool ignoreTouchesOverUI = true;

    [Header("Swipe Shoot")]
    public float minSwipeHeight = 0.12f;
    public float steadySpeed = 1f;
    public float steadyDelay = 0.04f;
    public float minUpwardShare = 0.5f;
    public float fullLoftSwipeHeight = 0.4f;
    private Vector2 lastTouchPosition;
    private Vector2 swipeStart;
    private float steadyTime;
    private bool shotThisTouch;

    private int activeFingerId = -1;
  
    private bool swipeActive;
    private float swipeLength;

    void Update()
    {
        if (Input.touchCount == 0)
        {
            activeFingerId = -1;
            return;
        }

        if (activeFingerId == -1)
        {
            TryStartAiming();
            return;
        }

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if (touch.fingerId != activeFingerId)
            {
                continue;
            }

            HandleActiveTouch(touch);
            return;
        }

        activeFingerId = -1;
    }

    void TryStartAiming()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if (touch.phase != TouchPhase.Began)
            {
                continue;
            }

            if (!CanUseTouchInput() || IsTouchOverUI(touch.fingerId))
            {
                continue;
            }

            activeFingerId = touch.fingerId;
            shotThisTouch = false;
            swipeActive = false;
            swipeLength = 0f;
            lastTouchPosition = touch.position;
            swipeStart = touch.position;
            steadyTime = steadyDelay;
            SetAim(touch.position);
            return;
        }
    }

    void HandleActiveTouch(Touch touch)
    {
        if (touch.phase == TouchPhase.Canceled)
        {
            activeFingerId = -1;
            return;
        }

        bool ended = touch.phase == TouchPhase.Ended;

        if (!shotThisTouch)
        {
            TrackFinger(touch);

            if (ended && swipeActive)
            {
                FireSwipe();
            }
        }

        if (ended)
        {
            activeFingerId = -1;
        }
    }

    void TrackFinger(Touch touch)
    {
        float dt = Mathf.Max(Time.unscaledDeltaTime, 0.001f);
        float speed = (touch.position - lastTouchPosition).magnitude / Screen.height / dt;
        lastTouchPosition = touch.position;

        bool isSlow = speed < steadySpeed;
        steadyTime = isSlow ? steadyTime + dt : 0f;

        if (isSlow)
        {
            if (steadyTime < steadyDelay)
            {
                return;
            }

            if (swipeActive)
            {
                FireSwipe();
                return;
            }

            swipeStart = touch.position;
            SetAim(touch.position);
            return;
        }

        Vector2 swipe = touch.position - swipeStart;
        bool farEnough = swipe.magnitude >= minSwipeHeight * Screen.height;
        bool upward = swipe.y >= swipe.magnitude * minUpwardShare;

        if (farEnough && upward)
        {
            swipeActive = true;
            swipeLength = swipe.magnitude;
        }
    }

    void FireSwipe()
    {
        shotThisTouch = true;
        swipeActive = false;

        if (!CanUseTouchInput())
        {
            return;
        }

        float swipeHeights = swipeLength / Screen.height;
        float loft = Mathf.InverseLerp(minSwipeHeight, fullLoftSwipeHeight, swipeHeights);
        shooter.TryShoot(loft);
    }

    

    bool CanUseTouchInput()
    {
        if (shooter == null)
        {
            return false;
        }

        if (pauseManager != null && pauseManager.IsPaused)
        {
            return false;
        }

        if (gameManager != null && !gameManager.RoundActive)
        {
            return false;
        }

        return shooter.IsInputEnabled && Time.timeScale > 0f;
    }

    bool IsTouchOverUI(int fingerId)
    {
        return ignoreTouchesOverUI
            && EventSystem.current != null
            && EventSystem.current.IsPointerOverGameObject(fingerId);
    }

    

    void SetAim(Vector2 screenPosition)
    {
        if (shooter != null)
        {
            shooter.SetAimFromScreenPosition(screenPosition);
        }
    }
}
