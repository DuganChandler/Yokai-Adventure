using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleScence : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Player") {
            Debug.Log("ENTERED");
            SceneManager.LoadScene("BattleScene");
        }
    }
}
