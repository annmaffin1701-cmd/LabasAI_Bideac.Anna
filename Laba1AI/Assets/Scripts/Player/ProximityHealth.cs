using System.Collections;
using UnityEngine;

public class ProximityHealth : MonoBehaviour
{
    [SerializeField] private int lives = 5;
    [SerializeField] private float damageDistance = 1.5f;
    [SerializeField] private float damageInterval = 1f;
    [SerializeField] private float redFlashTime = 0.15f;

    private Transform player;
    private SpriteRenderer spriteRenderer;

    private Color normalColor;
    private float damageTimer;


    private void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;

        spriteRenderer =
            GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            normalColor = spriteRenderer.color;
    }


    private void Update()
    {
        if (player == null)
            return;


        float distance =
            Vector2.Distance(
                transform.position,
                player.position
            );


        if (distance <= damageDistance)
        {
            damageTimer -= Time.deltaTime;

            if (damageTimer <= 0f)
            {
                TakeDamage();

                damageTimer = damageInterval;
            }
        }
        else
        {
            // „тобы при следующем приближении
            // урон мог пройти сразу
            damageTimer = 0f;
        }
    }


    private void TakeDamage()
    {
        lives--;

        Debug.Log(
            $"{name} получил урон. ќсталось жизней: {lives}"
        );


        if (spriteRenderer != null)
        {
            StopAllCoroutines();
            StartCoroutine(FlashRed());
        }


        if (lives <= 0)
        {
            gameObject.SetActive(false);
        }
    }


    private IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;

        yield return new WaitForSeconds(redFlashTime);

        spriteRenderer.color = normalColor;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            damageDistance
        );
    }
}