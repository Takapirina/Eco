using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PokemonInstance
{
    // --- Core ---
    public PokemonBase BaseData { get; private set; }
    public int Level { get; private set; }
    public Guid id { get; private set; }

    public bool IsShiny { get; private set; }

    // --- Stats calcolate ---
    public int MaxHP { get; private set; }
    public int Attack { get; private set; }
    public int Defense { get; private set; }
    public int SpAttack { get; private set; }
    public int SpDefense { get; private set; }
    public int Speed { get; private set; }

    public int MaxMana { get; private set; }

    public int attLv;       //[-6 to +6]
    public int deffLv;      //[-6 to +6]
    public int spAttLv;     //[-6 to +6]
    public int spDeffLv;    //[-6 to +6]
    public int speedLv;     //[-6 to +6]

    public int critLv;      //[0 to +3]


    // --- Runtime ---
    public int CurrentHP { get; set; }
    public int CurrentMana { get; set; }

    public bool IsFainted => CurrentHP <= 0;

    // --- State Animation ---
    public VisualPose visualPose;
    public UnitStatusVisual statusVisual;
    public Status status = Status.None;

    // --- Moves (importante: copia difensiva) ---
    private readonly List<Move_base> _moves = new List<Move_base>();
    public IReadOnlyList<Move_base> Moves => _moves;

    public PokemonInstance(PokemonBase pokemonBase, int level, bool isShiny, List<Move_base> moves)
    {
        if (pokemonBase == null) throw new ArgumentNullException(nameof(pokemonBase));
        if (level < 1) level = 1;

        id = Guid.NewGuid();

        BaseData = pokemonBase;
        Level = level;
        IsShiny = isShiny;

        RecalculateStats();
        RestoreFull();

        SetMoves(moves);
    }

    // --- Stats ---
    private void RecalculateStats()
    {
        // Nota: sto usando i TUOI calcoli identici, solo ordinati.
        MaxHP     = Mathf.FloorToInt(((2 * BaseData.base_hp         * Level) / 100f) + Level + 10);
        Attack    = Mathf.FloorToInt(((2 * BaseData.base_attack     * Level) / 100f) + 5);
        Defense   = Mathf.FloorToInt(((2 * BaseData.base_defense    * Level) / 100f) + 5);
        SpAttack  = Mathf.FloorToInt(((2 * BaseData.base_sp_attack  * Level) / 100f) + 5);
        SpDefense = Mathf.FloorToInt(((2 * BaseData.base_sp_defense * Level) / 100f) + 5);
        Speed     = Mathf.FloorToInt(((2 * BaseData.base_speed      * Level) / 100f) + 5);

        MaxMana   = Mathf.FloorToInt(((2 * BaseData.base_mana       * Level) / 100f) + Level + 10);

        // sicurezza: mai sotto 1
        MaxHP = Mathf.Max(1, MaxHP);
        MaxMana = Mathf.Max(0, MaxMana);
    }

    // --- Moves ---
    private void SetMoves(List<Move_base> moves)
    {
        _moves.Clear();

        if (moves == null) return;

        // Copia difensiva + filtra null (questo evita “mosse sparite” per riferimento esterno)
        foreach (var m in moves)
        {
            if (m != null) _moves.Add(m);
        }
    }

    // --- HP ---
    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;
        CurrentHP = Mathf.Max(0, CurrentHP - damage);
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
    }

    // --- Mana ---
    public bool CanUseMana(int cost)
    {
        if (cost <= 0) return true;
        return CurrentMana >= cost;
    }

    public bool UseMana(int cost)
    {
        if (!CanUseMana(cost)) return false;
        if (cost > 0) CurrentMana -= cost;
        return true;
    }

    public void RecoverMana(int amount)
    {
        if (amount <= 0) return;
        CurrentMana = Mathf.Min(MaxMana, CurrentMana + amount);
    }

    // --- stats ---
    public void boostStats(Statistica stat, int lv)
    {
        switch (stat)
        {
            case Statistica.att:
                attLv = Mathf.Min(6, attLv + lv);
            break;
            case Statistica.deff:
                deffLv = Mathf.Min(6, deffLv + lv);
            break;
            case Statistica.spAtt:
                spAttLv = Math.Min(6, spAttLv + lv);
            break;
            case Statistica.spDeff:
                spDeffLv = Math.Min(6, spDeffLv + lv);
            break;
            case Statistica.speed:
                speedLv = Math.Min(6, speedLv + lv);
            break;
            case Statistica.crit:
                critLv = Math.Min(3, critLv + lv);
            break;
        }
    }

    public void debufStats(Statistica stat, int lv)
    {
        switch (stat)
        {
            case Statistica.att:
                attLv = Mathf.Max(-6, attLv + lv);
            break;
            case Statistica.deff:
                deffLv = Mathf.Max(-6, deffLv + lv);
            break;
            case Statistica.spAtt:
                spAttLv = Math.Max(-6, spAttLv + lv);
            break;
            case Statistica.spDeff:
                spDeffLv = Math.Max(-6, spDeffLv + lv);
            break;
            case Statistica.speed:
                speedLv = Math.Max(-6, speedLv + lv);
            break;
            case Statistica.crit:
                critLv = Math.Max(-3, critLv + lv);
            break;
        }
    }

    public void removeStatus()
    {
        status = Status.None;
    }

    // --- Utility ---
    public void RestoreFull()
    {
        CurrentHP = MaxHP;
        CurrentMana = MaxMana;
        removeStatus();
    }

    public void DebugInfo()
    {
        Debug.Log($"Pokemon: {BaseData.pk_name} | Lv: {Level} | HP: {CurrentHP}/{MaxHP} | Mana: {CurrentMana}/{MaxMana} | Status: {status} | Shiny: {IsShiny}");

        for (int i = 0; i < _moves.Count; i++)
        {
            Debug.Log($"- Move {i}: {_moves[i].move_name}");
        }
    }
}

public enum Status
{
    None,
    removeNegativeStatus,
    Poison,
    Burn,
    Freeze,
    Sleep,
    Paralyze,
    Confusion
}

public enum Statistica
{
    att,
    deff,
    spAtt,
    spDeff,
    speed,
    crit
}