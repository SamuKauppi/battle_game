using UnityEngine;

public class ParticleOrbit : MonoBehaviour
{
    public float minSpeed = 2f;
    public float maxSpeed = 6f;

    private new ParticleSystem particleSystem;
    private ParticleSystem.Particle[] particles;

    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
    }

    void Update()
    {
        int particleCount = particleSystem.GetParticles(particles);

        for (int i = 0; i < particleCount; i++)
        {
            particles[i].velocity = Random.onUnitSphere * Random.Range(minSpeed, maxSpeed);
        }

        particleSystem.SetParticles(particles, particleCount);
    }
}
