using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PartyContext : MonoBehaviour
{
    public static PartyContext I { get; private set; }

    [Header("Party setup (Inspector)")]
    [SerializeField] private List<PartySlot> partySetup = new();

    [Header("Runtime party (DO NOT EDIT)")]
    public List<PokemonInstance> Party = new();

    private void Awake()
    {
        if (I != null)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);

        InitPartyFromInspector();
    }

    private void InitPartyFromInspector()
    {
        if (Party.Count > 0) return; // 🔒 NON si ricrea mai

        foreach (var slot in partySetup)
        {
            if (slot.pkBase == null) continue;

            var moves = slot.moves.Where(m => m != null).ToList();

            var pk = new PokemonInstance(
                slot.pkBase,
                slot.level,
                slot.isShiny,
                moves
            );

            Party.Add(pk);
        }

        Debug.Log($"[PLAYER PARTY INIT FROM INSPECTOR] count={Party.Count} listHash={Party.GetHashCode()}");
        for (int i = 0; i < Party.Count; i++)
        {
            Debug.Log($"  pk[{i}] hash={Party[i].GetHashCode()} name={Party[i].BaseData.pk_name}");
        }
    }
}

[System.Serializable]
public class PartySlot
{
    public PokemonBase pkBase;
    [Range(1, 100)] public int level = 5;
    public bool isShiny;
    public List<Move_base> moves = new();
}