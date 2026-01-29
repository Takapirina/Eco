using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "NewMove", menuName = "Move/Move Data")]
public class Move_base : ScriptableObject
{

    public string move_name;
    public int move_id;

    public PokemonType move_type;
    public MoveCategory move_category;

    public string move_description;

    public int potenza;
    public MoveTargets targetsType;
    public int precisione; // from 0 to 100
    public int manaCost; // new points to execute move

    /* 
        effetti secondari, sono classi distinte, 
        fallo come un array nel caso se la mossa e complessa 
        per riciclare effetti gia creati in precedenza
    */

    public int priority; // priorita della mossa, aggiunge speed valido fino al prossimo turno
    
    [Header("effetti visivi")]
    public VFX_DATA[] effettiVisivi;

    [Header("Effetti secondari")]
    public List<secondaryEffect> secondaryEffects;

}

[System.Serializable]
public class secondaryEffect
{
    [Header("Enumeratore che indica l'effetto generico o specifico")]
    public EffTypeCommon type;
    [Header("target - indica a chi applica il seguente effetto [utilizzatore/target]")]
    public ApplyTarget pokemonTarget;

    [Header("applyChache - percentuale di possibilitá di attivare l'effetto default 100%")]
    public int applyChache = 100;

    [Header("Momento in cui si attiva l'effetto secondario")]
    public ApplicationMoment applicationMoment;

    [Header("changerListStatistic - Lista di modificatori di statistiche")]
    public List<changheStats> changerListStatistic;

    [Header("Status - applicatore di status comme avv - par - burn etc")]
    public Status status;

    [Header("damagePercentage - percentuale di healing / recoil da 0 - 1.0 in base al danno applicato")]
    public float damagePercentage;
}

//--------------------------------------------------------------------------
//  classe di supporto per indicare le variazioni di statistiche coime array
//--------------------------------------------------------------------------
[System.Serializable]
public class changheStats{
    public Statistica statistica;
    public int livello;
}

[System.Serializable]
public enum ApplyTarget
{
    user, target
}

public enum EffTypeCommon
{
    healing,
    recoil,
    stun,
    status,
    changeStats,
}

public enum VFXMotion
{
    Static,
    UserToTarget,
    TargetToUser
}

public enum SpawnPosition
{
    head,
    center,
    feet
}

public enum MotionRender
{
    Move,    
    Stretch  
}

[System.Serializable]
public class VFX_DATA
{
    [Header("Oggetto con animazione")]
    public GameObject vfxPrefab;

    [Header("Dove nasce l'effetto")]
    public ApplyTarget anchor;

    [Header("Punto sul corpo")]
    public SpawnPosition spawnPos;

    [Header("Movimento dell'effetto")]
    public VFXMotion motion;

    public MotionRender render; 

    [Header("Timing")]
    public float delay = 0f;
    public float duration = 0.8f;
}

public enum ApplicationMoment
{
    before,
    after
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
    SelfT,
    SinglePartyT,
    AOEPartyT,
}