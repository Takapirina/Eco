using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class triggerEncounter : MonoBehaviour
{
    // ✅ payload per passare i nemici alla scena battle
    public static List<PokemonInstance> LastEncounterEnemies;

    [Header("Possible Pokémon encounters")]
    [SerializeField] public List<PokeEncounter> possiblePokemons;
    [SerializeField] public int minPkCount = 1;
    [SerializeField] public int maxPkCount = 1;

    private bool _triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Player")) return;
        _triggered = true;

        // Genera nemici
        int pkCount = Random.Range(minPkCount, maxPkCount + 1);

        LastEncounterEnemies = new List<PokemonInstance>(pkCount);

        for (int i = 0; i < pkCount; i++)
        {
            var pk = PokemonGenerator.GeneratePokemon(possiblePokemons);
            if (pk != null) LastEncounterEnemies.Add(pk);
        }

        SceneManager.LoadScene("CombactSystem");
    }
}

[System.Serializable]
public class PokeEncounter
{
    public PokemonBase pkBase;
    public int minLevel;
    public int maxLevel;
    public int percentageEncounter; // from 1 to 100
}