using System.Collections;
using UnityEngine;

public class Fire_FireControllerC : MonoBehaviour
{
    public float fadeDuration = 1f;

    private ParticleSystem[] particleSystems;
    private Gradient[] originalGradients;

    private void Start()
    {
        // Get all the child particle systems
        particleSystems = GetComponentsInChildren<ParticleSystem>();

        // Store the original color gradients for each particle system
        originalGradients = new Gradient[particleSystems.Length];
        for (int i = 0; i < particleSystems.Length; i++)
        {
            originalGradients[i] = particleSystems[i].colorOverLifetime.color.gradient;
        }

        // Start the PutOutFire coroutine
        StartCoroutine(PutOutFire());
    }

    private IEnumerator PutOutFire()
    {
        float startTime = Time.time;
        float endTime = startTime + fadeDuration;

        while (Time.time < endTime)
        {
            // Calculate the alpha value based on the elapsed time
            float timeFraction = (endTime - Time.time) / fadeDuration;

            // Update the color gradient for each particle system
            for (int i = 0; i < particleSystems.Length; i++)
            {
                Gradient gradient = new Gradient();
                gradient.mode = GradientMode.Blend;
                GradientColorKey[] colorKeys = new GradientColorKey[originalGradients[i].colorKeys.Length];
                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[originalGradients[i].alphaKeys.Length];

                // Update the alpha keys based on the time fraction
                for (int j = 0; j < originalGradients[i].alphaKeys.Length; j++)
                {
                    alphaKeys[j].time = originalGradients[i].alphaKeys[j].time;
                    alphaKeys[j].alpha = originalGradients[i].alphaKeys[j].alpha * timeFraction;
                }

                // Set the color keys to the original values
                for (int j = 0; j < originalGradients[i].colorKeys.Length; j++)
                {
                    colorKeys[j] = originalGradients[i].colorKeys[j];
                }

                gradient.SetKeys(colorKeys, alphaKeys);

                // Update the color gradient of the particle system
                ParticleSystem.ColorOverLifetimeModule colorModule = particleSystems[i].colorOverLifetime;
                colorModule.color = gradient;
            }

            // Wait for the next frame
            yield return null;
        }

        // Set the alpha of all particles to zero
        for (int i = 0; i < particleSystems.Length; i++)
        {
            ParticleSystem.ColorOverLifetimeModule colorModule = particleSystems[i].colorOverLifetime;
            Gradient gradient = new Gradient();
            gradient.mode = GradientMode.Blend;
            GradientColorKey[] colorKeys = new GradientColorKey[originalGradients[i].colorKeys.Length];
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[originalGradients[i].alphaKeys.Length];

            // Set the alpha keys to zero
            for (int j = 0; j < originalGradients[i].alphaKeys.Length; j++)
            {
                alphaKeys[j].time = originalGradients[i].alphaKeys[j].time;
                alphaKeys[j].alpha = 0f;
            }

            // Set the color keys to the original values
            for (int j = 0; j < originalGradients[i].colorKeys.Length; j++)
            {
                colorKeys[j] = originalGradients[i].colorKeys[j];
            }

            gradient.SetKeys(colorKeys, alphaKeys);

            // Update the color gradient of the particle system
            colorModule.color = gradient;
        }
    }
}
