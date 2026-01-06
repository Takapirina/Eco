// dentro a questo file trovi le classi che rappresentano le azioni che il giocatore puo fare in battaglia
/* 
 - attacca (una mossa che il pokemon conosce)
 - difendi (il pokemon si difende e ottiene piu difesa e mana a fine turno)
 - items (azione che permette di usare uno strumento dall'inventario)
 - run (esci dallo scontro)
*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// NB: NON chiamarla Action (confligge con System.Action). Meglio BattleAction.
public abstract class BattleAction
{
    public abstract void Execute();
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

    public override void Execute()
    {
        Debug.Log($"{pkUsers.Instance.BaseData.pk_name} usa {move.move_name} contro {targets.Count} target(s)");
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

    public override void Execute()
    {
        Debug.Log($"Pokemon: {pkusers.Instance.BaseData.pk_name} si difende, le sue statistiche di difesa aumentano");
    }
}

/** classe che indica l'azione di usare uno strumento dell'inventario*/
public class ItemsAction : BattleAction
{
    public override void Execute()
    {
        Debug.Log("DA Implementare, Usato Strumento");
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

    public override void Execute()
    {
        SceneManager.LoadScene("SampleScene");
    }
}