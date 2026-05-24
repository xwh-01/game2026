using UnityEngine;

public class TemporaryEffect : MonoBehaviour
{
    private float lifetime;

    public void Initialize(float effectLifetime)
    {
        lifetime = effectLifetime;
    }

    private void Update()
    {
        lifetime -= Time.deltaTime;
        if (lifetime <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
