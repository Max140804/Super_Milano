using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class PlayerDisplay : MonoBehaviour
{
    public TextMeshProUGUI playerNmae;

    public void SetPlayerInfo(Player _player)
    {
        playerNmae.text = _player.NickName;
    }
}
