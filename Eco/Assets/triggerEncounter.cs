using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class triggerEncounter : MonoBehaviour
{
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
        if (GetListForCurrentData().Count == 0) return;
        if (PartyContext.I == null || PartyContext.I.Party.Count == 0) return;
        if (PartyContext.I.Party.FindAll(pk => !pk.IsFainted).Count == 0) return;
        _triggered = true;

        int pkCount = Random.Range(minPkCount, maxPkCount + 1);

        LastEncounterEnemies = new List<PokemonInstance>(pkCount);

        for (int i = 0; i < pkCount; i++)
        {
            var pk = PokemonGenerator.GeneratePokemon(GetListForCurrentData());
            if (pk != null) LastEncounterEnemies.Add(pk);
        }

        SceneManager.LoadScene("CS_Style");
    }

    public List<PokeEncounter> GetListForCurrentData()
    {
        var filtered = new List<PokeEncounter>();

        if (TimeContext.I == null)
        {
            Debug.LogWarning("TimeContext mancante.");
            return filtered;
        }

        Season currentSeason = TimeContext.I.currentSeason;
        int currentHour = TimeContext.I.hour;

        foreach (var encounter in possiblePokemons)
        {
            bool seasonMatch = (encounter.season == Season.AllSesons) || (encounter.season == currentSeason);
            bool hourMatch = IsHourInRange(currentHour, encounter.startHour, encounter.endRange);

            if (seasonMatch && hourMatch)
                filtered.Add(encounter);
        }

        return filtered;
    }

    private static bool IsHourInRange(int currentHour, int startHour, int endHourExclusive)
    {
        currentHour = (currentHour % 24 + 24) % 24;
        startHour = (startHour % 24 + 24) % 24;
        endHourExclusive = (endHourExclusive % 24 + 24) % 24;

        if (startHour == endHourExclusive)
            return true;

        if (startHour < endHourExclusive)
            return currentHour >= startHour && currentHour < endHourExclusive;

        return currentHour >= startHour || currentHour < endHourExclusive;
}
}

[System.Serializable]
public class PokeEncounter
{
    public Season season;
    public int startHour;
    public int endRange;
    public PokemonBase pkBase;
    public int minLevel;
    public int maxLevel;
    public int percentageEncounter; // from 1 to 100
}