using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Status : MonoBehaviour
{
    public TextMeshProUGUI statusText;

    public void UpdateStatusText(string text)
    {
        statusText.text = text;
    }
}
