
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEditor.Scripting;
using UnityEngine;
using UnityEngine.Analytics;

public static class BattleUtilitis
{
    public static int calculateDmg(PokemonInstance pAtt, PokemonInstance pDef, Move_base move)
    {
        float aoeMult   = move.targetsType == MoveTargets.AOE ? 0.75f : 1f;
        float critMult  = Random.Range(1, pAtt.critLv == 0 ? 17 : 17/pAtt.critLv) == 1 ? 1.5f : 1f;
        float randMult  = Random.Range(0.9f, 1.1f);

        float levelFactor = (2f * pAtt.Level) / 5f + 2f;

        float attackStat = move.move_category == MoveCategory.Fisico
            ? pAtt.Attack
            : pAtt.SpAttack;

        float defenseStat = move.move_category == MoveCategory.Fisico
            ? pDef.Defense
            : pDef.SpDefense;

        if ( move.move_id == 100) return 40; // mossa ira di drago fissa 40 danni

        float baseDamage =
            levelFactor * move.potenza * (attackStat / defenseStat) / 50f + 2f;

        float typeMultiplier = getMultiplier( move.move_type, pDef.BaseData.type_1, pDef.BaseData.type_2);

        float finalDamage =
            baseDamage *
            aoeMult *
            critMult *
            typeMultiplier *
            randMult;

        return Mathf.Max(1, Mathf.FloorToInt(finalDamage));
    }

    // method to choose action for enemyUnit in random way, dumbies enemy
    public static AttackAction enemyRandomChoice(BattleUnit e, List<BattleUnit> party, List<BattleUnit> enemys)
    {
        if (e.Instance.Moves.Count == 0) return null;
        Move_base pendingMove = e.Instance.Moves[Random.Range(0, e.Instance.Moves.Count -1)];

            if (pendingMove.targetsType == MoveTargets.SelfT)
            {
                return new AttackAction(e, pendingMove, new List<BattleUnit> { e });
            }

            if (pendingMove.targetsType == MoveTargets.AOE)
            {
                List<BattleUnit> alivePk = party.Where( p => !p.Instance.IsFainted).ToList();
                return new AttackAction(e, pendingMove, party);
            }

            if (pendingMove.targetsType == MoveTargets.AOEPartyT)
            {
                List<BattleUnit> alivePk = enemys.Where( p => !p.Instance.IsFainted).ToList();
                return new AttackAction(e, pendingMove, enemys);
            }

            if (pendingMove.targetsType == MoveTargets.SingleT)
            {
                List<BattleUnit> alivePk = party.Where(p => !p.Instance.IsFainted).ToList();
                if (alivePk.Count == 0)
                    return null;

                int tgIndex = Random.Range(0, alivePk.Count);
                return new AttackAction(e, pendingMove, new List<BattleUnit> { alivePk[tgIndex] });
            }

            if (pendingMove.targetsType == MoveTargets.SinglePartyT)
            {
                List<BattleUnit> alivePk = enemys.Where( p => !p.Instance.IsFainted).ToList();
                int tgIndex = Random.Range(0, enemys.Count - 1);
                return new AttackAction(e, pendingMove, new List<BattleUnit>{ alivePk[tgIndex] });
            }

            return null;
    }

    // util where you may calculate the sequence unit in a turn
    public static List<BattleUnit> ManageTurnQueue(List<BattleUnit> party, List<BattleUnit> enemy)
    {
        var units = new List<BattleUnit>(party.Count + enemy.Count);
        units.AddRange(party);
        units.AddRange(enemy);

        // opzionale: togli KO / null
        units.RemoveAll(u => u == null || u.Instance == null || u.Instance.IsFainted);

        // dal più veloce al più lento
        units.Sort((a, b) => b.Instance.Speed.CompareTo(a.Instance.Speed));

        return units;
    }

    public static float getMultiplier(PokemonType atk, PokemonType def1, PokemonType def2)
    {
        float m1 = getSingle(atk, def1);
        float m2 = getSingle(atk, def2);
        return m1 * m2;
    }

    private static float getSingle(PokemonType atk, PokemonType def)
    {
        if (atk == PokemonType.None || def == PokemonType.None)
            return 1f;

        if (chart.TryGetValue((atk, def), out float mult))
            return mult;

        return 1f;
    }

    //---------------------------------------------
    // metodo che permette di applicare gli effetti
    //---------------------------------------------
    public static float timeDurationEffetc = 0f;

    public static void applyEffect(secondaryEffect eff, BattleUnit pk, int totDmg, MoveCategory mCategory)
    {
        if (Random.Range(0, 100) >= eff.applyChache) return;

        switch (eff.type)
        {
            case EffTypeCommon.changeStats:
            Debug.Log("cambio di statistica");
                foreach(changheStats change in eff.changerListStatistic)
                {
                    if (change.livello == 0)continue;
                    if (change.livello > 0)
                    {
                        pk.Instance.boostStats(change.statistica, change.livello);
                        Debug.Log("boost applicato");
                        timeDurationEffetc = pk.View.durationBoost;
                        pk.View.playBoostStatistic();
                    } 
                        else
                    {
                        pk.Instance.debufStats(change.statistica, change.livello);
                        Debug.Log("Debuff applicato");
                        pk.View.playDebuffStattistic();
                        timeDurationEffetc = pk.View.durationDebuff;
                    }
                }
            break;
            case EffTypeCommon.healing:
                if (mCategory == MoveCategory.Status) pk.Instance.Heal(Mathf.FloorToInt(pk.Instance.MaxHP * eff.damagePercentage));
                else 
                    pk.Instance.Heal(Mathf.Max(1, Mathf.FloorToInt(totDmg * eff.damagePercentage)));
                
                pk.View.playHealing();
                timeDurationEffetc = pk.View.durationHealing;
            break;
            case EffTypeCommon.recoil:
                pk.Instance.TakeDamage(Mathf.Max(1, Mathf.FloorToInt(totDmg * eff.damagePercentage)));
            break;
            case EffTypeCommon.status:
                if (eff.status == Status.None ) break;
                if (eff.status == Status.removeNegativeStatus)
                {
                    pk.Instance.removeStatus();
                    break;
                }
                pk.Instance.status = eff.status;
                // capire come applicare anche lo status visual essendo due enumeratori diversi
            break;
        }
    }

    public static List<secondaryEffect>  getEffByMoment(List<secondaryEffect> listEffect, ApplicationMoment moment)
    {
        List<secondaryEffect> effects = listEffect.FindAll(e => e.applicationMoment == moment);
        return effects;
    }

    public static void applyEffectsBymoment(BattleUnit pkUser, List<BattleUnit> Targets, Move_base move, int totDmg, ApplicationMoment moment)
    {
        
        List<secondaryEffect> effetcsFilteredbyMoment = getEffByMoment(move.secondaryEffects, moment);
        foreach(secondaryEffect eff in effetcsFilteredbyMoment)
        {
            Debug.Log($"Move {move.move_name} [{moment}] effect type={eff.type}, target={eff.pokemonTarget}, chance={eff.applyChache}");
            if (eff.pokemonTarget == ApplyTarget.target)
            {
                foreach(BattleUnit target in Targets) applyEffect(eff, target, totDmg, move.move_category);
            } else
            {
                applyEffect(eff, pkUser, totDmg, move.move_category);
            }
        }
    }



    private static readonly Dictionary<(PokemonType atk, PokemonType def), float> chart
        = new Dictionary<(PokemonType, PokemonType), float>
    {
        // NORMALE
        {(PokemonType.Normale, PokemonType.Roccia), 0.5f},
        {(PokemonType.Normale, PokemonType.Acciaio), 0.5f},

        {(PokemonType.Normale, PokemonType.Spettro), 0f},

        // LOTTA
        {(PokemonType.Lotta, PokemonType.Normale), 2f},
        {(PokemonType.Lotta, PokemonType.Roccia), 2f},
        {(PokemonType.Lotta, PokemonType.Acciaio), 2f},
        {(PokemonType.Lotta, PokemonType.Ghiaccio), 2f},
        {(PokemonType.Lotta, PokemonType.Buio), 2f},

        {(PokemonType.Lotta, PokemonType.Volante), 0.5f},
        {(PokemonType.Lotta, PokemonType.Veleno), 0.5f},
        {(PokemonType.Lotta, PokemonType.Coleottero), 0.5f},
        {(PokemonType.Lotta, PokemonType.Psico), 0.5f},
        {(PokemonType.Lotta, PokemonType.Folletto), 0.5f},

        {(PokemonType.Lotta, PokemonType.Spettro), 0f},

        // VOLANTE
        {(PokemonType.Volante, PokemonType.Lotta), 2f},
        {(PokemonType.Volante, PokemonType.Coleottero), 2f},
        {(PokemonType.Volante, PokemonType.Erba), 2f},

        {(PokemonType.Volante, PokemonType.Roccia), 0.5f},
        {(PokemonType.Volante, PokemonType.Acciaio), 0.5f},
        {(PokemonType.Volante, PokemonType.Elettro), 0.5f},

        // VELENO
        {(PokemonType.Veleno, PokemonType.Erba), 2f},
        {(PokemonType.Veleno, PokemonType.Folletto), 2f},

        {(PokemonType.Veleno, PokemonType.Veleno), 0.5f},
        {(PokemonType.Veleno, PokemonType.Terra), 0.5f},
        {(PokemonType.Veleno, PokemonType.Roccia), 0.5f},
        {(PokemonType.Veleno, PokemonType.Spettro), 0.5f},

        {(PokemonType.Veleno, PokemonType.Acciaio), 0f},

        // TERRA
        {(PokemonType.Terra, PokemonType.Veleno), 2f},
        {(PokemonType.Terra, PokemonType.Roccia), 2f},
        {(PokemonType.Terra, PokemonType.Acciaio), 2f},
        {(PokemonType.Terra, PokemonType.Fuoco), 2f},
        {(PokemonType.Terra, PokemonType.Elettro), 2f},

        {(PokemonType.Terra, PokemonType.Coleottero), 0.5f},
        {(PokemonType.Terra, PokemonType.Erba), 0.5f},

        {(PokemonType.Terra, PokemonType.Volante), 0f},

        // COLEOTTERO
        {(PokemonType.Coleottero, PokemonType.Erba), 2f},
        {(PokemonType.Coleottero, PokemonType.Psico), 2f},
        {(PokemonType.Coleottero, PokemonType.Buio), 2f},

        {(PokemonType.Coleottero, PokemonType.Lotta), 0.5f},
        {(PokemonType.Coleottero, PokemonType.Volante), 0.5f},
        {(PokemonType.Coleottero, PokemonType.Veleno), 0.5f},
        {(PokemonType.Coleottero, PokemonType.Spettro), 0.5f},
        {(PokemonType.Coleottero, PokemonType.Acciaio), 0.5f},
        {(PokemonType.Coleottero, PokemonType.Fuoco), 0.5f},
        {(PokemonType.Coleottero, PokemonType.Folletto), 0.5f},

        // SPETTRO
        {(PokemonType.Spettro, PokemonType.Spettro), 2f},
        {(PokemonType.Spettro, PokemonType.Psico), 2f},

        {(PokemonType.Spettro, PokemonType.Buio), 0.5f},

        {(PokemonType.Spettro, PokemonType.Normale), 0f},

        // ACCIAIO
        {(PokemonType.Acciaio, PokemonType.Roccia), 2f},
        {(PokemonType.Acciaio, PokemonType.Ghiaccio), 2f},
        {(PokemonType.Acciaio, PokemonType.Folletto), 2f},

        {(PokemonType.Acciaio, PokemonType.Acciaio), 0.5f},
        {(PokemonType.Acciaio, PokemonType.Fuoco), 0.5f},
        {(PokemonType.Acciaio, PokemonType.Acqua), 0.5f},
        {(PokemonType.Acciaio, PokemonType.Elettro), 0.5f},

        // FUOCO
        {(PokemonType.Fuoco, PokemonType.Coleottero), 2f},
        {(PokemonType.Fuoco, PokemonType.Acciaio), 2f},
        {(PokemonType.Fuoco, PokemonType.Erba), 2f},
        {(PokemonType.Fuoco, PokemonType.Ghiaccio), 2f},

        {(PokemonType.Fuoco, PokemonType.Roccia), 0.5f},
        {(PokemonType.Fuoco, PokemonType.Fuoco), 0.5f},
        {(PokemonType.Fuoco, PokemonType.Acqua), 0.5f},
        {(PokemonType.Fuoco, PokemonType.Drago), 0.5f},

        // ACQUA
        {(PokemonType.Acqua, PokemonType.Terra), 2f},
        {(PokemonType.Acqua, PokemonType.Roccia), 2f},
        {(PokemonType.Acqua, PokemonType.Fuoco), 2f},

        {(PokemonType.Acqua, PokemonType.Acqua), 0.5f},
        {(PokemonType.Acqua, PokemonType.Erba), 0.5f},
        {(PokemonType.Acqua, PokemonType.Drago), 0.5f},

        // ERBA
        {(PokemonType.Erba, PokemonType.Terra), 2f},
        {(PokemonType.Erba, PokemonType.Roccia), 2f},
        {(PokemonType.Erba, PokemonType.Acqua), 2f},

        {(PokemonType.Erba, PokemonType.Volante), 0.5f},
        {(PokemonType.Erba, PokemonType.Veleno), 0.5f},
        {(PokemonType.Erba, PokemonType.Acciaio), 0.5f},
        {(PokemonType.Erba, PokemonType.Fuoco), 0.5f},
        {(PokemonType.Erba, PokemonType.Erba), 0.5f},
        {(PokemonType.Erba, PokemonType.Drago), 0.5f},

        // ELETTRO
        {(PokemonType.Elettro, PokemonType.Volante), 2f},
        {(PokemonType.Elettro, PokemonType.Acqua), 2f},

        {(PokemonType.Elettro, PokemonType.Erba), 0.5f},
        {(PokemonType.Elettro, PokemonType.Elettro), 0.5f},
        {(PokemonType.Elettro, PokemonType.Drago), 0.5f},

        {(PokemonType.Elettro, PokemonType.Terra), 0f},

        // PSICO
        {(PokemonType.Psico, PokemonType.Lotta), 2f},
        {(PokemonType.Psico, PokemonType.Veleno), 2f},

        {(PokemonType.Psico, PokemonType.Acciaio), 0.5f},
        {(PokemonType.Psico, PokemonType.Psico), 0.5f},

        {(PokemonType.Psico, PokemonType.Buio), 0f},

        // GHIACCIO
        {(PokemonType.Ghiaccio, PokemonType.Volante), 2f},
        {(PokemonType.Ghiaccio, PokemonType.Terra), 2f},
        {(PokemonType.Ghiaccio, PokemonType.Erba), 2f},
        {(PokemonType.Ghiaccio, PokemonType.Drago), 2f},

        {(PokemonType.Ghiaccio, PokemonType.Acciaio), 0.5f},
        {(PokemonType.Ghiaccio, PokemonType.Fuoco), 0.5f},
        {(PokemonType.Ghiaccio, PokemonType.Acqua), 0.5f},
        {(PokemonType.Ghiaccio, PokemonType.Ghiaccio), 0.5f},

        // DRAGO
        {(PokemonType.Drago, PokemonType.Drago), 2f},

        {(PokemonType.Drago, PokemonType.Acciaio), 0.5f},

        {(PokemonType.Drago, PokemonType.Folletto), 0f},

        // BUIO
        {(PokemonType.Buio, PokemonType.Spettro), 2f},
        {(PokemonType.Buio, PokemonType.Psico), 2f},

        {(PokemonType.Buio, PokemonType.Lotta), 0.5f},
        {(PokemonType.Buio, PokemonType.Buio), 0.5f},
        {(PokemonType.Buio, PokemonType.Folletto), 0.5f},

        // FOLLETTO
        {(PokemonType.Folletto, PokemonType.Lotta), 2f},
        {(PokemonType.Folletto, PokemonType.Drago), 2f},
        {(PokemonType.Folletto, PokemonType.Buio), 2f},

        {(PokemonType.Folletto, PokemonType.Veleno), 0.5f},
        {(PokemonType.Folletto, PokemonType.Acciaio), 0.5f},
        {(PokemonType.Folletto, PokemonType.Fuoco), 0.5f},

    };

}