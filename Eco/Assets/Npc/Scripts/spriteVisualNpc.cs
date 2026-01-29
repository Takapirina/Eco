using UnityEngine;

public class spriteVisualNpc : MonoBehaviour
{
    private npcController npcController;
    private SpriteRenderer spriteRenderer;

    [SerializeField] private Transform cameraTransform;

    private int currentFrameIndex = 0;
    private float frameTimer = 0f;

    private FacingDirection currentFacing = FacingDirection.Down;
    private Vector3 lastLookDirWorld = Vector3.forward;

    private void Awake()
    {
        npcController = GetComponentInParent<npcController>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void Start()
    {
        ApplySprite(0);
    }

    private void Update()
    {
        if (npcController == null || npcController.npcData == null) return;

        // 1) Prendi una direzione world dell’NPC
        Vector3 lookDirWorld = GetNpcLookDirWorld();

        if (lookDirWorld.sqrMagnitude > 0.0001f)
            lastLookDirWorld = lookDirWorld.normalized;

        // 2) Assi camera attuali (NON cachati)
        GetCameraAxesOnPlane(out var camForward, out var camRight);

        // 3) Facing relativo alla camera attuale
        var newFacing = GetFacingRelativeToCamera(lastLookDirWorld, camForward, camRight);

        if (newFacing != currentFacing)
        {
            currentFacing = newFacing;
            currentFrameIndex = 0;
            frameTimer = 0f;
            ApplySprite(currentFrameIndex);
        }

        // 4) Animazione (cicla i frame dell’array corrente)
        var frames = GetFramesForFacing(currentFacing);
        if (frames == null || frames.Length == 0) return;

        float frameDuration = 1f / Mathf.Max(0.01f, npcController.npcData.frameRate);
        frameTimer += Time.deltaTime;

        if (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            currentFrameIndex = (currentFrameIndex + 1) % frames.Length;
            ApplySprite(currentFrameIndex);
        }
    }

    private Vector3 GetNpcLookDirWorld()
    {
        // Se nel tuo npcController hai già una direzione, usa quella.
        // ESEMPI (scegline uno e decommenta in base a come l’hai chiamata):
        //
        // return npcController.MoveWorld; 
        // return npcController.FacingWorld;
        //
        // Fallback: guarda “in avanti” col transform del parent
        Transform t = npcController != null ? npcController.transform : transform.parent;
        if (t == null) t = transform;
        return t.forward; // oppure t.right ecc, dipende da come orienti i prefab
    }

    private void ApplySprite(int index)
    {
        if (npcController == null || npcController.npcData == null || spriteRenderer == null) return;

        var frames = GetFramesForFacing(currentFacing);
        if (frames == null || frames.Length == 0) return;

        index = Mathf.Clamp(index, 0, frames.Length - 1);
        spriteRenderer.sprite = frames[index];
    }

    private Sprite[] GetFramesForFacing(FacingDirection dir)
    {
        var data = npcController.npcData;
        return dir switch
        {
            FacingDirection.Up => data.up,
            FacingDirection.Down => data.down,
            FacingDirection.Left => data.left,
            FacingDirection.Right => data.right,
            _ => data.down
        };
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

    private FacingDirection GetFacingRelativeToCamera(Vector3 moveDirWorld, Vector3 camForwardOnPlane, Vector3 camRightOnPlane)
    {
        float side = Vector3.Dot(moveDirWorld, camRightOnPlane);
        float fwd = Vector3.Dot(moveDirWorld, camForwardOnPlane);

        if (Mathf.Abs(side) > Mathf.Abs(fwd))
            return (side > 0f) ? FacingDirection.Left : FacingDirection.Right;
        else
            return (fwd > 0f) ? FacingDirection.Up : FacingDirection.Down;
    }
}