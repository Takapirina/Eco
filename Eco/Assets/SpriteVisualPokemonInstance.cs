using System.Collections;
using UnityEngine;

public class SpriteVisualPokemonInstance : MonoBehaviour
{
    [SerializeField] private SpriteRenderer pokemonRenderer;
    [SerializeField] private GameObject targetIndicator;

    // flash
    private Coroutine flashRoutine;
    private Color baseColor = Color.white;

    public PokemonInstance Instance { get; private set; }

    private int currentFrameIndex = 0;
    private float frameTimer = 0f;

    private void Awake()
    {
        if (pokemonRenderer != null)
            baseColor = pokemonRenderer.color;
    }

    public void Initialize(PokemonInstance instance)
    {
        Instance = instance;

        pokemonRenderer.sprite = instance.IsShiny
            ? instance.BaseData.sprites_shiny[0]
            : instance.BaseData.sprites[0];

        float h = Instance.BaseData.height / 100;
        float s = Mathf.Log10(h + 1f) * 1.6f + 0.8f;

        pokemonRenderer.transform.localScale = Vector3.one * s;
    }

    public void SetTargeted(bool on)
    {
        // freccia/cubo
        if (targetIndicator != null)
            targetIndicator.SetActive(on);

        // lampeggio bianco
        if (on) StartFlash();
        else StopFlash();
    }

    private void StartFlash()
    {
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashCo());
    }

    private void StopFlash()
    {
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = null;

        if (pokemonRenderer != null)
            pokemonRenderer.color = baseColor;
    }

    private IEnumerator FlashCo()
    {
        // “blink” semplice e visibile
        while (true)
        {
            if (pokemonRenderer != null)
                pokemonRenderer.color = Color.white;
            yield return new WaitForSeconds(0.25f);

            if (pokemonRenderer != null)
                pokemonRenderer.color = new Color(0.75f, 0.75f, 0.00f, 1f);
            yield return new WaitForSeconds(0.25f);
        }
    }

    private void Update()
    {
        if (Instance == null) return;

        int len = Instance.IsShiny
            ? Instance.BaseData.sprites_shiny.Length
            : Instance.BaseData.sprites.Length;

        if (len == 0) return;

        float frameDuration = 1f / 12f;
        frameTimer += Time.deltaTime;

        if (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;

            currentFrameIndex++;
            if (currentFrameIndex >= len)
                currentFrameIndex = 0;

            pokemonRenderer.sprite = Instance.IsShiny
                ? Instance.BaseData.sprites_shiny[currentFrameIndex]
                : Instance.BaseData.sprites[currentFrameIndex];
        }
    }
}