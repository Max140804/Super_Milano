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
    public GameObject currentTour;

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
    private Dictionary<string, List<Text>> playerUILists = new Dictionary<string, List<Text>>();

    int playersInTour = -1;

    public float matchStartDelay = 1800f;

    public void SetBid(float bid)
    {
        tournamentbid = bid;
    }
    public void SetType(string type)
    {
        tournamenttype = type;
    }

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
            ListenForTournamentChanges();
            LoadExistingTournaments();
            InvokeRepeating("CheckForExpiredTournaments", 0, 3600);
            InvokeRepeating("UpdateTournamentTimes", 0, 30);
        });
    }

    private void UpdateTournamentTimes()
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
                long matchStartTime = createdAt;
                long timeRemaining = matchStartTime - currentTime;
                if (timeRemaining < 0) timeRemaining = 0;

                if (tournamentData.timeRemaining != timeRemaining)
                {
                    tournamentData.timeRemaining = timeRemaining;
                    string updatedJson = JsonUtility.ToJson(tournamentData);
                    databaseReference.Child("tournaments").Child(tournamentSnapshot.Key).SetRawJsonValueAsync(updatedJson).ContinueWithOnMainThread(updateTask =>
                    {
                        if (updateTask.IsCompleted)
                        {
                            Debug.Log("Tournament time remaining updated successfully.");
                        }
                        else
                        {
                            Debug.LogError("Failed to update tournament time remaining in the database.");
                        }
                    });
                }
            }
        });
    }

    private void ListenForTournamentChanges()
    {
        databaseReference.Child("tournaments").ChildAdded += HandleTournamentAdded;
        databaseReference.Child("tournaments").ChildChanged += HandleTournamentChanged;
        databaseReference.Child("tournaments").ChildRemoved += HandleTournamentRemoved;
    }

    private void HandleTournamentChanged(object sender, ChildChangedEventArgs e)
    {
        if (e.DatabaseError != null)
        {
            Debug.LogError(e.DatabaseError.Message);
            return;
        }

        DataSnapshot snapshot = e.Snapshot;
        string tournamentKey = snapshot.Key;
        string tournamentName = HelperClass.Decrypt(tournamentKey, playerId);

        if (snapshot.Child("players").Exists)
        {
            UpdatePlayerList(tournamentKey);
        }
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
                        tournamentComponent.SetData(tournamentName, tournamentData.type, tournamentData.players, tournamentData.bid, tournamentData.timeRemaining);

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

            if (tournamentName == null)
            {
                Debug.LogError("Failed to decrypt tournament name.");
                return;
            }

            Debug.Log($"Tournament added: {tournamentName}");

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
            playerUILists.Remove(tournamentName);
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
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var newTournamentData = new TournamentData
        {
            name = tournamentname.text,
            type = tournamenttype,
            players = 16,
            bid = tournamentbid,
            createdAt = currentTime,
            timeRemaining = matchStartDelay
        };
        string json = JsonUtility.ToJson(newTournamentData);

        databaseReference.Child("tournaments").Child(key).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Tournament created successfully.");
                InstantiateTournamentPrefab(key, newTournamentData);

                StartCoroutine(StartMatchDelay(key, newTournamentData.timeRemaining));

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
        string tournamentName = HelperClass.Decrypt(key, playerId);
        if (!instantiatedTournaments.ContainsKey(tournamentName))
        {
            parent.GetComponent<RectTransform>().sizeDelta = new Vector2(parent.GetComponent<RectTransform>().sizeDelta.x, parent.GetComponent<RectTransform>().sizeDelta.y + 350);

            // Instantiate the tournament prefab and set data
            GameObject tournamentObject = Instantiate(tournamentPref, content.transform);
            Tournament tournamentComponent = tournamentObject.GetComponent<Tournament>();
            if (tournamentComponent != null)
            {
                tournamentComponent.SetData(tournamentName, tournamentData.type, tournamentData.players, tournamentData.bid, tournamentData.timeRemaining);

                // Add the instantiated tournament to the dictionary
                instantiatedTournaments[tournamentName] = tournamentObject;
                playerUILists[tournamentName] = names16UI; // Assuming 16 player UI list for now
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
        playersInTour++;

        bool playerAlreadyJoined = false;
        foreach (Text nameText in names16UI)
        {
            if (nameText.text == Photon.Pun.PhotonNetwork.NickName)
            {
                playerAlreadyJoined = true;
                Debug.Log("Player already in the list.");
                return;
            }
        }

        if (!playerAlreadyJoined)
        {
            Debug.Log($"Adding player {Photon.Pun.PhotonNetwork.NickName} at index {playersInTour}");
            if (playersInTour >= 0 && playersInTour < names16UI.Count)
            {
                names16UI[playersInTour].text = Photon.Pun.PhotonNetwork.NickName;
                databaseReference.Child("tournaments").Child(key).Child("players").Child("player" + (playersInTour + 1)).SetValueAsync(Photon.Pun.PhotonNetwork.NickName).ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompleted)
                    {
                        Debug.Log("Player added to tournament in the database.");
                    }
                    else
                    {
                        Debug.LogError("Failed to add player to tournament in the database.");
                    }
                });
            }
            else
            {
                Debug.LogError("Invalid player index or UI list is not populated correctly.");
            }
        }

        tourrName.text = HelperClass.Decrypt(key, playerId);
        UpdatePlayerList(key);
    }

    private void UpdatePlayerList(string tournamentKey, string playerName = "")
    {
        databaseReference.Child("tournaments").Child(tournamentKey).Child("players").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to retrieve player list from database.");
                return;
            }

            DataSnapshot snapshot = task.Result;
            int index = 0;
            foreach (DataSnapshot playerSnapshot in snapshot.Children)
            {
                string playerNameFromDb = playerSnapshot.Value.ToString();
                if (index < names16UI.Count)
                {
                    names16UI[index].text = playerNameFromDb;
                    index++;
                }
            }
        });
    }

    public void FinalizeTournament()
    {
        if (string.IsNullOrEmpty(currenttour))
        {
            Debug.LogWarning("No tournament selected to finalize.");
            return;
        }

        string tournamentName = HelperClass.Decrypt(currenttour, playerId);

        if (instantiatedTournaments.ContainsKey(tournamentName))
        {
            GameObject tournamentObject = instantiatedTournaments[tournamentName];
            tournamentObject.transform.SetParent(currentTour.transform);
            tournamentObject.transform.localPosition = Vector3.zero;

            Tournament tournamentComponent = tournamentObject.GetComponent<Tournament>();
            if (tournamentComponent != null)
            {
                tournamentComponent.DisableEntry();
            }

            Debug.Log($"Tournament '{tournamentName}' has been finalized and moved under 'currentTour'.");
            databaseReference.Child("tournaments").Child(currenttour).Child("finalized").SetValueAsync(true).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("Tournament finalized status updated in database.");
                }
                else
                {
                    Debug.LogError("Failed to update tournament finalized status in the database.");
                }
            });
        }
        else
        {
            Debug.LogWarning("Tournament not found in instantiated tournaments.");
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
                    playerUILists.Remove(tournamentName);
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
                if (currentTime - createdAt > 1800)
                {
                    string tournamentKey = tournamentSnapshot.Key;
                    DeleteTournament(tournamentKey);
                }
            }
        });
    }

    public void RefreshTournamentList()
    {
        foreach (var tournament in instantiatedTournaments)
        {
            Destroy(tournament.Value);
        }
        instantiatedTournaments.Clear();
       // playerUILists.Clear();
        LoadExistingTournaments();
    }

    IEnumerator StartMatchDelay(string key, float delay)
    {
        yield return new WaitForSeconds(delay);
        StartMatch(key);
    }

    private void StartMatch(string tournamentKey)
    {
        // Here you can implement the logic to start the match
        // For example, notify players, initialize match settings, etc.

        Debug.Log($"Match for tournament {tournamentKey} is starting now.");
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
    public float timeRemaining;
}
