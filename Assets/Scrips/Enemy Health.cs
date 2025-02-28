using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] private float hitPoints = 4f; 
    private float maxHitPoints;                   
    private bool isDestroyed = false;
    public HealthBar healthBar;

    void Start()
    {
        maxHitPoints = hitPoints;
        healthBar.SetHealth(hitPoints, maxHitPoints);
    }

    public void TakeDamage(int dmg)
    {
        hitPoints -= dmg;
        healthBar.SetHealth(hitPoints, maxHitPoints);

        if (hitPoints <= 0 && !isDestroyed)
        {
            EnemySpawner.onEnemyDestroy.Invoke();
            isDestroyed = true;
            Destroy(gameObject);
        }
    }

   
    public float GetCurrentHealth()
    {
        return hitPoints;
    }

    public float GetMaxHealth()
    {
        return maxHitPoints;
    }
}