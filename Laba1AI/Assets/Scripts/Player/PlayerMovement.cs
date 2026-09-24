using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float movingSpeed = 5f;

    private PlayerInput playerInput;
    private Rigidbody2D rb;


    // =====================================================
    // INITIALIZATION
    // =====================================================

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        InitializeInput();
    }


    private void OnEnable()
    {
        InitializeInput();

        playerInput.Enable();
    }


    private void OnDisable()
    {
        if (playerInput != null)
        {
            playerInput.Disable();
        }
    }


    private void OnDestroy()
    {
        if (playerInput != null)
        {
            playerInput.Dispose();
            playerInput = null;
        }
    }


    // =====================================================
    // MOVEMENT
    // =====================================================

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        Vector2 inputVector = GetMovementVector();

        // „тобы по диагонали игрок не двигалс€ быстрее
        inputVector = inputVector.normalized;

        Vector2 newPosition =
            rb.position +
            inputVector *
            movingSpeed *
            Time.fixedDeltaTime;

        rb.MovePosition(newPosition);
    }


    // =====================================================
    // INPUT
    // =====================================================

    private Vector2 GetMovementVector()
    {
        if (playerInput == null)
            return Vector2.zero;

        return playerInput.Player.Move.ReadValue<Vector2>();
    }


    private void InitializeInput()
    {
        if (playerInput == null)
        {
            playerInput = new PlayerInput();
        }
    }
}