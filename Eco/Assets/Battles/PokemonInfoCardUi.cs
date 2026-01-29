using TMPro;
using UnityEngine;

public class PokemonInfoCardUI : MonoBehaviour
{
    public BattleUnit unit;
    public CanvasGroup canvasGroup;

    public TMP_Text nameText;
    public TMP_Text level;

    public TMP_Text hpText;
    public RectTransform hpFill;

    public TMP_Text manaText;
    public RectTransform manaFill;

    // Cache degli scale originali (del prefab)
    [HideInInspector] public Vector3 hpBaseScale;
    [HideInInspector] public Vector3 manaBaseScale;
}