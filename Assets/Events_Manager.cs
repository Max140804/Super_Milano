using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;
using System;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class Events_Manager : MonoBehaviourPunCallbacks
{
    string playerId = "234353423";

    public Tournament tournamentPrefab;
    public GameObject tournamentPref;
    public GameObject parent;
    public GameObject CurrentTourParent;
    public GameObject content;

    public GameObject tourr;
    public TextMeshProUGUI tourrName;
    public MainMenu menu;

    public List<Text> names16UI;
    public List<Text> names8UI;
    public List<Text> names4UI;
    public List<Text> names2UI;

    private List<string> names16 = new List<string>();
    private List<string> names8 = new List<string>();
    private List<string> names4 = new List<string>();
    private List<string> names2 = new List<string>();

    public List<Text> ready16;
    public List<Text> ready8;
    public List<Text> ready4;
    public List<Text> ready2;

    public string currenttour;
    public string currentusername;

    public InputField tournamentname;
    public string tournamenttype;
    public int tournamentplayers;
    public float tournamentbid;

    private Dictionary<string, TournamentData> tournamentDataDictionary = new Dictionary<string, TournamentData>();

    public void changetype(string typ)
    {
        tournamenttype = typ;
    }
    public void changeplayers(int pla)
    {
        tournamentplayers = pla;
    }
    public void changebid(int bi)
    {
        if (bi == 42)
        {
            tournamentbid = 0.05f;
        }
        else
        {
            tournamentbid = bi;
        }
    }

    public void GetCurrentTournamentsData()
    {
        CurrentTourParent.GetComponent<RectTransform>().sizeDelta = new Vector2(CurrentTourParent.GetComponent<RectTransform>().sizeDelta.x, 0);

        foreach (Transform child in CurrentTourParent.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var tournamentEntry in tournamentDataDictionary)
        {
            Debug.Log(tournamentEntry.Key);
            CurrentTourParent.GetComponent<RectTransform>().sizeDelta = new Vector2(CurrentTourParent.GetComponent<RectTransform>().sizeDelta.x, CurrentTourParent.GetComponent<RectTransform>().sizeDelta.y + 350);
        }
    }

    public void GetTournamentsData()
    {
        parent.GetComponent<RectTransform>().sizeDelta = new Vector2(parent.GetComponent<RectTransform>().sizeDelta.x, 0);
        foreach (Transform child in parent.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var tournamentEntry in tournamentDataDictionary)
        {
            parent.GetComponent<RectTransform>().sizeDelta = new Vector2(parent.GetComponent<RectTransform>().sizeDelta.x, parent.GetComponent<RectTransform>().sizeDelta.y + 350);

            string tournamentName = tournamentEntry.Key;
            var tournamentData = tournamentEntry.Value;

            // Instantiate the tournament prefab and set data
            Tournament tournamentComponent = PhotonNetwork.Instantiate(tournamentPref.name, content.transform.position, Quaternion.identity).GetComponent<Tournament>();
            tournamentComponent.SetData(tournamentData.name, tournamentData.type, tournamentData.players, tournamentData.bid);
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
    }

    public void CreateTournament()
    {
        string key = HelperClass.Encrypt(tournamentname.text, playerId);
        var newTournamentData = new TournamentData
        {
            name = HelperClass.Encrypt(tournamentname.text, playerId),
            type = tournamenttype,
            players = tournamentplayers,
            bid = tournamentbid
        };
        tournamentDataDictionary[key] = newTournamentData;
        Create(key);
    }

    public void Create(string key)
    {
        var tournamentComponent = PhotonNetwork.Instantiate(tournamentPref.name, content.transform.position, Quaternion.identity).GetComponent<Tournament>();
        var tournamentData = tournamentDataDictionary[key];
        tournamentComponent.SetData(tournamentData.name, tournamentData.type, tournamentData.players, tournamentData.bid);
        PhotonNetwork.Instantiate(tournamentPref.name, content.transform.position, Quaternion.identity);
        tourr.SetActive(true);
        Join(key);
    }

    public void Join(string key)
    {
        Debug.Log("Joining tournament with key: " + key);
        currenttour = key;

        tourr.SetActive(true);

        tourrName.text = HelperClass.Decrypt(key, playerId);

        string playerName = currentusername;

        for (int i = 0; i < names16UI.Count; i++)
        {
            if (string.IsNullOrEmpty(names16UI[i].text))
            {
                names16UI[i].text = playerName;
                break;
            }
        }
    }

    public void DeleteTournament(string tournamentKey)
    {
        if (tournamentDataDictionary.Remove(tournamentKey))
        {
            Debug.Log("Tournament deleted successfully");
        }
        else
        {
            Debug.LogError("Error deleting tournament");
        }
    }
}

[System.Serializable]
public class TournamentData
{
    public string name;
    public string type;
    public int players;
    public float bid;
}
