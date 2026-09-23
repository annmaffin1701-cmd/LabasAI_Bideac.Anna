using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 5;

    [SerializeField] private SpriteRenderer spriteRenderer;

    private int currentHealth;

    private Color originalColor;

    public bool IsDead { get; private set; }


    private void Awake()
    {
        currentHealth = maxHealth;

        if (spriteRenderer == null)
            spriteRenderer =
                GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor =
                spriteRenderer.color;
    }


    public void TakeDamage(int damage)
    {
        if (IsDead)
            return;

        currentHealth -= damage;

        Debug.Log(
            $"PLAYER HP: {currentHealth}/{maxHealth}"
        );

        StartCoroutine(
            DamageFlash()
        );

        if (currentHealth <= 0)
        {
            Die();
        }
    }


    private void Die()
    {
        IsDead = true;

        currentHealth = 0;

        Debug.Log("PLAYER DEAD");

        if (spriteRenderer != null)
            spriteRenderer.color = Color.gray;
    }


    private IEnumerator DamageFlash()
    {
        if (spriteRenderer == null)
            yield break;

        spriteRenderer.color =
            Color.red;

        yield return new WaitForSeconds(
            0.15f
        );

        if (!IsDead)
            spriteRenderer.color =
                originalColor;
    }


    private void OnGUI()
    {
        GUI.Box(
            new Rect(
                20,
                20,
                150,
                50
            ),
            $"PLAYER HP\n{currentHealth} / {maxHealth}"
        );
    }
}