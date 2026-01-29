using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class NpcAction
{
    public static void ExecuteHeal(List<PokemonInstance> party)
    {
        foreach(PokemonInstance p in party)
        {
            p.CurrentHP = p.MaxHP;
            p.CurrentMana = p.MaxMana;
        }
    }
}
