using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase;
using Firebase.Database;
using System.Collections;
using System;
public class sccc : MonoBehaviourPunCallbacks
{
    public InputField input;
    public InputField playerid;
    public Text text;
    // Start is called before the first frame update
    void Start()
    {


    }
    public void translate()
    {
        text.text = HelperClass.Decrypt(input.text, playerid.text);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
