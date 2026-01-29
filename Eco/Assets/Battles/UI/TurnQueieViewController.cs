using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TurnQueieViewController : MonoBehaviour
{
    [Header("Position")]
    public RectTransform contentTransform;

    [Header("Prefabs")]
    public GameObject turnQueieElementPrefab;

    public Color colorParty;
    public Color colorEnemy;

    private List<BattleUnit> battleUnitsInQueie;
    private readonly List<PokemonPortraitUi> portraitsSpawned = new();

    public void build(List<BattleUnit> battleUnits)
    {
        battleUnitsInQueie = new List<BattleUnit>(battleUnits); // COPIA
        Rebuild();
    }

    private void Rebuild()
    {
        foreach (PokemonPortraitUi portrait in portraitsSpawned)
        {
            if (portrait != null)
            {
                portrait.transform.DOKill();
                Destroy(portrait.gameObject);
            }
        }

        portraitsSpawned.Clear();

        foreach (BattleUnit unit in battleUnitsInQueie)
        {
            PokemonPortraitUi portrait = Instantiate(turnQueieElementPrefab, contentTransform).GetComponent<PokemonPortraitUi>();
            portrait.unit = unit;
            portrait.SetUnit(unit);
            portrait.SetSideColor(unit.isMypokemon ? colorParty : colorEnemy);
            portraitsSpawned.Add(portrait);
        }

        for (int i = 0; i < portraitsSpawned.Count; i++)
        {
            PokemonPortraitUi p = portraitsSpawned[i];
            SpawnEffect(p);
        }
    }

        private IEnumerator SpawnEffect(PokemonPortraitUi p)
        {
            RectTransform rt = (RectTransform)p.transform;

            rt.DOKill();

            yield return new WaitForSeconds(0.1f);

            rt.DOScale(1f, 0.5f).From(0f).SetEase(Ease.OutBack);
            rt.DOAnchorPosX(rt.anchoredPosition.x, 0.75f)
            .SetEase(Ease.OutCubic);
        }

    public void Remove(Guid id)
    {
        battleUnitsInQueie.RemoveAll(unit => unit.Instance.id == id);
        Rebuild();
    }
}
