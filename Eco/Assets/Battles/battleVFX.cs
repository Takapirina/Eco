using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// Utility statica per gestire gli effetti visivi (VFX) delle mosse in battaglia.
/// - Non contiene logica di gioco (danno, cure, stati).
/// - Spawna prefab VFX in posizioni corrette e rispetta timing (delay/duration) tramite VFXScheduler.
/// </summary>
public static class battleVFX
{
    public static void playAnimation(BattleUnit user, Move_base move, BattleUnit target)
    {
        if (move == null || move.effettiVisivi == null || move.effettiVisivi.Length == 0)
            return;

        if (VFXScheduler.I == null)
        {
            Debug.LogError("battleVFX: manca VFXScheduler in scena. Aggiungi un GameObject 'VFXSystem' con componente VFXScheduler.");
            return;
        }

        foreach (VFX_DATA eff in move.effettiVisivi)
        {
            if (!IsValidEffect(eff)) continue;

            BattleUnit anchorUnit = (eff.anchor == ApplyTarget.user) ? user : target;
            Vector3 startPos = getPositionBySpawn(anchorUnit, eff.spawnPos);

            if (eff.motion == VFXMotion.Static)
            {
                VFXScheduler.I.Spawn(eff.vfxPrefab, startPos, eff.duration, eff.delay);
                continue;
            }

            Vector3 endPos = startPos;

            if (eff.motion == VFXMotion.UserToTarget)
            {
                endPos = getPositionBySpawn(target, SpawnPosition.center);
            }

            if (eff.motion == VFXMotion.TargetToUser)
            {
                endPos = getPositionBySpawn(user, SpawnPosition.center);
            }
                

            if (eff.render == MotionRender.Move)
            {
                VFXScheduler.I.SpawnMove(eff.vfxPrefab, startPos, endPos, eff.duration, eff.delay);
            }
            if (eff.render == MotionRender.Stretch)
            {
                VFXScheduler.I.Spawn(eff.vfxPrefab, startPos, eff.duration, eff.delay, (fx) =>
                {
                    SetupStretch(fx, startPos, endPos);
                });
            }
        }
    }

    public static float calculateTime(Move_base move)
    {
        float maxTime = 0f;
        if (move == null || move.effettiVisivi == null) return 0f;

        foreach (VFX_DATA eff in move.effettiVisivi)
        {
            if (eff == null) continue;
            float t = Mathf.Max(0f, eff.delay) + Mathf.Max(0f, eff.duration);
            if (t > maxTime) maxTime = t;
        }

        return maxTime;
    }
    
    public static Vector3 getPositionBySpawn(BattleUnit unit, SpawnPosition spawn)
    {
        if (unit == null || unit.View == null) return Vector3.zero;

        Transform fallback = unit.View.transform;

        switch (spawn)
        {
            case SpawnPosition.center:
                return unit.View.FX_Center != null ? unit.View.FX_Center.position : fallback.position;

            case SpawnPosition.head:
                return unit.View.FX_Head != null ? unit.View.FX_Head.position : fallback.position;

            case SpawnPosition.feet:
                return unit.View.FX_Feet != null ? unit.View.FX_Feet.position : fallback.position;

            default:
                return fallback.position;
        }
    }

    private static bool IsValidEffect(VFX_DATA eff)
    {
        if (eff == null) return false;
        if (eff.vfxPrefab == null) return false;
        if (VFXScheduler.I == null) return false;
        return true;
    }

    /// <summary>
    /// Orienta e allunga un prefab tra due punti (beam).
    /// Assunzione: il beam è lungo sull'asse X del prefab.
    /// </summary>
    private static void SetupStretch(GameObject fx, Vector3 start, Vector3 end)
    {
        Vector3 dir = end - start;
        float dist = dir.magnitude;
        if (dist < 0.0001f) dist = 0.0001f;

        fx.transform.position = (start + end) * 0.5f;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        fx.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Vector3 s = fx.transform.localScale;
        s.x = dist;
        fx.transform.localScale = s;
    }
}