using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AIPlayerData : MonoBehaviour
{
    public TextMeshProUGUI playerNameText;
    [HideInInspector] public string playerName;

    private void Start()
    {
        string[] randomNames = {
            "Jake",
            "Jumoke",
            "Fashola",
            "Mark",
            "Maximilian",
            "Donald",
            "Fareed",
            "Kolade",
            "Jace",
            "Robert"
        };

        playerName = randomNames[Random.Range(0, randomNames.Length)];
        playerNameText.text = playerName;
    }
}
