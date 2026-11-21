using TMPro;
using UnityEngine;

public class BlinkText : MonoBehaviour
{
    public float blinksPerSecond = 2f;
    public float dimAlpha = 0.15f;

    private TMP_Text text;

    void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    void Update()
    {
        if (text == null)
        {
            return;
        }

        bool bright = Mathf.Repeat(Time.unscaledTime * blinksPerSecond, 1f) < 0.5f;

        Color color = text.color;
        color.a = bright ? 1f : dimAlpha;
        text.color = color;
    }
}