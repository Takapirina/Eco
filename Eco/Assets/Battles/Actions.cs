// dentro a questo file trovi le classi che rappresentano le azioni che il giocatore puo fare in battaglia
/* 
 - attacca (una mossa che il pokemon conosce)
 - difendi (il pokemon si difende e ottiene piu difesa e mana a fine turno)
 - items (azione che permette di usare uno strumento dall'inventario)
 - run (esci dallo scontro)
*/

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

// NB: NON chiamarla Action (confligge con System.Action). Meglio BattleAction.
public abstract class BattleAction
{
    public abstract IEnumerator Execute();
}

/** classe che indica l'azione di attaccare e usare una mossa*/
public class AttackAction : BattleAction
{
    public BattleUnit pkUsers;
    public Move_base move;
    public List<BattleUnit> targets;

    public AttackAction(BattleUnit pk, Move_base m, List<BattleUnit> targets)
    {
        this.pkUsers = pk;
        this.move = m;
        this.targets = targets;
    }

    public override IEnumerator Execute()
    {
        int totDamage = 0;
        if (!pkUsers.Instance.UseMana(move.manaCost))
            yield break;

        BattleUtilitis.applyEffectsBymoment(pkUsers, targets, move, totDamage, ApplicationMoment.before);

        foreach (BattleUnit target in targets)
        {
            if (move.effettiVisivi != null && target.View != null)
            {
                battleVFX.playAnimation(pkUsers, move, target);
            }
            // Logica (danno)
            int damage = BattleUtilitis.calculateDmg(pkUsers.Instance, target.Instance, move);
            totDamage += damage;
            target.Instance.TakeDamage(damage);
        }

        yield return new WaitForSeconds(battleVFX.calculateTime(move));

        BattleUtilitis.applyEffectsBymoment(pkUsers, targets, move, totDamage, ApplicationMoment.after);

        yield return new WaitForSeconds(BattleUtilitis.timeDurationEffetc);
    }
}

/** classe che indica l'azione di difendersi durante lo scontro*/
public class DefendAction : BattleAction
{
    /*
        TODO - incrementare le statistiche di difesa del pokemon e settargli lo stato come difesa
    */
    public BattleUnit pkusers;

    public DefendAction(BattleUnit pk)
    {
        this.pkusers = pk;
    }

    public override IEnumerator Execute()
    {
        Debug.Log($"Pokemon: {pkusers.Instance.BaseData.pk_name} si difende, le sue statistiche di difesa aumentano");
        yield return new WaitForSeconds(0);
    }
}

/** classe che indica l'azione di usare uno strumento dell'inventario*/
public class ItemsAction : BattleAction
{
    public override IEnumerator Execute()
    {
        Debug.Log("DA Implementare, Usato Strumento");
        yield return new WaitForSeconds(0);
    }
}

/** classe che indica l'azione di fuggire dallo scontro*/
public class RunAction : BattleAction
{
    /*
        TODO - aggiungere uno script che indica la possibilita di fuggire da uno scontro
        - dalle lotte contro boss e allenatori non si puo scappare
        - implementare in futuro un context globale per recuperare ultima scena e la posizione
    */

    public override IEnumerator Execute()
    {
        SceneManager.LoadScene("SampleScene");
        yield return new WaitForSeconds(0.5f);
    }
}