using UnityEngine;

public enum SpriteAnimState { Idle, Walking, Running }
public enum FacingDirection { Down, Up, Left, Right }
public enum PlayerState { Idle, Walking, Running }

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runMultiplier = 2.5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    private SpriteVisual spriteVisual;
    private CharacterController controller;

    private Vector3 moveWorld;
    private Vector3 velocity;
    private PlayerState state = PlayerState.Idle;

    public bool freeze = false;

    private Vector3 cachedForward = Vector3.forward;
    private Vector3 cachedRight = Vector3.right;
    private bool hasCachedAxes = false;

    [Header("Interaction / Look Pointer")]
    [SerializeField] private Transform lookPointer;
    [SerializeField] private float interactDistance = 0.8f;
    [SerializeField] private float interactHeight = 0.0f;
    private Vector3 lastLookDir = Vector3.forward;

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        if (spriteVisual == null)
            spriteVisual = GetComponentInChildren<SpriteVisual>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
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
        Vector3 input = new Vector3(x, 0f, z);

        // Assi attuali della camera (per lo sprite)
        GetCameraAxesOnPlane(out var camForward, out var camRight);

        if (input.sqrMagnitude < 0.01f)
        {
            state = PlayerState.Idle;
            moveWorld = Vector3.zero;

            // Se sei fermo, aggiorna comunque lo sprite in base alla camera usando lastLookDir
            if (spriteVisual != null)
            {
                var idleFacing = GetFacingRelativeToCamera(lastLookDir, camForward, camRight);
                spriteVisual.SetDirection(idleFacing);
                spriteVisual.SetState(SpriteAnimState.Idle);
            }

            hasCachedAxes = false; // ok lasciarlo: quando riparti, ricachea gli assi per il movimento
            return;
        }

        input.Normalize();

        // Cache assi SOLO per il movimento
        if (!hasCachedAxes)
        {
            cachedForward = camForward;
            cachedRight = camRight;
            hasCachedAxes = true;
        }

        // Movimento in world basato sugli assi cachati
        moveWorld = (cachedRight * input.x + cachedForward * input.z);
        if (moveWorld.sqrMagnitude > 0.0001f)
            moveWorld.Normalize();

        bool running = Input.GetKey(KeyCode.LeftShift);
        state = running ? PlayerState.Running : PlayerState.Walking;

        UpdateLookPointer(moveWorld);

        // FACING calcolato rispetto alla camera ATTUALE (non cached)
        FacingDirection facingDir = GetFacingRelativeToCamera(moveWorld, camForward, camRight);

        if (spriteVisual != null)
        {
            spriteVisual.SetDirection(facingDir);
            spriteVisual.SetState(state == PlayerState.Running ? SpriteAnimState.Running : SpriteAnimState.Walking);
        }
    }

    private FacingDirection GetFacingRelativeToCamera(Vector3 moveDirWorld, Vector3 camForwardOnPlane, Vector3 camRightOnPlane)
    {
        float side = Vector3.Dot(moveDirWorld, camRightOnPlane);
        float fwd = Vector3.Dot(moveDirWorld, camForwardOnPlane);

        if (Mathf.Abs(side) > Mathf.Abs(fwd))
            return (side > 0f) ? FacingDirection.Right : FacingDirection.Left;
        else
            return (fwd > 0f) ? FacingDirection.Up : FacingDirection.Down;
    }

    private void HandleGravity()
    {
        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;
        else
            velocity.y += gravity * Time.deltaTime;
    }

    private void ApplyMovement()
    {
        float speed = (state == PlayerState.Running) ? moveSpeed * runMultiplier : moveSpeed;

        Vector3 horizontal = moveWorld * speed;
        Vector3 finalMovement = horizontal + velocity;

        controller.Move(finalMovement * Time.deltaTime);
    }

    private void UpdateLookPointer(Vector3 moveDirWorld)
    {
        if (lookPointer == null) return;

        if (moveDirWorld.sqrMagnitude > 0.0001f)
            lastLookDir = moveDirWorld.normalized;

        Vector3 offset = lastLookDir * interactDistance;
        offset.y = interactHeight;

        lookPointer.position = transform.position + offset;
    }

    private void GetCameraAxesOnPlane(out Vector3 forward, out Vector3 right)
    {
        forward = Vector3.forward;
        right = Vector3.right;

        if (cameraTransform != null)
        {
            forward = cameraTransform.forward;
            right = cameraTransform.right;
        }

        forward.y = 0f;
        right.y = 0f;

        if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward;
        if (right.sqrMagnitude < 0.0001f) right = Vector3.right;

        forward.Normalize();
        right.Normalize();
    }
}