using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName ="Card", menuName = "Domino/Card")]
public class DominoCard : ScriptableObject
{
    public Sprite cardVisual;
    public int topValue;
    public int bottomValue;
}
