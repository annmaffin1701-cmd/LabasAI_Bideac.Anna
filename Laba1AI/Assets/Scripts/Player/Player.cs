using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float movingSpeed = 5f;

    private PlayerInput playerInput;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        playerInput = new PlayerInput();
    }

    private void OnEnable()
    {
        playerInput.Enable();
    }

    private void OnDisable()
    {
        playerInput.Disable();
    }

    private void FixedUpdate()
    {
        Vector2 inputVector = GetMovementVector();

        inputVector = inputVector.normalized;

        rb.MovePosition(
            rb.position + inputVector * movingSpeed * Time.fixedDeltaTime
        );
    }

    private Vector2 GetMovementVector()
    {
        return playerInput.Player.Move.ReadValue<Vector2>();
    }
}