using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PokemonInstance
{
    // --- Core ---
    public PokemonBase BaseData { get; private set; }
    public int Level { get; private set; }

    public bool IsShiny { get; private set; }
    public Status Status { get; private set; } = Status.None;

    // --- Stats calcolate ---
    public int MaxHP { get; private set; }
    public int Attack { get; private set; }
    public int Defense { get; private set; }
    public int SpAttack { get; private set; }
    public int SpDefense { get; private set; }
    public int Speed { get; private set; }

    public int MaxMana { get; private set; }

    // --- Runtime ---
    public int CurrentHP { get; private set; }
    public int CurrentMana { get; private set; }

    public bool IsFainted => CurrentHP <= 0;

    // --- Moves (importante: copia difensiva) ---
    private readonly List<Move_base> _moves = new List<Move_base>();
    public IReadOnlyList<Move_base> Moves => _moves;

    public PokemonInstance(PokemonBase pokemonBase, int level, bool isShiny, List<Move_base> moves)
    {
        if (pokemonBase == null) throw new ArgumentNullException(nameof(pokemonBase));
        if (level < 1) level = 1;

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

    // --- Status ---
    public void ApplyStatus(Status newStatus) => Status = newStatus;
    public void CureStatus() => Status = Status.None;

    // --- Utility ---
    public void RestoreFull()
    {
        CurrentHP = MaxHP;
        CurrentMana = MaxMana;
        Status = Status.None;
    }

    public void DebugInfo()
    {
        Debug.Log($"Pokemon: {BaseData.pk_name} | Lv: {Level} | HP: {CurrentHP}/{MaxHP} | Mana: {CurrentMana}/{MaxMana} | Status: {Status} | Shiny: {IsShiny}");

        for (int i = 0; i < _moves.Count; i++)
        {
            Debug.Log($"- Move {i}: {_moves[i].move_name}");
        }
    }
}

public enum Status
{
    None,
    Poison,
    Burn,
    Freeze,
    Sleep,
    Paralyze,
    Confusion
}