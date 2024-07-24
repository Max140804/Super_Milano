using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;
using System;
using UnityEngine.UI;
using TMPro;
using Firebase.Database;
using Firebase;
using Firebase.Extensions;

public class Events_Manager : MonoBehaviour
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

    private DatabaseReference databaseReference;
    private Dictionary<string, GameObject> instantiatedTournaments = new Dictionary<string, GameObject>();

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        });
    }

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

        databaseReference.Child("tournaments").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to retrieve data from database.");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                foreach (DataSnapshot tournamentSnapshot in snapshot.Children)
                {
                    CurrentTourParent.GetComponent<RectTransform>().sizeDelta = new Vector2(CurrentTourParent.GetComponent<RectTransform>().sizeDelta.x, CurrentTourParent.GetComponent<RectTransform>().sizeDelta.y + 350);
                }
            }
        });
    }

    public void GetTournamentsData()
    {
        parent.GetComponent<RectTransform>().sizeDelta = new Vector2(parent.GetComponent<RectTransform>().sizeDelta.x, 0);
        foreach (Transform child in parent.transform)
        {
            Destroy(child.gameObject);
        }

        databaseReference.Child("tournaments").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to retrieve data from database.");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                foreach (DataSnapshot tournamentSnapshot in snapshot.Children)
                {
                    string tournamentName = tournamentSnapshot.Key;
                    TournamentData tournamentData = JsonUtility.FromJson<TournamentData>(tournamentSnapshot.GetRawJsonValue());

                    // Check if the tournament has already been instantiated
                    if (!instantiatedTournaments.ContainsKey(tournamentName))
                    {
                        parent.GetComponent<RectTransform>().sizeDelta = new Vector2(parent.GetComponent<RectTransform>().sizeDelta.x, parent.GetComponent<RectTransform>().sizeDelta.y + 350);

                        // Instantiate the tournament prefab and set data
                        GameObject tournamentObject = Instantiate(tournamentPref, content.transform);
                        tournamentObject.transform.parent = content.transform;
                        Tournament tournamentComponent = tournamentObject.GetComponent<Tournament>();
                        tournamentComponent.SetData(tournamentData.name, tournamentData.type, tournamentData.players, tournamentData.bid);

                        // Add the instantiated tournament to the dictionary
                        instantiatedTournaments[tournamentName] = tournamentObject;
                    }
                }

                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
            }
        });
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
        string json = JsonUtility.ToJson(newTournamentData);

        databaseReference.Child("tournaments").Child(key).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Create(key);
            }
            else
            {
                Debug.LogError("Failed to create tournament.");
            }
        });
    }

    public void Create(string key)
    {
        databaseReference.Child("tournaments").Child(key).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                TournamentData tournamentData = JsonUtility.FromJson<TournamentData>(snapshot.GetRawJsonValue());

                // Instantiate the tournament prefab and set data
                GameObject tournamentObject = Instantiate(tournamentPref, content.transform);
                Tournament tournamentComponent = tournamentObject.GetComponent<Tournament>();
                tournamentComponent.SetData(tournamentData.name, tournamentData.type, tournamentData.players, tournamentData.bid);

                // Add the instantiated tournament to the dictionary
                instantiatedTournaments[key] = tournamentObject;

                Join(key);
                tourr.SetActive(true);
            }
            else
            {
                Debug.LogError("Failed to retrieve tournament data.");
            }
        });
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
        databaseReference.Child("tournaments").Child(tournamentKey).RemoveValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                // Remove the instantiated tournament from the dictionary and destroy it
                if (instantiatedTournaments.ContainsKey(tournamentKey))
                {
                    Destroy(instantiatedTournaments[tournamentKey]);
                    instantiatedTournaments.Remove(tournamentKey);
                }

                Debug.Log("Tournament deleted successfully");
            }
            else
            {
                Debug.LogError("Error deleting tournament");
            }
        });
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
