using UnityEditor;
using UnityEngine;

public enum SpriteAnimState
{
    Idle,
    Walking,
    Running
}

public enum FacingDirection
{
    Down,
    Up,
    Left,
    Right
}

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runMultiplier = 2.5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("References")]
    private SpriteVisual spriteVisual;   
    [SerializeField] private Transform lookPointer; 

    private CharacterController controller;

    private Vector3 input;
    private Vector3 velocity;
    private PlayerState state = PlayerState.Idle;

    public bool freeze = false;

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        if (spriteVisual == null)
        {
            spriteVisual = GetComponentInChildren<SpriteVisual>();
        }
    }

    private void Update()
    {
        if (freeze) return;
        HandleMovement();
        HandleGravity();
        ApplyMovement();
    }

    private void HandleMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        input = new Vector3(x, 0f, z);

        if (input.magnitude < 0.1f)
        {
            state = PlayerState.Idle;

            if (spriteVisual != null)
                spriteVisual.SetState(SpriteAnimState.Idle);

            return;
        }

        input.Normalize();

        bool running = Input.GetKey(KeyCode.LeftShift);
        state = running ? PlayerState.Running : PlayerState.Walking;

        FacingDirection facingDir;
        if (Mathf.Abs(input.x) > Mathf.Abs(input.z))
        {
            facingDir = (input.x > 0f) ? FacingDirection.Right : FacingDirection.Left;
        }
        else
        {
            facingDir = (input.z > 0f) ? FacingDirection.Up : FacingDirection.Down;
        }

        Vector3 lookDir = new Vector3(input.x, 0f, input.z);
        if (lookDir.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(lookDir.x, lookDir.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            if (lookPointer != null)
                lookPointer.forward = lookDir;
        }

        if (spriteVisual != null)
        {
            spriteVisual.SetDirection(facingDir);

            if (state == PlayerState.Running)
                spriteVisual.SetState(SpriteAnimState.Running);
            else
                spriteVisual.SetState(SpriteAnimState.Walking);
        }
    }

    private void HandleGravity()
    {
        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }
    }

    private void ApplyMovement()
    {
        float speed = (state == PlayerState.Running) ? moveSpeed * runMultiplier : moveSpeed;

        Vector3 horizontal = input * speed;
        Vector3 finalMovement = horizontal + velocity;

        controller.Move(finalMovement * Time.deltaTime);
    }
}

public enum PlayerState
{
    Idle,
    Walking,
    Running
}