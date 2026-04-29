using UnityEngine;

public class SpellProjectile : MonoBehaviour
{
    [Header("Spell Settings")]
    public float speed = 10f;
    public float lifetime = 3f;

    void Start()
    {
        // Destroy the spell after 'lifetime' seconds so it doesn't clutter the scene
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Move the spell forward continuously
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // For later: Here is where you would add damage logic or explosion effects!
        // Debug.Log(gameObject.name + " hit " + other.gameObject.name);
        
        // Destroy the spell if it hits something
        // Destroy(gameObject); 
    }
}
