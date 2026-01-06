using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public List<PokemonInstance> PlayerTeam;
    public List<PokemonInstance> EnemyTeam;

    public List<BattleUnit> PlayerUnits = new();
    public List<BattleUnit> EnemyUnits = new();

    [SerializeField] private Transform[] playerSlots;
    [SerializeField] private Transform[] enemySlots;

    [SerializeField] public GameObject pokemonPrefab;


    [SerializeField] private BattleUI_info ui;
    [SerializeField] private MenuFollow menu;
    [SerializeField] private MenuController menuController;

    public bool IsReady { get; private set; } = false;

    BattleAction action;
    

    private void Start()
    {
        PlayerTeam = PartyContext.I.Party;
        EnemyTeam = triggerEncounter.LastEncounterEnemies;

        if (PlayerTeam == null || PlayerTeam.Count == 0)
        {
            Debug.LogError("BattleManager: PlayerTeam vuoto. Hai messo PlayerPartyContext nella scena iniziale e settato partySetup?");
            return;
        }

        if (EnemyTeam == null || EnemyTeam.Count == 0)
        {
            Debug.LogError("BattleManager: EnemyTeam vuoto. Sei entrato in battle senza passare dal trigger?");
            return;
        }

        if (pokemonPrefab == null)
        {
            Debug.LogError("BattleManager: pokemonPrefab non assegnato nell'Inspector.");
            return;
        }

        SpawnSide(PlayerTeam, playerSlots, PlayerUnits);

        SpawnSide(EnemyTeam, enemySlots, EnemyUnits);

        ui.Build(PlayerUnits);

        triggerEncounter.LastEncounterEnemies = null;

        IsReady = true;
        StartBattle();
    }

    private void SpawnSide(List<PokemonInstance> team, Transform[] slots, List<BattleUnit> outUnits)
    {
        outUnits.Clear();

        if (slots == null || slots.Length == 0)
        {
            Debug.LogError("BattleManager: slots non assegnati o vuoti in Inspector.");
            return;
        }

        int count = Mathf.Min(team.Count, slots.Length);

        for (int i = 0; i < count; i++)
        {
            if (slots[i] == null)
            {
                Debug.LogError($"BattleManager: slot NULL all'indice {i}. Controlla l'array in Inspector.");
                continue;
            }

            var view = SpawnUnit(team[i], slots[i]);
            if (view == null) continue;

            var unit = new BattleUnit
            {
                Instance = team[i],
                View = view,
                Slot = slots[i],
                HomePos = view.transform.position
            };

            outUnits.Add(unit);
        }
    }

    public SpriteVisualPokemonInstance SpawnUnit(PokemonInstance instance, Transform slot)
    {
        if (instance == null)
        {
            Debug.LogError("BattleManager.SpawnUnit: PokemonInstance NULL.");
            return null;
        }

        GameObject go = Instantiate(pokemonPrefab, slot.position, slot.rotation);

        var view = go.GetComponent<SpriteVisualPokemonInstance>();
        if (view == null)
        {
            Debug.LogError("BattleManager.SpawnUnit: sul prefab manca SpriteVisualPokemonInstance (sul ROOT).");
            return null;
        }

        view.Initialize(instance);
        return view;
    }

    public void StartBattle()
    {
        if (!IsReady) return;
        StartCoroutine(BattleLoop());
    }

    private IEnumerator BattleLoop()
    {
        while (!CheckBattleEnd())
        {
            yield return new WaitForSeconds(0.5f);

            for (int i = 0; i < PlayerUnits.Count; i++)
            {
                PlayerUnits[i].Instance.DebugInfo();

                // ActionTurn ora aspetta la decisione del menu e ti riempie "action"
                yield return ActionTurn(PlayerUnits[i], i);

                // esegui l'azione scelta
                action?.Execute();

                yield return new WaitForSeconds(0.5f);
            }


            // il nemico scegli una mossa anche randomicamente per il momento
            // (in futuro: stesso schema ma autoselezione)
            // for (int i = 0; i < EnemyUnits.Count; i++) { ... }

            // scelgo l'ordine

            // eseguo le azioni?

            yield return null;
        }
    }

    public bool CheckBattleEnd()
    {
        bool playerAllFainted = true;
        foreach (var pk in PlayerTeam)
        {
            if (pk.CurrentHP > 0) { playerAllFainted = false; break; }
        }

        bool enemyAllFainted = true;
        foreach (var pk in EnemyTeam)
        {
            if (pk.CurrentHP > 0) { enemyAllFainted = false; break; }
        }

        return playerAllFainted || enemyAllFainted;
    }

    private IEnumerator ActionTurn(BattleUnit unit, int index)
    {
        var t = unit.View.transform;

        action = null;

        yield return null;

        t.position = unit.HomePos + new Vector3(2f, 0f, 0f);
        ui.ActiveSelectionCardById(index, new Vector2(50f, 0f));

        menu.Show(unit.View.transform);

        menuController.Open(unit, EnemyUnits, PlayerUnits);

        while (!menuController.HasDecision)
            yield return null;

        // salva l'azione scelta
        action = menuController.Decision;

        // nascondi il menu (follow)
        menu.Hide();

        t.position = unit.HomePos;
        ui.ResetSelectionCardByIndex(index, new Vector2(50f, 0f));
    }
}

[System.Serializable]
public class BattleUnit
{
    public PokemonInstance Instance;
    public SpriteVisualPokemonInstance View;
    public Transform Slot;
    public Vector3 HomePos;
}