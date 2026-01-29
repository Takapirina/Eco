using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PokemonGenerator
{
    public static PokemonInstance GeneratePokemon(List<PokeEncounter> encounters)
    {
        if (encounters == null || encounters.Count == 0)
        {
            Debug.LogWarning("GeneratePokemon: lista encounters vuota o null.");
            return null;
        }

        // Clamp dei pesi (evita valori negativi o >100)
        int totalWeight = 0;
        for (int i = 0; i < encounters.Count; i++)
        {
            int w = Mathf.Clamp(encounters[i].percentageEncounter, 0, 100);
            totalWeight += w;
        }

        // Se nessun encounter ha peso > 0, fallback: scelta uniforme (o return null, come preferisci)
        if (totalWeight <= 0)
        {
            Debug.LogWarning("GeneratePokemon: tutti i percentageEncounter sono 0. Scelta uniforme come fallback.");
            return BuildPokemon(encounters[Random.Range(0, encounters.Count)]);
        }

        // Roll 1..totalWeight (inclusivo)
        int roll = Random.Range(1, totalWeight + 1);

        int cumulative = 0;
        for (int i = 0; i < encounters.Count; i++)
        {
            cumulative += Mathf.Clamp(encounters[i].percentageEncounter, 0, 100);
            if (roll <= cumulative)
                return BuildPokemon(encounters[i]);
        }

        // Non dovrebbe mai succedere, ma fallback ultra-safe
        return BuildPokemon(encounters[encounters.Count - 1]);
    }

    private static PokemonInstance BuildPokemon(PokeEncounter entry)
    {
        int minL = Mathf.Min(entry.minLevel, entry.maxLevel);
        int maxL = Mathf.Max(entry.minLevel, entry.maxLevel);

        int level = Random.Range(minL, maxL + 1);

        // Se vuoi: rimpiazza con la tua shiny chance reale
        bool isShiny = Random.Range(0, 5) < 1;

        var possibleMoves = entry.pkBase.learnable_moves
            .Where(lm => lm.level <= level)
            .OrderBy(lm => lm.level)
            .Select(lm => lm.move)
            .ToList();

        // ultime 4 mosse disponibili
        int take = Mathf.Min(4, possibleMoves.Count);
        var finalMoves = possibleMoves.Skip(Mathf.Max(0, possibleMoves.Count - take)).ToList();

        return new PokemonInstance(entry.pkBase, level, isShiny, finalMoves);
    }
}