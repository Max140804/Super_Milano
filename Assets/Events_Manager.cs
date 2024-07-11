using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System;
using UnityEditor;
using UnityEngine.UI;
using System.Security.Cryptography;

public class Events_Manager : MonoBehaviour
{

    string playerId = "234353423"; 
    
    
    public GameObject tournamentPrefab;
    public GameObject parent;
    public GameObject CurrentTourParent;
    public GameObject content;

    public GameObject tourr;
    public MainMenu menu;

    public List<Text> names16;
    public List<Text> names8;
    public List<Text> names4;
    public List<Text> names2;


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
    private void Awake()
    {

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
        menu.databaseReference.Child(HelperClass.Encrypt("players", menu.playerId)).Child(HelperClass.Encrypt(menu.playerId, menu.playerId)).Child(HelperClass.Encrypt("Tournments", playerId)).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                // Handle the error
                Debug.LogError("Error getting data: " + task.Exception);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                foreach (DataSnapshot childSnapshot in snapshot.Children)
                {
                    Debug.Log(childSnapshot.Key);
                    CurrentTourParent.GetComponent<RectTransform>().sizeDelta = new Vector2(CurrentTourParent.GetComponent<RectTransform>().sizeDelta.x, CurrentTourParent.GetComponent<RectTransform>().sizeDelta.y + 350);

                    // Instantiate a GameObject for each tournament
                    GameObject tournamentObject = Instantiate(tournamentPrefab);
                    tournamentObject.transform.SetParent(CurrentTourParent.transform);

                    // Set data in the instantiated GameObject
                    Tournament tournamentComponent = tournamentObject.GetComponent<Tournament>();
                    Debug.Log(childSnapshot.Key);
                    StartCoroutine(tour(childSnapshot.Key, tournamentComponent));
                }


                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());

            }
        });
    }

    public IEnumerator tour(string key, Tournament tournamentComponent)
    {

        string name = "Failed To load";
        int players = 2;
        float bid = 0.5f;
        string type = "Uno";


        var typetask = menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(key).Child(HelperClass.Encrypt("type", playerId)).GetValueAsync();
        yield return new WaitUntil(() => typetask.IsCompleted);

        if (typetask.Exception != null)
        {
            Debug.LogError($"Failed to get player coins: {typetask.Exception}");
        }
        else if (typetask.Result.Value == null)
        {
            Debug.Log("Player not found or coins not set.");
        }
        else
        {
            type = typetask.Result.Value.ToString();
            // Do something with the retrieved coins, e.g., update UI
        }

        name = HelperClass.Decrypt(key, playerId);

        var playerstask = menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(key).Child(HelperClass.Encrypt("players", playerId)).GetValueAsync();
        yield return new WaitUntil(() => playerstask.IsCompleted);

        if (playerstask.Exception != null)
        {
            Debug.LogError($"Failed to get player coins: {playerstask.Exception}");
        }
        else if (playerstask.Result.Value == null)
        {
            Debug.Log("Player not found or coins not set.");
        }
        else
        {
            players = int.Parse(playerstask.Result.Value.ToString());
            // Do something with the retrieved coins, e.g., update UI
        }

        var bidtask = menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(key).Child(HelperClass.Encrypt("bid", playerId)).GetValueAsync();
        yield return new WaitUntil(() => bidtask.IsCompleted);

        if (bidtask.Exception != null)
        {
            Debug.LogError($"Failed to get player coins: {bidtask.Exception}");
        }
        else if (bidtask.Result.Value == null)
        {
            Debug.Log("Player not found or coins not set.");
        }
        else
        {
            bid = float.Parse(bidtask.Result.Value.ToString());
            // Do something with the retrieved coins, e.g., update UI
        }

        tournamentComponent.SetData(name, type, players, bid);
    }
    public void GetTournamentsData()
    {
        parent.GetComponent<RectTransform>().sizeDelta = new Vector2(parent.GetComponent<RectTransform>().sizeDelta.x, 0);
        foreach (Transform child in parent.transform)
        {
            Destroy(child.gameObject);
        }
        menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                // Handle the error
                Debug.LogError("Error getting data: " + task.Exception);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                foreach (DataSnapshot childSnapshot in snapshot.Children)
                {
                    Debug.Log("xxx");

                    parent.GetComponent<RectTransform>().sizeDelta = new Vector2(parent.GetComponent<RectTransform>().sizeDelta.x, parent.GetComponent<RectTransform>().sizeDelta.y + 350);
                    // Assuming each child has some data you want to use
                    string tournamentName = HelperClass.Decrypt(childSnapshot.Key, playerId);
                    int tournamentplayers = int.Parse(childSnapshot.Child(HelperClass.Encrypt("players", playerId)).Value.ToString());
                    float tournamentbid = float.Parse(childSnapshot.Child(HelperClass.Encrypt("bid", playerId)).Value.ToString());
                    string tournamenttype = childSnapshot.Child(HelperClass.Encrypt("type", playerId)).Value.ToString();
                    Debug.Log("ddddddddddddddd");

                    // Instantiate a GameObject for each tournament
                    GameObject tournamentObject = Instantiate(tournamentPrefab);
                    tournamentObject.transform.SetParent(parent.transform);

                    // Set data in the instantiated GameObject
                    Tournament tournamentComponent = tournamentObject.GetComponent<Tournament>();
                    if (tournamentComponent != null)
                    {
                        tournamentComponent.SetData(tournamentName, tournamenttype, tournamentplayers, tournamentbid);
                    }


                }


                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());

            }
        });
    }

    public void Create()
    {
        if(tournamenttype == "Domino")
        {
            tournamentplayers = 4;
        }
        StartCoroutine(createtournment());

        GetTournamentsData();

    }
    public IEnumerator createtournment()
    {
        var check = menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(HelperClass.Encrypt(tournamentname.text, playerId)).GetValueAsync();
        yield return new WaitUntil(() => check.IsCompleted);

        if (check.Exception != null)
        {
            Debug.LogError($"Failed to get player coins: {check.Exception}");
        }
        else if (check.Result.Value == null)
        {
            StartCoroutine(Createtour(tournamentname.text, tournamenttype, tournamentplayers, tournamentbid));
        }
        else
        {
            menu.errorpanel.gameObject.SetActive(true);
            menu.errorpanel_text.text = "Name is used, Please change tournament name";
        }



    }

    public IEnumerator Createtour(string name, string type, int players, float bid)
    {
        var task = menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(HelperClass.Encrypt(name, playerId)).Child(HelperClass.Encrypt("type", playerId)).SetValueAsync(type);
        yield return new WaitUntil(() => task.IsCompleted);
        var task2 = menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(HelperClass.Encrypt(name, playerId)).Child(HelperClass.Encrypt("players", playerId)).SetValueAsync(players);
        yield return new WaitUntil(() => task2.IsCompleted);
        var task3 = menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(HelperClass.Encrypt(name, playerId)).Child(HelperClass.Encrypt("bid", playerId)).SetValueAsync(bid);
        yield return new WaitUntil(() => task3.IsCompleted);

        if (task3.Exception != null)
        {
        }
        else
        {
        }
        if (task3.Exception != null)
        {
        }
        else
        {
        }
        if (task2.Exception != null)
        {
        }
        else
        {
        }
        menu.errorpanel.SetActive(true);
        menu.errorpanel_text.text = "Tournament has been created successfully";
    }

    public void jointournment(string playername, string tourname)
    {
        currenttour = tourname;
        currentusername = playername;
        StartCoroutine(jointour(playername, tourname));
        tourr.SetActive(true);
        Get16Tournment(tourname);
        Get8Tournment(tourname);
        Get4Tournment(tourname);
        Get2Tournment(tourname);

    }
    public IEnumerator jointour(string username, string name)
    {

        var ready = menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(HelperClass.Encrypt(name, playerId)).Child(HelperClass.Encrypt("16", playerId)).Child(HelperClass.Encrypt(username, playerId)).Child(HelperClass.Encrypt("Is Ready", playerId)).GetValueAsync();
        yield return new WaitUntil(() => ready.IsCompleted);

        if (ready.Exception != null)
        {
            Debug.LogError($"Failed to get player coins: {ready.Exception}");
        }
        else if (ready.Result.Value == null)
        {
            var task = menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(HelperClass.Encrypt(name, playerId)).Child(HelperClass.Encrypt("16", playerId)).Child(HelperClass.Encrypt(username, playerId)).Child(HelperClass.Encrypt("Is Ready", playerId)).SetValueAsync(HelperClass.Encrypt("Not Ready", playerId));
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.Exception != null)
            {
            }
            else
            {
            }
        }
        else
        {

        }






        var task2 = menu.databaseReference.Child(HelperClass.Encrypt("players", menu.playerId)).Child(HelperClass.Encrypt(menu.playerId, menu.playerId)).Child(HelperClass.Encrypt("Tournments", playerId)).Child(HelperClass.Encrypt(name, playerId)).SetValueAsync("Not Finished");
        yield return new WaitUntil(() => task2.IsCompleted);

        if (task2.Exception != null)
        {
        }
        else
        {
        }



        var task3 = menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(HelperClass.Encrypt(name, playerId)).Child(HelperClass.Encrypt("16", playerId)).Child(HelperClass.Encrypt(username, playerId)).Child(HelperClass.Encrypt("Room To Join", playerId)).SetValueAsync("23324234");
        yield return new WaitUntil(() => task3.IsCompleted);

        if (task3.Exception != null)
        {
        }
        else
        {
        }
    }

    public void Get16Tournment(string name)
    {
        int stage16 = 0;
        menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(HelperClass.Encrypt(name, playerId)).Child(HelperClass.Encrypt("16", playerId)).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                menu.errorpanel.SetActive(true);
                menu.errorpanel_text.text = "Error Getting Data";
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                foreach (DataSnapshot childSnapshot in snapshot.Children)
                {
                    names16[stage16].text = HelperClass.Decrypt(childSnapshot.Key, playerId);
                    ready16[stage16].text = HelperClass.Decrypt(childSnapshot.Child(HelperClass.Encrypt("Is Ready", playerId)).Value.ToString(), playerId);

                    stage16 += 1;

                }
            }
        });
    }


    public void Get8Tournment(string name)
    {
        int stage8 = 0;
        menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(HelperClass.Encrypt(name, playerId)).Child(HelperClass.Encrypt("8", playerId)).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                menu.errorpanel.SetActive(true);
                menu.errorpanel_text.text = "Error Getting Data";
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                foreach (DataSnapshot childSnapshot in snapshot.Children)
                {
                    ready8[stage8].text = HelperClass.Decrypt(childSnapshot.Child(HelperClass.Encrypt("Is Ready", playerId)).Value.ToString(), playerId);

                    names8[stage8].text = HelperClass.Decrypt(childSnapshot.Key, playerId);
                    stage8 += 1;

                }
            }
        });
    }
    public void Get4Tournment(string name)
    {
        int stage4 = 0;
        menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(HelperClass.Encrypt(name, playerId)).Child(HelperClass.Encrypt("4", playerId)).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                menu.errorpanel.SetActive(true);
                menu.errorpanel_text.text = "Error Getting Data";
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                foreach (DataSnapshot childSnapshot in snapshot.Children)
                {
                    ready4[stage4].text = HelperClass.Decrypt(childSnapshot.Child(HelperClass.Encrypt("Is Ready", playerId)).Value.ToString(), playerId);

                    names4[stage4].text = HelperClass.Decrypt(childSnapshot.Key, playerId);
                    stage4 += 1;

                }
            }
        });
    }
    public void Get2Tournment(string name)
    {
        int stage2 = 0;
        menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(HelperClass.Encrypt(name, playerId)).Child(HelperClass.Encrypt("2", playerId)).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                menu.errorpanel.SetActive(true);
                menu.errorpanel_text.text = "Error Getting Data";
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                foreach (DataSnapshot childSnapshot in snapshot.Children)
                {
                    ready2[stage2].text = HelperClass.Decrypt(childSnapshot.Child(HelperClass.Encrypt("Is Ready", playerId)).Value.ToString(), playerId);

                    names2[stage2].text = HelperClass.Decrypt(childSnapshot.Key, playerId);
                    stage2 += 1;

                }
            }
        });
    }

    public void Ready()
    {
        StartCoroutine(GetReady());

    }
    public IEnumerator GetReady()
    {
        var task = menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(HelperClass.Encrypt(currenttour, playerId)).Child(HelperClass.Encrypt("16", playerId)).Child(HelperClass.Encrypt(currentusername, playerId)).Child(HelperClass.Encrypt("Is Ready", playerId)).SetValueAsync(HelperClass.Encrypt("Ready",playerId));
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
        }
        else
        {
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
