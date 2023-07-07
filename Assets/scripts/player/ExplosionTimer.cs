using System.Collections;
using UnityEngine;

public class ExplosionTimer : MonoBehaviour
{
    [SerializeField] private ParticleSystem particle;
    bool isPlaying;

    private void Update()
    {
        if (gameObject.activeInHierarchy && !isPlaying)
        {
            StartCoroutine(ParticleShow());
        }
    }

    IEnumerator ParticleShow()
    {
        isPlaying = true;
        particle.Play();
        yield return new WaitForSeconds(particle.main.duration);
        particle.Stop();
        isPlaying = false;
        gameObject.SetActive(false);
    }
}
