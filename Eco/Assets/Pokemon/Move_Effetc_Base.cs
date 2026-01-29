using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class Move_Effect_Base : ScriptableObject
{
    public abstract void Apply(BattleUnit pk, int dmg);
}