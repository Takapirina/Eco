using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "NewMove", menuName = "NPC/Move Data")]
public class Move_base : ScriptableObject
{

    public string move_name;

    public PokemonType move_type;
    public MoveCategory move_category;

    public string move_description;

    public int potenza;
    public MoveTargets targetsType;
    public int precisione; // from 0 to 100
    public int manaCost; // new points to execute move

    // secondary effects

    /* 
        effetti secondari, sono classi distinte, 
        fallo come un array nel caso se la mossa e complessa 
        per riciclare effetti gia creati in precedenza
    */

    public int priority; // priorita della mossa, aggiunge speed valido fino al prossimo turno

}

public enum MoveCategory
{
    Fisico,
    Speciale,
    Status,
}

public enum MoveTargets
{
    AOE,
    SingleT,
    MultiT,
    SelfT,
    SinglePartyT,
    AOEPartyT,
}