using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class triggerencounter : MonoBehaviour
{

    void OnTriggerEnter(Collider other)
    {
        print("triggerencounter: OnTriggerEnter");
        SceneManager.LoadScene("CombactSystem");
    }

}
