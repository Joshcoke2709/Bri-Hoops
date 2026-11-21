using UnityEngine;

public class LEDColorCycle : MonoBehaviour
{
    public Renderer[] ledRenderers;
    public Light[] ledLights;

    public float colorSpeed = 0.5f;
    public float emissionStrength = 3f;

    void Update()
    {
        Color color = Color.HSVToRGB(Mathf.PingPong(Time.time * colorSpeed, 1f), 1f, 1f);
        Color emissionColor = color * emissionStrength;

        foreach (Renderer ledRenderer in ledRenderers)
        {
            if (ledRenderer != null)
            {
                ledRenderer.material.color = color;
                ledRenderer.material.SetColor("_EmissionColor", emissionColor);
            }
        }

        foreach (Light ledLight in ledLights)
        {
            if (ledLight != null)
            {
                ledLight.color = color;
            }
        }
    }
}