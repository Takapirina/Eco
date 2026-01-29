using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Scheduler minimale per spawnare VFX con delay senza dover passare MonoBehaviour in giro.
/// Mettilo su un GameObject nella scena Battle (es. "VFXSystem").
/// </summary>
public class VFXScheduler : MonoBehaviour
{
    public static VFXScheduler I { get; private set; }
    /// <summary>
    /// Spawna un prefab VFX in una posizione con un delay opzionale e lo distrugge dopo duration.
    /// </summary>
    public void Spawn(GameObject prefab, Vector3 pos, float duration, float delay, Action<GameObject> onSpawned)
    {
        if (prefab == null) return;
        StartCoroutine(SpawnRoutine(prefab, pos, duration, delay, onSpawned));
    }

    private void Awake()
    {
        
        if (I != null && I != this)
        {
            Debug.LogWarning("Secondo VFXScheduler trovato, distruggo: " + gameObject.name);
            Destroy(gameObject);
            return;
        }

        I = this;
    }

    private IEnumerator SpawnRoutine(GameObject prefab, Vector3 pos, float duration, float delay, Action<GameObject> onSpawned)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        GameObject fx = Instantiate(prefab, pos, Quaternion.identity);
        onSpawned?.Invoke(fx);

        float d = duration > 0f ? duration : 2f;
        Destroy(fx, d);
    }


    public void Spawn(GameObject prefab, Vector3 pos, float duration, float delay)
    {
        if (prefab == null) return;
        StartCoroutine(SpawnRoutine(prefab, pos, duration, delay));
    }

    private IEnumerator SpawnRoutine(GameObject prefab, Vector3 pos, float duration, float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        GameObject fx = Instantiate(prefab, pos, Quaternion.identity);

        float d = duration > 0f ? duration : 2f;
        Destroy(fx, d);
    }

    public void SpawnMove(GameObject prefab, Vector3 start, Vector3 end, float duration, float delay)
    {
        if (prefab == null) return;
        StartCoroutine(SpawnMoveRoutine(prefab, start, end, duration, delay));
    }

    private IEnumerator SpawnMoveRoutine(GameObject prefab, Vector3 start, Vector3 end, float duration, float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        GameObject fx = Instantiate(prefab, start, Quaternion.identity);

        float d = duration > 0f ? duration : 0.5f;
        float t = 0f;

        while (t < d)
        {
            t += Time.deltaTime;
            float u = d <= 0.0001f ? 1f : Mathf.Clamp01(t / d);
            fx.transform.position = Vector3.Lerp(start, end, u);
            yield return null;
        }

        Destroy(fx);
    }
}