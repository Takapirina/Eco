using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public static class PokemonGenerator
{
    public static PokemonInstance GeneratePokemon(List<PokeEncounter> pokemons)
    {
        bool flag = false;
        PokemonInstance pk = null;

        while (!flag)
        {
            int encounterRoll = Random.Range(1,101);

            int indexRandom = Random.Range(0, pokemons.Count);

            if (encounterRoll <= pokemons[indexRandom].percentageEncounter)
            {
                flag = true;
                var entry = pokemons[indexRandom];

                int level = Random.Range(entry.minLevel, entry.maxLevel + 1);

                bool isShiny = Random.Range(0, 5) < 1;

                var possibleMoves = entry.pkBase.learnable_moves
                    .Where(lm => lm.level <= level)
                    .OrderBy(lm => lm.level)
                    .Select(lm => lm.move)
                    .ToList();

                var finalMoves = possibleMoves.Skip(Mathf.Max(0, possibleMoves.Count - 4)).ToList();

                pk = new PokemonInstance(entry.pkBase, level, isShiny, finalMoves);
            }
        }

        return pk;
    }
}