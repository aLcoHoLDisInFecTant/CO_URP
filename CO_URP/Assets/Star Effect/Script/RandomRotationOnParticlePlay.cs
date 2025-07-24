using UnityEngine;

namespace Game.RandomRotation
{
    
public class RandomRotationOnParticlePlay : MonoBehaviour
{
    private ParticleSystem particleSystem;

    void Start()
    {
        // Get the ParticleSystem component
        particleSystem = GetComponent<ParticleSystem>();
    }

    public void PlayWithRandomRotation()
    {
        // Set a random rotation angle
        float randomX = Random.Range(0f, 360f);
        float randomY = Random.Range(0f, 360f);
        float randomZ = Random.Range(0f, 360f);

        // Apply the random rotation
        transform.rotation = Quaternion.Euler(randomX, randomY, randomZ);

        // Play the particle system
        particleSystem.Play();
    }
}
}
