using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using DG.Tweening;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class DoTwennProve : MonoBehaviour
{

    [SerializeField] private Transform[] boxes;
    [SerializeField] private Transform box;

    [SerializeField] private Transform portrait, mask;
    [SerializeField] private float duration = 0.35f;

    // Start is called before the first frame update
    void Start()
    {
        //.SetLoops(-1, LoopType.Yoyo) -> fa un loop infinito dell'animazione
        box.DOLocalMoveX(300f, duration).SetEase(Ease.InOutSine);
        box.DOScale(new Vector3(3f, 3f, 3f), duration).SetEase(Ease.InOutSine);

    }

    public IEnumerator spawneffect()
    {
        yield return new WaitForSeconds(0.1f);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
