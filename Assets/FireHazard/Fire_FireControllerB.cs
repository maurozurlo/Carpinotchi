using System.Collections;
using UnityEngine;

public class Fire_FireControllerB : MonoBehaviour
{
    public ParticleSystem[] particleSystems;
    public AnimationCurve maxParticlesCurve;
    public float fadeTime = 2f;

    private int maxParticles;
    private Gradient[] originalGradients;

    private void Start()
    {
        particleSystems = GetComponentsInChildren<ParticleSystem>(); // Get all the particle systems attached to the game object
                                                                     // Store the original ColorOverLifetime gradients of each particle system child


        StartPuttingOutFire();
    }

    public void StartPuttingOutFire()
    {
        StartCoroutine(PutOutFire());
    }


    IEnumerator PutOutFire()
    {
        originalGradients = new Gradient[particleSystems.Length];
        for (int i = 0; i < particleSystems.Length; i++)
        {
            originalGradients[i] = particleSystems[i].colorOverLifetime.color.gradient;
        }

        maxParticles = particleSystems[0].main.maxParticles;
        float curveTime = maxParticlesCurve[maxParticlesCurve.length - 1].time;

        for (float t = 0f; t < curveTime; t += Time.deltaTime)
        {
            float curveValue = maxParticlesCurve.Evaluate(t);
            int newMaxParticles = (int)(maxParticles * curveValue);

            for (int i = 0; i < particleSystems.Length; i++)
            {
                ParticleSystem.MainModule main = particleSystems[i].main;

                // Create new Gradient with alpha values set according to maxParticlesCurve
                Gradient gradient = new Gradient();
                GradientColorKey[] colorKeys = new GradientColorKey[originalGradients[i].colorKeys.Length];
                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[originalGradients[i].alphaKeys.Length];
                for (int j = 0; j < originalGradients[i].colorKeys.Length; j++)
                {
                    colorKeys[j] = originalGradients[i].colorKeys[j];
                }
                for (int j = 0; j < originalGradients[i].alphaKeys.Length; j++)
                {
                    alphaKeys[j].time = originalGradients[i].alphaKeys[j].time;
                    alphaKeys[j].alpha = originalGradients[i].alphaKeys[j].alpha * curveValue;
                }
                gradient.SetKeys(colorKeys, alphaKeys);
                ParticleSystem.ColorOverLifetimeModule colorModule = particleSystems[i].colorOverLifetime;
                colorModule.color = gradient;

                main.maxParticles = newMaxParticles;
            }
        }

        // Ensure all particle systems have been fully extinguished
        yield return new WaitForSeconds(fadeTime);

        for (int i = 0; i < particleSystems.Length; i++)
        {
            ParticleSystem.MainModule main = particleSystems[i].main;

            // Set the ColorOverLifetime gradient to transparent
            Gradient gradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[originalGradients[i].colorKeys.Length];
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[originalGradients[i].alphaKeys.Length];
            for (int j = 0; j < originalGradients[i].colorKeys.Length; j++)
            {
                colorKeys[j] = originalGradients[i].colorKeys[j];
            }
            for (int j = 0; j < originalGradients[i].alphaKeys.Length; j++)
            {
                alphaKeys[j].time = originalGradients[i].alphaKeys[j].time;
                alphaKeys[j].alpha = 0f;
            }
            gradient.SetKeys(colorKeys, alphaKeys);
            ParticleSystem.ColorOverLifetimeModule colorModule = particleSystems[i].colorOverLifetime;
            colorModule.color = gradient;

            main.maxParticles = 0;
        }
    }
}