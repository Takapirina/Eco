using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PokemonPortraitUi : MonoBehaviour
{
    public BattleUnit unit;
    [SerializeField] private Image portraitImage;
    [SerializeField] private Image backgroundImage;

    public void SetUnit(BattleUnit unit)
    {
        portraitImage.sprite = unit.Instance.IsShiny
            ? unit.Instance.BaseData.portrait_shiny
            : unit.Instance.BaseData.portrait;
    }

    public void SetSideColor(Color color)
    {
        backgroundImage.color = color;
    }

    public void Pop()
    {
        transform.DOKill();
        transform.DOScale(1f, 0.12f).From(0.9f).SetEase(Ease.OutBack);
    }


}