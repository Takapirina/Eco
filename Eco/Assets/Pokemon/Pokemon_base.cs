using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// idea first pokemon to create
// honedge/ shjieldon / axew /wooloo powniard 

[CreateAssetMenu(fileName = "NewPokemon", menuName = "NPC/Pokemon Data")]
public class PokemonBase : ScriptableObject
{
    public string pk_name;
    public string pokedex_description;
    public int pokedex_number;

    public float height; // in centimetri
    public float weight; // in kg

    public PokemonType type_1;
    public PokemonType type_2;

    public Sprite[] sprites;
    public Sprite[] sprites_shiny;
    public Sprite portrait;
    public Sprite portrait_shiny;

    public bool isFly;


    public GameObject vfxEnter;

    // base stats
    public int base_hp;
    public int base_attack;
    public int base_defense;
    public int base_sp_attack;
    public int base_sp_defense;
    public int base_speed;
    public int base_mana;

    public bool is_asexual;
    public int male_percentage; // 0 to 100

    public int exp_yeld;
    public int level_grow;
    public int catch_rate;

    // moves learnings

    public List<LearnableMove> learnable_moves;

    // abilities

}

[System.Serializable]
public class LearnableMove
{
    public Move_base move;
    public LearnMethod learnMethod;
    [Min(1)] public int level;
}

public enum LearnMethod { LevelUp, TM }

public enum PokemonType
{
    None,
    Normale,
    Fuoco,
    Lotta,
    Acqua,
    Volante,
    Erba,
    Veleno,
    Elettro,
    Terra,
    Psico,
    Roccia,
    Ghiaccio,
    Coleottero,
    Drago,
    Spettro,
    Buio,
    Acciaio,
    Folletto
}
