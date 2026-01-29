using System.Collections;
using UnityEngine;

public class SpriteVisualPokemonInstance : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private SpriteRenderer pokemonRenderer;
    [SerializeField] private GameObject targetIndicator;

    [Header("FX Anchors")]
    [SerializeField] public Transform FX_Center;
    [SerializeField] public Transform FX_Head;
    [SerializeField] public Transform FX_Feet;

    [Header("Clips FPS")]
    [SerializeField, Min(1f)] private float idleFps = 12f;
    [SerializeField, Min(1f)] private float hitFps = 12f;
    [SerializeField, Min(1f)] private float koFps = 12f;

    [Header("Enter Style")]
    [SerializeField] public EnterStyle enterStyle = EnterStyle.BallDrop;

    [Header("Enter Transition - BallDrop")]
    [SerializeField] public float enterDuration = 0.45f;
    [SerializeField] public float enterStartScale = 0.05f;
    [SerializeField] public float enterStartYOffset = 1.2f;
    [SerializeField] private AnimationCurve enterEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Enter Transition - Wild Shadow Fade")]
    [SerializeField, Range(0f, 1f)]
    private float wildShadowStrength = 0.80f; // 0.80 = molto scuro ma non nero pieno (stile "ombra")
    [SerializeField, Min(0.01f)]
    private float wildShadowFadeDuration = 0.35f;
    [SerializeField] private AnimationCurve wildShadowEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("VFX Prefabs (optional)")]
    [SerializeField] private GameObject vfxEnter;

    [SerializeField] private GameObject vfxHealing;
    [SerializeField] public float durationHealing;
    [SerializeField] private GameObject vfxBoost;
    [SerializeField] public float durationBoost;
    [SerializeField] private GameObject vfxDebuff;
    [SerializeField] public float durationDebuff;

    [SerializeField] private GameObject vfxParalysis;
    [SerializeField] private GameObject vfxSleep;
    [SerializeField] private GameObject vfxBurn;
    [SerializeField] private GameObject vfxPoison;


    private MaterialPropertyBlock mpb;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    public PokemonInstance Instance { get; private set; }

    // -------------------------
    // Pose/Status
    // -------------------------
    private VisualPose pose = VisualPose.Idle;
    private UnitStatusVisual status = UnitStatusVisual.None;

    private Sprite[] currentFrames;
    private float currentFps;
    private int frameIndex;
    private float frameTimer;
    private bool loop = true;
    private bool holdLastFrame = false;

    // -------------------------
    // Layered Color
    // base * status * shadow  (+ flash brightness)
    // -------------------------
    private Color baseColor = Color.white;
    private Color statusTint = Color.white;

    private bool shadowActive;
    private float shadowStrength01; // 0..1, dove 1 = "molto scuro", 0 = normale

    private bool flashActive;
    private Coroutine flashRoutine;

    // -------------------------
    // Transitions / routines
    // -------------------------
    private Coroutine transitionRoutine;
    private Coroutine oneShotRoutine;
    private Coroutine statusVfxRoutine;

    // cached transform (local, perché tu usi slot/scene)
    private Vector3 baseLocalPos;
    private Vector3 baseLocalScale;

    // -------------------------
    // Hidden (Pokemon-style)
    // -------------------------
    private bool isHidden = true;

    private void Awake()
    {
        if (pokemonRenderer != null)
            baseColor = pokemonRenderer.color;

        mpb = new MaterialPropertyBlock();

        baseLocalPos = transform.localPosition;
        baseLocalScale = transform.localScale;
    }

    public void Initialize(PokemonInstance instance, bool isMyPoke)
    {
        Instance = instance;

        ApplyScaleFromHeight();

        // salva "home" locali (se il prefab è già piazzato nello slot)
        baseLocalPos = instance.BaseData.isFly ? new Vector3(transform.localPosition.x, transform.localPosition.y + 0.5f, transform.localPosition.z) : transform.localPosition;
        transform.localPosition = baseLocalPos;
        baseLocalScale = transform.localScale;
        pokemonRenderer.flipX = isMyPoke;

        SetPose(VisualPose.Idle);
        SetStatus(UnitStatusVisual.None);

        SetShadow(true, wildShadowStrength);

        // prepara sprite/frame senza renderlo visibile
        ResetClipToFirstFrameSafe();
        ApplyFinalColor();
    }

    private void Update()
    {
        if (Instance == null || pokemonRenderer == null) return;

        TickClipAnimation();

        // se flash è attivo, il flashRoutine controlla il color
        if (flashRoutine == null)
            ApplyFinalColor();
    }

    // ==========================================================
    // Hidden API (Pokemon-like)
    // ==========================================================

    public void HideImmediate()
    {
        isHidden = true;

        if (pokemonRenderer != null)
            pokemonRenderer.enabled = false;

        if (targetIndicator != null)
            targetIndicator.SetActive(false);
    }

    private void RevealImmediate()
    {
        isHidden = false;

        if (pokemonRenderer != null)
            pokemonRenderer.enabled = true;
    }

    // ==========================================================
    // Shadow filter API (nero "80%" ecc)
    // ==========================================================

    /// <summary>
    /// Applica un filtro scuro (tipo "ombra"). strength01=0 => normale, 1 => molto scuro.
    /// </summary>
    public void SetShadow(bool on, float strength01 = 0.8f)
    {
        shadowActive = on;
        shadowStrength01 = Mathf.Clamp01(strength01);

        if (flashRoutine == null)
            ApplyFinalColor();
    }

    public void ClearShadow()
    {
        shadowActive = false;
        shadowStrength01 = 0f;

        if (flashRoutine == null)
            ApplyFinalColor();
    }

    // ==========================================================
    // Public API
    // ==========================================================

    public void SetTargeted(bool on)
    {
        if (targetIndicator != null)
            targetIndicator.SetActive(on);

        if (on) StartFlash();
        else StopFlash();
    }

    public void PlayEnter()
    {
        // durante enter non vogliamo lampeggi / indicatori
        SetTargeted(false);

        // diventa visibile SOLO ora
        RevealImmediate();

        StartTransition(enterStyle switch
        {
            EnterStyle.BallDrop => EnterBallDropCo(),
            EnterStyle.WildFade => EnterWildShadowFadeCo(),
            _ => EnterBallDropCo()
        });
    }

    public void FollowPath(Vector3[] localPoints, float duration, AnimationCurve ease = null)
    {
        StartTransition(FollowPathCo(localPoints, duration, ease));
    }

    public void PlayOneShotPose(VisualPose oneShot, VisualPose returnPose = VisualPose.Idle, bool force = true)
    {
        if (oneShotRoutine != null)
        {
            if (!force) return;
            StopCoroutine(oneShotRoutine);
        }
        oneShotRoutine = StartCoroutine(OneShotPoseCo(oneShot, returnPose));
    }

    public void SetPose(VisualPose newPose)
    {
        pose = newPose;
        ResolvePoseClip(newPose, out currentFrames, out currentFps, out loop, out holdLastFrame);
        ResetClipToFirstFrameSafe();
    }

    public void SetStatus(UnitStatusVisual newStatus)
    {
        status = newStatus;
        ResolveStatusOverlay(newStatus, out statusTint);

        if (statusVfxRoutine != null) StopCoroutine(statusVfxRoutine);
        statusVfxRoutine = StartCoroutine(StatusVfxLoopCo(newStatus));

        if (flashRoutine == null)
            ApplyFinalColor();
    }

    public void SpawnVfx(GameObject prefab, Transform anchor, float lifetime = 1.5f, Vector3 localOffset = default)
    {
        if (prefab == null || anchor == null) return;

        GameObject go = Instantiate(prefab, anchor);
        go.transform.localPosition = localOffset;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        if (lifetime > 0f) Destroy(go, lifetime);
    }

    // ==========================================================
    // Clips
    // ==========================================================

    private void TickClipAnimation()
    {
        if (currentFrames == null || currentFrames.Length == 0) return;
        if (currentFps <= 0f) return;

        frameTimer += Time.deltaTime;
        float frameDuration = 1f / currentFps;

        while (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;

            if (frameIndex < currentFrames.Length - 1)
            {
                frameIndex++;
            }
            else
            {
                if (loop) frameIndex = 0;
                else if (holdLastFrame) frameIndex = currentFrames.Length - 1;
            }
        }

        pokemonRenderer.sprite = currentFrames[frameIndex];
    }

    private void ResetClipToFirstFrameSafe()
    {
        frameIndex = 0;
        frameTimer = 0f;

        if (pokemonRenderer == null) return;

        if (currentFrames == null || currentFrames.Length == 0)
        {
            pokemonRenderer.sprite = GetFallbackSprite();
            return;
        }

        pokemonRenderer.sprite = currentFrames[0];
    }

    private Sprite GetFallbackSprite()
    {
        if (Instance?.BaseData == null) return null;

        Sprite[] arr = Instance.IsShiny ? Instance.BaseData.sprites_shiny : Instance.BaseData.sprites;
        if (arr == null || arr.Length == 0) return null;
        return arr[0];
    }

    private void ResolvePoseClip(VisualPose p, out Sprite[] frames, out float fps, out bool shouldLoop, out bool holdLast)
    {
        frames = null;
        fps = 12f;
        shouldLoop = true;
        holdLast = false;

        if (Instance?.BaseData == null) return;

        Sprite[] idle = Instance.IsShiny ? Instance.BaseData.sprites_shiny : Instance.BaseData.sprites;

        switch (p)
        {
            case VisualPose.Idle:
                frames = idle;
                fps = idleFps;
                shouldLoop = true;
                holdLast = false;
                break;

            case VisualPose.Hit:
                frames = (idle != null && idle.Length > 0) ? new Sprite[] { idle[0] } : null;
                fps = hitFps;
                shouldLoop = false;
                holdLast = true;
                break;

            case VisualPose.KO:
                frames = (idle != null && idle.Length > 0) ? new Sprite[] { idle[^1] } : null;
                fps = koFps;
                shouldLoop = false;
                holdLast = true;
                break;

            default:
                frames = idle;
                fps = idleFps;
                shouldLoop = true;
                holdLast = false;
                break;
        }
    }

    // ==========================================================
    // Transitions
    // ==========================================================

    private void StartTransition(IEnumerator co)
    {
        if (transitionRoutine != null) StopCoroutine(transitionRoutine);
        transitionRoutine = StartCoroutine(co);
    }

    private IEnumerator EnterBallDropCo()
    {
        // Pose stabile
        SetPose(VisualPose.Idle);
        ResetClipToFirstFrameSafe();

        // niente shadow in enter classico
        ClearShadow();

        // VFX enter (optional)
        SpawnVfx(vfxEnter, FX_Center, 2f);

        Vector3 startPos = baseLocalPos + Vector3.up * enterStartYOffset;
        Vector3 endPos = baseLocalPos;

        Vector3 startScale = baseLocalScale * enterStartScale;
        Vector3 endScale = baseLocalScale;

        float t = 0f;
        transform.localPosition = startPos;
        transform.localScale = startScale;

        while (t < enterDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / enterDuration);
            float e = enterEase != null ? enterEase.Evaluate(a) : a;

            transform.localPosition = Vector3.LerpUnclamped(startPos, endPos, e);
            transform.localScale = Vector3.LerpUnclamped(startScale, endScale, e);

            yield return null;
        }

        transform.localPosition = endPos;
        transform.localScale = endScale;

        transitionRoutine = null;
    }

    /// <summary>
    /// Wild: entra già visibile ma "oscurato", poi l'ombra svanisce -> colori naturali.
    /// </summary>
    private IEnumerator EnterWildShadowFadeCo()
    {
        // Pose stabile
        SetPose(VisualPose.Idle);
        ResetClipToFirstFrameSafe();

        // Applica shadow subito (tipo Pokémon selvatico)
        SetShadow(true, wildShadowStrength);
        yield return null;

        float t = 0f;
        while (t < wildShadowFadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / wildShadowFadeDuration);
            float e = wildShadowEase != null ? wildShadowEase.Evaluate(a) : a;

            //e=0 => shadowStrength, e=1 => 0
            float s = Mathf.Lerp(wildShadowStrength, 0f, e);
            SetShadow(true, s);

            yield return null;
        }

        // torna ai colori naturali
        ClearShadow();

        transitionRoutine = null;
    }

    private IEnumerator FollowPathCo(Vector3[] localPoints, float duration, AnimationCurve ease)
    {
        if (localPoints == null || localPoints.Length == 0) yield break;
        if (duration <= 0f) duration = 0.01f;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / duration);
            float e = ease != null ? ease.Evaluate(a) : a;

            Vector3 p = SamplePolyline(localPoints, e);
            transform.localPosition = baseLocalPos + p;

            yield return null;
        }

        transform.localPosition = baseLocalPos + localPoints[^1];
        transitionRoutine = null;
    }

    private static Vector3 SamplePolyline(Vector3[] pts, float t01)
    {
        if (pts.Length == 1) return pts[0];
        float scaled = t01 * (pts.Length - 1);
        int i = Mathf.FloorToInt(scaled);
        int j = Mathf.Clamp(i + 1, 0, pts.Length - 1);
        float u = scaled - i;
        return Vector3.LerpUnclamped(pts[i], pts[j], u);
    }

    // ==========================================================
    // One-shot
    // ==========================================================

    private IEnumerator OneShotPoseCo(VisualPose oneShot, VisualPose returnPose)
    {
        SetPose(oneShot);

        float frameDuration = (currentFps > 0f) ? (1f / currentFps) : 0.05f;

        if (currentFrames == null || currentFrames.Length == 0)
        {
            yield return new WaitForSeconds(0.1f);
            SetPose(returnPose);
            oneShotRoutine = null;
            yield break;
        }

        int last = currentFrames.Length - 1;
        while (frameIndex < last)
            yield return null;

        yield return new WaitForSeconds(frameDuration);

        SetPose(returnPose);
        oneShotRoutine = null;
    }

    // ==========================================================
    // Status overlay (tint + VFX loop)
    // ==========================================================

    private void ResolveStatusOverlay(UnitStatusVisual s, out Color tint)
    {
        switch (s)
        {
            case UnitStatusVisual.Paralyzed: tint = new Color(0.92f, 0.92f, 1.0f, 1f); break;
            case UnitStatusVisual.Asleep: tint = new Color(0.90f, 0.90f, 0.90f, 1f); break;
            case UnitStatusVisual.Burned: tint = new Color(1.00f, 0.92f, 0.92f, 1f); break;
            case UnitStatusVisual.Poisoned: tint = new Color(0.95f, 0.90f, 1.00f, 1f); break;
            case UnitStatusVisual.Healing: tint = new Color(0.90f, 1.00f, 0.90f, 1f); break;
            default: tint = Color.white; break;
        }
    }

    private IEnumerator StatusVfxLoopCo(UnitStatusVisual s)
    {
        while (true)
        {
            switch (s)
            {
                case UnitStatusVisual.None:
                    yield return null;
                    break;

                case UnitStatusVisual.Paralyzed:
                    SpawnVfx(vfxParalysis, FX_Center, 1.2f);
                    yield return new WaitForSeconds(1.0f);
                    break;

                case UnitStatusVisual.Asleep:
                    SpawnVfx(vfxSleep, FX_Head, 1.5f);
                    yield return new WaitForSeconds(1.2f);
                    break;

                case UnitStatusVisual.Burned:
                    SpawnVfx(vfxBurn, FX_Feet, 1.2f);
                    yield return new WaitForSeconds(0.9f);
                    break;

                case UnitStatusVisual.Poisoned:
                    SpawnVfx(vfxPoison, FX_Center, 1.2f);
                    yield return new WaitForSeconds(1.0f);
                    break;

                default:
                    yield return null;
                    break;
            }
        }
    }

    // ==========================================================
    // Status healing
    // ==========================================================

    public void playHealing()
    {
         StartCoroutine(healEffect());
    }

    public IEnumerator healEffect()
    {
        SpawnVfx(vfxHealing, FX_Center, durationHealing);
        yield return new WaitForSeconds(durationHealing);
    }

    // ==========================================================
    // StatisticVariation
    // ==========================================================

    public void playBoostStatistic()
    {
         StartCoroutine(boostStat());
    }

    private IEnumerator boostStat()
    {
        SpawnVfx(vfxBoost, FX_Feet, durationBoost);
        yield return new WaitForSeconds(durationBoost);
    }

    public void playDebuffStattistic()
    {
        StartCoroutine(debuffStat());
    }

    private IEnumerator debuffStat()
    {
        SpawnVfx(vfxDebuff, FX_Center, durationDebuff);
        yield return new WaitForSeconds(durationDebuff);
    }

    // ==========================================================
    // Flash (targeted)
    // ==========================================================

    private void StartFlash()
    {
        flashActive = true;
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashCo());
    }

    private void StopFlash()
    {
        flashActive = false;
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = null;

        ApplyFinalColor();
    }

    private IEnumerator FlashCo()
    {
        while (flashActive)
        {
            // flash = “brightness” sopra il colore finale
            Color baseFinal = ComputeFinalColor();
            pokemonRenderer.color = baseFinal * 1.15f;
            yield return new WaitForSeconds(0.20f);

            pokemonRenderer.color = baseFinal * 0.75f;
            yield return new WaitForSeconds(0.20f);
        }
    }

    // ==========================================================
    // Color layering
    // ==========================================================

    private void ApplyFinalColor()
    {
        if (pokemonRenderer == null) return;

        Color c = ComputeFinalColor();

        pokemonRenderer.GetPropertyBlock(mpb);
        mpb.SetColor(BaseColorId, c);
        pokemonRenderer.SetPropertyBlock(mpb);

        // opzionale: tienilo uguale per compatibilità con shader sprite
        pokemonRenderer.color = c;
    }

    private Color ComputeFinalColor()
    {
        Color c = baseColor;

        // status
        c = MultiplyColor(c, statusTint);

        // shadow filter: scurisce moltiplicando per (1 - strength)
        if (shadowActive && shadowStrength01 > 0f)
        {
            float m = Mathf.Clamp01(1f - shadowStrength01); // strength=0.8 => m=0.2
            Color shadowMul = new Color(m, m, m, 1f);
            c = MultiplyColor(c, shadowMul);
        }

        return c;
    }

    private static Color MultiplyColor(Color a, Color b)
        => new Color(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a);

    // ==========================================================
    // Scale from height
    // ==========================================================

    private void ApplyScaleFromHeight()
    {
        if (Instance?.BaseData == null || pokemonRenderer == null) return;

        float h = Instance.BaseData.height / 100f;
        float s = Mathf.Log10(h + 1f) * 1.6f + 0.8f;

        // scala il renderer, non il GO (così enter muove/scala il GO senza “cambiare taglia”)
        pokemonRenderer.transform.localScale = Vector3.one * s;
    }
}

public enum VisualPose
{
    Idle,
    Hit,
    KO
}

public enum EnterStyle
{
    BallDrop,
    WildFade
}

public enum UnitStatusVisual
{
    None,
    Paralyzed,
    Asleep,
    Burned,
    Poisoned,
    Healing
}