using System;
using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField] private TurnQueieViewController turnQueieView;
    [SerializeField] private MenuFollow menu;
    [SerializeField] private MenuController menuController;

    public List<BattleUnit> currentTurn;
    public List<BattleUnit> nextTurn;

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

        SpawnSide(PlayerTeam, playerSlots, PlayerUnits, true);

        SpawnSide(EnemyTeam, enemySlots, EnemyUnits, false);

        ui.Build(PlayerUnits);

        triggerEncounter.LastEncounterEnemies = null;
        //foreach(BattleUnit e in PlayerUnits) e.View.SetShadow();
        foreach(BattleUnit p in PlayerUnits) p.View.HideImmediate();
   

        IsReady = true;
        StartBattle();
    }

    private void SpawnSide(List<PokemonInstance> team, Transform[] slots, List<BattleUnit> outUnits, bool isMyPokemon)
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
                // only alive pokemon
            if (!team[i].IsFainted)
            {
                var view = SpawnUnit(team[i], slots[i], isMyPokemon);
                if (view == null) continue;
                view.enterStyle = isMyPokemon ? EnterStyle.BallDrop : EnterStyle.WildFade;

                var unit = new BattleUnit
                {
                    isMypokemon = isMyPokemon,
                    Instance = team[i],
                    View = view,
                    Slot = slots[i],
                    HomePos = view.transform.position
                };
                outUnits.Add(unit);
            }

        }
    }

    public SpriteVisualPokemonInstance SpawnUnit(PokemonInstance instance, Transform slot, bool isMyPokemon)
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

        view.Initialize(instance, isMyPokemon);
        return view;
    }

    private void CleanupFaintedUnits()
    {
        CleanupList(PlayerUnits);
        CleanupList(EnemyUnits);
    }

    private void CleanupList(List<BattleUnit> units)
    {
        for (int i = 0; i < units.Count; i++)
        {
            var u = units[i];
            if (u == null || u.Instance == null) continue;

            if (!u.Instance.IsFainted) continue;

            if (u.View != null)
            {
                Destroy(u.View.gameObject);
                turnQueieView.Remove(u.Instance.id);
                u.View = null;
            }
        }
    }

    public void StartBattle()
    {
        if (!IsReady) return;
        StartCoroutine(StartBattleCo());
    }

    private IEnumerator StartBattleCo()
    {     
        foreach (BattleUnit u in EnemyUnits)
        {
            u.View.PlayEnter();
            yield return new WaitForSeconds(0.2f);
        }
        // sono comparsi un gruppo di pokemon

        yield return new WaitForSeconds(EnemyUnits[0].View.enterDuration);

        foreach (BattleUnit u in PlayerUnits)
        {
            u.View.PlayEnter();
            yield return new WaitForSeconds(0.3f);
        }
            

 
        yield return new WaitForSeconds(PlayerUnits[0].View.enterDuration);

        StartCoroutine(BattleLoop());
    }

    private IEnumerator BattleLoop()
    {


        while (!CheckBattleEnd())
        {
            //yield return new WaitForSeconds(0.5f);
        currentTurn = BattleUtilitis.ManageTurnQueue(PlayerUnits, EnemyUnits);
        turnQueieView.build(currentTurn);

            // add actionManagment, design how turns work, who play first and vice versa

            for (int i = 0; i < currentTurn.Count; i++)
            {
                currentTurn[i].Instance.DebugInfo();

                if (!currentTurn[i].Instance.IsFainted)
                {
                    if (currentTurn[i].isMypokemon)
                    {
                        moveTowardUnit(currentTurn[i]);

                        // ActionTurn ora aspetta la decisione del menu e ti riempie "action"
                        yield return ActionTurn(currentTurn[i]); // guarda cosa vuole dall'indice

                        // esegui l'azione scelta
                        yield return action.Execute();
                        resetPositionUnit(currentTurn[i]);
                        resetCardPosition(currentTurn[i]);
                        CleanupFaintedUnits();
                    }
                    else
                    {
                        moveTowardUnit(currentTurn[i]);
                        action = BattleUtilitis.enemyRandomChoice(currentTurn[i], PlayerUnits, EnemyUnits);
                        yield return action.Execute();
                        resetPositionUnit(currentTurn[i]);
                        CleanupFaintedUnits();
                    }
                    if (CheckBattleEnd()) break;
                    turnQueieView.Remove(currentTurn[i].Instance.id);
                }
                
                foreach(BattleUnit partyPk in PlayerUnits) ui.RefreshCard(partyPk);

                nextTurn = BattleUtilitis.ManageTurnQueue(PlayerUnits, EnemyUnits);
            }

            currentTurn = nextTurn;


            // il nemico scegli una mossa anche randomicamente per il momento
            // (in futuro: stesso schema ma autoselezione)
            // for (int i = 0; i < EnemyUnits.Count; i++) { ... }

            // scelgo l'ordine

            yield return null;
        }

        SceneManager.LoadScene("SampleScene");
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

    private IEnumerator ActionTurn(BattleUnit unit)
    {
        var t = unit.View.transform;

        action = null;

        yield return null;

        moveTowardUnit(unit);
        ui.ActiveSelectionCardById(unit, new Vector2(50f, 0f));

        menu.Show(unit.View.transform);

        menuController.Open(unit, EnemyUnits, PlayerUnits);

        while (!menuController.HasDecision)
            yield return null;

        // salva l'azione scelta
        action = menuController.Decision;

        // nascondi il menu (follow)
        menu.Hide();

        //t.position = unit.HomePos;
        //ui.ResetSelectionCardByIndex(unit, new Vector2(50f, 0f));
    }

    private void resetCardPosition(BattleUnit unit)
    {
        var t = unit.View.transform;
        ui.ResetSelectionCardByIndex(unit, new Vector2(50f, 0f)); 
        
        
    }

    private void resetPositionUnit(BattleUnit unit)
    {
        var t = unit.View.transform;
        t.position = unit.HomePos;
    }

    private void moveTowardUnit(BattleUnit unit)
    {
        var t = unit.View.transform;
        t.position = unit.HomePos + new Vector3( unit.isMypokemon ? 2f : -2f, 0f, 0f);
    }
}

[System.Serializable]
public class BattleUnit
{
    public bool isMypokemon;
    public PokemonInstance Instance;
    public SpriteVisualPokemonInstance View;
    public Transform Slot;
    public Vector3 HomePos;
}