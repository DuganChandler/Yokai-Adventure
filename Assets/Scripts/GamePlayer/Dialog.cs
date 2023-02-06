using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this class is to control dialog wiht npcs, 
// made into class to expand upon it in future such as giving items and trade possibly
[System.Serializable]
public class Dialog
{
    [SerializeField] List<string> lines;

    public List<string> Lines {
        get { return lines; }
    }
    
}
