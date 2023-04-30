using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSetting : MonoBehaviour
{
    [SerializeField] Color highlightedColor;
    public Color HighlightedColor => highlightedColor;

    public static GlobalSetting i { get; private set; }

    private void Awake() {
        i = this;
    }
}
