using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteVisual : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private bool useFlipForLeftRight = false; 

    [Header("Idle Frames")]
    [SerializeField] private Sprite[] idleDown;
    [SerializeField] private Sprite[] idleUp;
    [SerializeField] private Sprite[] idleLeft;
    [SerializeField] private Sprite[] idleRight;

    [Header("Walk Frames")]
    [SerializeField] private Sprite[] walkDown;
    [SerializeField] private Sprite[] walkUp;
    [SerializeField] private Sprite[] walkLeft;
    [SerializeField] private Sprite[] walkRight;

    [Header("Run Frames (opzionale, se vuoto usa il Walk)")]
    [SerializeField] private Sprite[] runDown;
    [SerializeField] private Sprite[] runUp;
    [SerializeField] private Sprite[] runLeft;
    [SerializeField] private Sprite[] runRight;

    [Header("FPS")]
    [SerializeField] private float idleFPS = 4f;
    [SerializeField] private float walkFPS = 6f;
    [SerializeField] private float runFPS = 10f;

    private SpriteRenderer spriteRenderer;

    private SpriteAnimState currentState = SpriteAnimState.Idle;
    public void setSpriteAnimState(SpriteAnimState newState) => SetState(newState);
    private FacingDirection currentDirection = FacingDirection.Down;

    private Sprite[] currentFrames;
    private float currentFPS;

    private int currentFrameIndex = 0;
    private float frameTimer = 0f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        RefreshAnimationSet();
    }

    private void Update()
    {
        if (currentFrames == null || currentFrames.Length == 0 || currentFPS <= 0f)
            return;

        float frameDuration = 1f / currentFPS;
        frameTimer += Time.deltaTime;

        if (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;

            currentFrameIndex++;
            if (currentFrameIndex >= currentFrames.Length)
                currentFrameIndex = 0;

            spriteRenderer.sprite = currentFrames[currentFrameIndex];
        }
    }

    public void SetState(SpriteAnimState newState)
    {
        if (newState == currentState) return;

        currentState = newState;
        ResetAnimation();
        RefreshAnimationSet();
    }

    public void SetDirection(FacingDirection newDir)
    {
        if (newDir == currentDirection) return;

        currentDirection = newDir;
        ResetAnimation();
        RefreshAnimationSet();
    }

    private void ResetAnimation()
    {
        frameTimer = 0f;
        currentFrameIndex = 0;
    }

    private void RefreshAnimationSet()
    {
        spriteRenderer.flipX = false;
        currentFrames = null;

        switch (currentState)
        {
            case SpriteAnimState.Idle:
                currentFPS = idleFPS;
                currentFrames = GetFramesForDirection(
                    idleDown, idleUp, idleLeft, idleRight
                );
                break;

            case SpriteAnimState.Walking:
                currentFPS = walkFPS;
                currentFrames = GetFramesForDirection(
                    walkDown, walkUp, walkLeft, walkRight
                );
                break;

            case SpriteAnimState.Running:
                currentFPS = runFPS;
                
                Sprite[] down = (runDown != null && runDown.Length > 0) ? runDown : walkDown;
                Sprite[] up   = (runUp   != null && runUp.Length > 0) ? runUp   : walkUp;
                Sprite[] left = (runLeft != null && runLeft.Length > 0) ? runLeft : walkLeft;
                Sprite[] right= (runRight!= null && runRight.Length > 0) ? runRight: walkRight;

                currentFrames = GetFramesForDirection(down, up, left, right);
                break;
        }

        if (currentFrames != null && currentFrames.Length > 0)
        {
            spriteRenderer.sprite = currentFrames[0];
        }
    }

    private Sprite[] GetFramesForDirection(Sprite[] down, Sprite[] up, Sprite[] left, Sprite[] right)
    {
        switch (currentDirection)
        {
            case FacingDirection.Down:
                return down;

            case FacingDirection.Up:
                return up;

            case FacingDirection.Right:
                spriteRenderer.flipX = false;
                return right;

            case FacingDirection.Left:
                if (useFlipForLeftRight)
                {
                    spriteRenderer.flipX = true;
                    return right;
                }
                else
                {
                    spriteRenderer.flipX = false;
                    return left;
                }
        }

        return down;
    }
}