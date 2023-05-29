using System.Collections;
using UnityEngine;

public class Fire_Fader : MonoBehaviour
{
    public float fadeDuration = 1f;

    private ParticleSystem[] particleSystems;

    private void Start()
    {
        // Get all the child particle systems
        particleSystems = GetComponentsInChildren<ParticleSystem>();

        // Start the PutOutFire coroutine
        StartCoroutine(PutOutFire());

        Destroy(gameObject, fadeDuration);
    }

    private IEnumerator PutOutFire()
    {
        // Stop emitting particles for all particle systems
        foreach (ParticleSystem ps in particleSystems)
        {
            if (ps.transform.name != "smokext")
            ps.Stop();
        }

        // Wait for the fade duration
        yield return new WaitForSeconds(fadeDuration);

        // Destroy the game object
        Destroy(gameObject);
    }
}
