using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Healer : MonoBehaviour
{
    public IEnumerator Heal(Transform player, Dialog dialog, Action onFinished=null) {
        yield return StartCoroutine(DialogManager.Instance.ShowDialog(dialog));

        yield return Fader.i.FadeIn(1.5f);

        var playerParty = player.GetComponent<YokaiParty>();
        playerParty.YokaiList.ForEach(y => y.Heal());
        playerParty.PartyUpdated();

        yield return Fader.i.FadeOut(1.5f);
    }
}