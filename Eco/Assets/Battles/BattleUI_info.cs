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
            card.canvasGroup.alpha = Mathf.Clamp01(0.7f);
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

    private void FillCard(PokemonInfoCardUI card, BattleUnit unit)
    {
        var p = unit.Instance;
        if (p == null) return;

        card.nameText.text = p.BaseData.name;
        card.level.text = $"Lv {p.Level}";

        card.hpText.text = $"{p.CurrentHP}/{p.MaxHP}";
        card.hpFill.fillAmount = p.CurrentHP / (float)p.MaxHP;

        if (card.manaFill != null && card.manaText != null)
        {
            card.manaText.text = $"{p.CurrentMana}/{p.MaxMana}";
            card.manaFill.fillAmount = p.CurrentMana / (float)p.MaxMana;
        }
    }

    public void ActiveSelectionCardById(int index, Vector2 offset)
    {
        if (index < 0 || index >= spawned.Count) return;

        RectTransform rt = spawned[index].GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchoredPosition += offset;

        var cg = spawned[index].canvasGroup;
        cg.alpha = Mathf.Clamp01(1f);
    }

    public void ResetSelectionCardByIndex(int index, Vector2 offset)
    {
        if (index < 0 || index >= spawned.Count) return;

        RectTransform rt = spawned[index].GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchoredPosition -= offset;

        var cg = spawned[index].canvasGroup;
        cg.alpha = Mathf.Clamp01(0.8f);
    }
}