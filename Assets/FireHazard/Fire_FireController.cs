using System.Collections;
using UnityEngine;

public class Fire_FireController : MonoBehaviour
{
    public AnimationCurve particleCurve; // The animation curve to control the amount of particles
    public AnimationCurve opacityCurve; // The animation curve to control the opacity of the particle systems
    public float duration; // The amount of time it takes for the particles to reach 0
    private ParticleSystem[] particleSystems; // The particle systems attached to the game object
    private float timeElapsed; // The amount of time that has elapsed since the coroutine started

    private void Start()
    {
        particleSystems = GetComponentsInChildren<ParticleSystem>(); // Get all the particle systems attached to the game object
        StartPuttingOutFire();
    }

    private IEnumerator PutOutFire()
    {
        float maxParticles = particleSystems[0].main.maxParticles; // Get the initial max particles
        while (timeElapsed < duration)
        {
            float normalizedTime = timeElapsed / duration; // Get the normalized time
            float particleCurveValue = particleCurve.Evaluate(normalizedTime); // Evaluate the particle curve for the current time
            float opacityCurveValue = opacityCurve.Evaluate(normalizedTime); // Evaluate the opacity curve for the current time
            foreach (ParticleSystem particleSystem in particleSystems)
            {
                var main = particleSystem.main;
                main.maxParticles = (int)(maxParticles * particleCurveValue); // Set the max particles for all particle systems
                var colorOverLifetime = particleSystem.colorOverLifetime;
                colorOverLifetime.enabled = true;
                Gradient gradient = new Gradient();
                gradient.SetKeys(new GradientColorKey[] { new GradientColorKey(particleSystem.main.startColor.color, 0f), new GradientColorKey(particleSystem.main.startColor.color, 1f) }, new GradientAlphaKey[] { new GradientAlphaKey(opacityCurveValue, 0f), new GradientAlphaKey(0f, 1f) });
                colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);
            }
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        // Disable the particle systems once the fire is put out
        foreach (ParticleSystem particleSystem in particleSystems)
        {
            particleSystem.gameObject.SetActive(false);
        }
    }

    public void StartPuttingOutFire()
    {
        StartCoroutine(PutOutFire());
    }
}
