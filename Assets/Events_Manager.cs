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
    public GameObject content;

    public GameObject tourr;
    public TextMeshProUGUI tourrName;
    public MainMenu menu;

    public List<Text> names16UI;
    public List<Text> names8UI;
    public List<Text> names4UI;
    public List<Text> names2UI;

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
            LoadExistingTournaments(); // Load existing tournaments
            InvokeRepeating("CheckForExpiredTournaments", 0, 3600); // Check every hour
        });
    }

    private void ListenForTournamentChanges()
    {
        databaseReference.Child("tournaments").ChildAdded += HandleTournamentAdded;
        databaseReference.Child("tournaments").ChildRemoved += HandleTournamentRemoved;
    }

    private void LoadExistingTournaments()
    {
        databaseReference.Child("tournaments").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to retrieve tournaments from database.");
                return;
            }

            DataSnapshot snapshot = task.Result;

            foreach (DataSnapshot tournamentSnapshot in snapshot.Children)
            {
                TournamentData tournamentData = JsonUtility.FromJson<TournamentData>(tournamentSnapshot.GetRawJsonValue());
                string tournamentName = HelperClass.Decrypt(tournamentSnapshot.Key, playerId);

                // Check if the tournament has already been instantiated
                if (!instantiatedTournaments.ContainsKey(tournamentName))
                {
                    parent.GetComponent<RectTransform>().sizeDelta = new Vector2(parent.GetComponent<RectTransform>().sizeDelta.x, parent.GetComponent<RectTransform>().sizeDelta.y + 350);

                    // Instantiate the tournament prefab and set data
                    GameObject tournamentObject = Instantiate(tournamentPref, content.transform);
                    tournamentObject.transform.parent = content.transform;
                    Tournament tournamentComponent = tournamentObject.GetComponent<Tournament>();
                    if (tournamentComponent != null)
                    {
                        tournamentComponent.SetData(tournamentName, tournamentData.type, tournamentData.players, tournamentData.bid);

                        // Add the instantiated tournament to the dictionary
                        instantiatedTournaments[tournamentName] = tournamentObject;
                    }
                    else
                    {
                        Debug.LogError("Tournament component not found on instantiated prefab.");
                    }
                }
            }
        });
    }

    private void HandleTournamentAdded(object sender, ChildChangedEventArgs e)
    {
        if (e.DatabaseError != null)
        {
            Debug.LogError(e.DatabaseError.Message);
            return;
        }

        DataSnapshot snapshot = e.Snapshot;
        string tournamentKey = snapshot.Key;

        if (snapshot.Exists)
        {
            TournamentData tournamentData = JsonUtility.FromJson<TournamentData>(snapshot.GetRawJsonValue());
            string tournamentName = HelperClass.Decrypt(tournamentKey, playerId);

            Debug.Log($"Tournament added: {tournamentName}");

            // Check if the tournament has already been instantiated
            if (!instantiatedTournaments.ContainsKey(tournamentName))
            {
                parent.GetComponent<RectTransform>().sizeDelta = new Vector2(parent.GetComponent<RectTransform>().sizeDelta.x, parent.GetComponent<RectTransform>().sizeDelta.y + 350);

                // Instantiate the tournament prefab and set data
                InstantiateTournamentPrefab(tournamentKey, tournamentData);
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
        string tournamentKey = snapshot.Key;
        string tournamentName = HelperClass.Decrypt(tournamentKey, playerId);

        if (instantiatedTournaments.ContainsKey(tournamentName))
        {
            // Instead of destroying, we can deactivate the GameObject
            instantiatedTournaments[tournamentName].SetActive(false);
            instantiatedTournaments.Remove(tournamentName);
        }
    }

    public void CreateTournament()
    {
        if (tournamentname.text.Length <= 0)
        {
            menu.errorpanel.SetActive(true);
            menu.errorpanel_text.text = "Invalid TournamentName";
            return;
        }

        string key = HelperClass.Encrypt(tournamentname.text, playerId);
        var newTournamentData = new TournamentData
        {
            name = tournamentname.text, // Store the original name
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
                InstantiateTournamentPrefab(key, newTournamentData);

                tourr.SetActive(true);
                tourrName.text = tournamentname.text;
            }
            else
            {
                Debug.LogError("Failed to create tournament.");
            }
        });
    }

    private void InstantiateTournamentPrefab(string key, TournamentData tournamentData)
    {
        // Check if the tournament has already been instantiated
        string tournamentName = HelperClass.Decrypt(key, playerId);
        if (!instantiatedTournaments.ContainsKey(tournamentName))
        {
            parent.GetComponent<RectTransform>().sizeDelta = new Vector2(parent.GetComponent<RectTransform>().sizeDelta.x, parent.GetComponent<RectTransform>().sizeDelta.y + 350);

            // Instantiate the tournament prefab and set data
            GameObject tournamentObject = Instantiate(tournamentPref, content.transform);
            tournamentObject.transform.parent = content.transform;
            Tournament tournamentComponent = tournamentObject.GetComponent<Tournament>();
            if (tournamentComponent != null)
            {
                tournamentComponent.SetData(tournamentName, tournamentData.type, tournamentData.players, tournamentData.bid);

                // Add the instantiated tournament to the dictionary
                instantiatedTournaments[tournamentName] = tournamentObject;
                Debug.Log($"Tournament instantiated: {tournamentName}");
            }
            else
            {
                Debug.LogError("Tournament component not found on instantiated prefab.");
            }
        }
        else
        {
            Debug.LogWarning("Tournament already instantiated: " + tournamentName);
        }
    }

    public void Join(string key)
    {
        Debug.Log("Joining tournament with key: " + key);
        currenttour = key;
        tourr.SetActive(true);

        // Decrypt the tournament name for display
        tourrName.text = HelperClass.Decrypt(key, playerId);

        // Get the player's name
        string playerName = PhotonNetwork.NickName;
        Debug.Log("Player name: " + playerName);

        // Flag to check if player is already in the list
        bool playerAlreadyJoined = false;

        // Check if the player is already in the list
        foreach (Text nameText in names16UI)
        {
            if (nameText.text == playerName)
            {
                playerAlreadyJoined = true;
                Debug.Log("Player already in the list.");
                break;
            }
        }

        // If the player is not in the list, add them
        if (!playerAlreadyJoined)
        {
            bool nameAdded = false;
            for (int i = 0; i < names16UI.Count; i++)
            {
                    names16UI[i].text = playerName;
                    nameAdded = true;
                Debug.Log("Added player to the list: " + playerName);
            }

            if (!nameAdded)
            {
                Debug.LogWarning("No empty slots available to add player.");
            }
        }
    }

    public void DeleteTournament(string tournamentKey)
    {
        databaseReference.Child("tournaments").Child(tournamentKey).RemoveValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                // Remove the instantiated tournament from the dictionary and deactivate it
                string tournamentName = HelperClass.Decrypt(tournamentKey, playerId);
                if (instantiatedTournaments.ContainsKey(tournamentName))
                {
                    instantiatedTournaments[tournamentName].SetActive(false);
                    instantiatedTournaments.Remove(tournamentName);
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
