using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
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

    public void SetTournamentType(string type)
    {
        tournamenttype = type;
    }

    public void SetTournamentBid(float bid)
    {
        tournamentbid = bid;
    }

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
            ListenForTournamentChanges();
            InvokeRepeating("CheckForExpiredTournaments", 0, 3600); // Check every hour
        });
    }

    private void ListenForTournamentChanges()
    {
        databaseReference.Child("tournaments").ChildAdded += HandleTournamentAdded;
        databaseReference.Child("tournaments").ChildRemoved += HandleTournamentRemoved;
    }

    private void HandleTournamentAdded(object sender, ChildChangedEventArgs e)
    {
        if (e.DatabaseError != null)
        {
            Debug.LogError(e.DatabaseError.Message);
            return;
        }

        DataSnapshot snapshot = e.Snapshot;
        string tournamentName = snapshot.Key;

        if (snapshot.Exists)
        {
            TournamentData tournamentData = JsonUtility.FromJson<TournamentData>(snapshot.GetRawJsonValue());

            // Check if the tournament has already been instantiated
            if (!instantiatedTournaments.ContainsKey(tournamentName))
            {
                parent.GetComponent<RectTransform>().sizeDelta = new Vector2(parent.GetComponent<RectTransform>().sizeDelta.x, parent.GetComponent<RectTransform>().sizeDelta.y + 350);

                // Instantiate the tournament prefab and set data
                GameObject tournamentObject = Instantiate(tournamentPref, content.transform);
                tournamentObject.transform.parent = content.transform;
                Tournament tournamentComponent = tournamentObject.GetComponent<Tournament>();
                tournamentComponent.SetData(tournamentname.text, tournamentData.type, tournamentData.players, tournamentData.bid);

                // Add the instantiated tournament to the dictionary
                instantiatedTournaments[tournamentName] = tournamentObject;
            }
        }
    }

    private void HandleTournamentRemoved(object sender, ChildChangedEventArgs e)
    {
        if (e.DatabaseError != null)
        {
            Debug.LogError(e.DatabaseError.Message);
            return;
        }

        DataSnapshot snapshot = e.Snapshot;
        string tournamentName = snapshot.Key;

        if (instantiatedTournaments.ContainsKey(tournamentName))
        {
            Destroy(instantiatedTournaments[tournamentName]);
            instantiatedTournaments.Remove(tournamentName);
        }
    }

    private void HandleTournamentUpdate(string tournamentName, TournamentData tournamentData)
    {
        // Update the existing tournament
        if (instantiatedTournaments.ContainsKey(tournamentName))
        {
            Tournament tournamentComponent = instantiatedTournaments[tournamentName].GetComponent<Tournament>();
            tournamentComponent.SetData(tournamentname.text, tournamentData.type, tournamentData.players, tournamentData.bid);
        }
    }

    public void CreateTournament()
    {
        if(tournamentname.text.Length <= 0)
        {
            menu.errorpanel.SetActive(true);
            menu.errorpanel_text.text = "Invalid TournamentName";
            return;
        }

        string key = HelperClass.Encrypt(tournamentname.text, playerId);
        var newTournamentData = new TournamentData
        {
            name = HelperClass.Encrypt(tournamentname.text, playerId),
            type = tournamenttype,
            players = 16,
            bid = tournamentbid,
            createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
        string json = JsonUtility.ToJson(newTournamentData);

        databaseReference.Child("tournaments").Child(key).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Tournament created successfully.");
            }
            else
            {
                Debug.LogError("Failed to create tournament.");
            }
        });
    }

    public void Join(string key)
    {
        Debug.Log("Joining tournament with key: " + key);
        currenttour = key;
        tourr.SetActive(true);

        tourrName.text = HelperClass.Decrypt(key, playerId);

        string playerName = PhotonNetwork.NickName;

        // Check if the player is already in the list
        bool playerAlreadyJoined = false;
        foreach (Text nameText in names16UI)
        {
            if (nameText.text == playerName)
            {
                playerAlreadyJoined = true;
                break;
            }
        }

        // If the player is not already in the list, add them
        if (!playerAlreadyJoined)
        {
            for (int i = 0; i < names16UI.Count; i++)
            {
                if (string.IsNullOrEmpty(names16UI[i].text))
                {
                    names16UI[i].text = playerName;
                    break;
                }
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

    private void CheckForExpiredTournaments()
    {
        databaseReference.Child("tournaments").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to retrieve tournaments from database.");
                return;
            }

            DataSnapshot snapshot = task.Result;
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            foreach (DataSnapshot tournamentSnapshot in snapshot.Children)
            {
                TournamentData tournamentData = JsonUtility.FromJson<TournamentData>(tournamentSnapshot.GetRawJsonValue());
                long createdAt = tournamentData.createdAt;

                // Check if 2 days (172800 seconds) have passed
                if (currentTime - createdAt > 172800)
                {
                    string tournamentKey = tournamentSnapshot.Key;
                    DeleteTournament(tournamentKey);
                }
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
    public long createdAt;
}
