using System.Collections.Generic;
using UnityEngine;

public class BattleUI_info : MonoBehaviour
{
    [SerializeField] private Transform container;
    [SerializeField] private PokemonInfoCardUI pokemonInfoPrefab;

    private List<BattleUnit> units;
    private readonly List<PokemonInfoCardUI> spawned = new();

    public void Build(List<BattleUnit> playerUnits)
    {
        units = playerUnits;
        Rebuild();
    }

    private void Rebuild()
    {
        Clear();

        foreach (BattleUnit unit in units)
        {
            PokemonInfoCardUI card = Instantiate(pokemonInfoPrefab, container);
            card.canvasGroup.alpha = 0.7f;

            // cache scale iniziali
            if (card.hpFill != null)
                card.hpBaseScale = card.hpFill.localScale;
            if (card.manaFill != null)
                card.manaBaseScale = card.manaFill.localScale;

            spawned.Add(card);
            FillCard(card, unit);
        }
    }

    private void Clear()
    {
        for (int i = 0; i < spawned.Count; i++)
        {
            if (spawned[i] != null)
                Destroy(spawned[i].gameObject);
        }
        spawned.Clear();
    }

    private static float SafeRatio(float current, float max)
    {
        if (max <= 0f) return 0f;
        return Mathf.Clamp01(current / max);
    }

    // 🔑 SOLO X, basato sullo scale iniziale
    private void SetBarScaleX(RectTransform bar, Vector3 baseScale, float current, float max)
    {
        if (bar == null) return;

        float ratio = SafeRatio(current, max);

        Vector3 s = bar.localScale;
        s.x = baseScale.x * ratio;
        bar.localScale = s;
    }

    private void FillCard(PokemonInfoCardUI card, BattleUnit unit)
    {
        var p = unit?.Instance;
        if (p == null) return;

        card.unit = unit;

        card.nameText.text = p.BaseData.name;
        card.level.text = $"Lv {p.Level}";

        card.hpText.text = $"{p.CurrentHP}/{p.MaxHP}";
        SetBarScaleX(card.hpFill, card.hpBaseScale, p.CurrentHP, p.MaxHP);

        card.manaText.text = $"{p.CurrentMana}/{p.MaxMana}";
        SetBarScaleX(card.manaFill, card.manaBaseScale, p.CurrentMana, p.MaxMana);
    }

    // ✅ QUI la modifica runtime (come volevi)
    public void RefreshCard(BattleUnit unit)
    {
        var card = spawned.Find(c => c != null && c.unit?.Instance != null && c.unit.Instance.id == unit.Instance.id);
        if (card == null) return;

        var p = unit.Instance;

        card.hpText.text = $"{p.CurrentHP}/{p.MaxHP}";
        SetBarScaleX(card.hpFill, card.hpBaseScale, p.CurrentHP, p.MaxHP);

        card.manaText.text = $"{p.CurrentMana}/{p.MaxMana}";
        SetBarScaleX(card.manaFill, card.manaBaseScale, p.CurrentMana, p.MaxMana);
    }

    // 🔒 METODI LASCIATI IDENTICI (promesso)
    public void ActiveSelectionCardById(BattleUnit p, Vector2 offset)
    {
        PokemonInfoCardUI spawn = spawned.Find(c => c.unit.Instance.id == p.Instance.id);
        if (!spawn) return;

        RectTransform rt = spawn.GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchoredPosition += offset;

        var cg = spawn.canvasGroup;
        cg.alpha = Mathf.Clamp01(1f);
    }

    public void ResetSelectionCardByIndex(BattleUnit p, Vector2 offset)
    {
        PokemonInfoCardUI spawn = spawned.Find(c => c.unit.Instance.id == p.Instance.id);
        if (!spawn) return;

        RectTransform rt = spawn.GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchoredPosition -= offset;

        var cg = spawn.canvasGroup;
        cg.alpha = Mathf.Clamp01(0.8f);
    }
}