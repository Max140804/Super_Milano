using UnityEngine;
using TMPro;

public class DominoTile : MonoBehaviour
{
    public TextMeshProUGUI valueText; // Reference to the TextMesh component

    public void SetValue(int value)
    {
        valueText.text = value.ToString();
    }
}
