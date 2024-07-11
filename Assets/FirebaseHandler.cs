using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using System;
using TMPro;
using UnityEngine;

public class FirebaseHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tmpText;
    [SerializeField] private TextMeshProUGUI usedIDTmp;
    // Start is called before the first frame update
    void Start()
    {
        
        tmpText = GameObject.Find("Canvas/Panel/LogTmp").GetComponent<TextMeshProUGUI>();
        usedIDTmp = GameObject.Find("Canvas/Panel/UserIDTmp").GetComponent<TextMeshProUGUI>();

    }

    void FirebaseAuthCurrentUser()
    {
        
        FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync().ContinueWithOnMainThread(task => {
            string userID = task.Result.User.UserId;
            Debug.Log($"userID: {userID}");
            usedIDTmp.text = $"userID: {userID}";
        });
    }


    public void ConnectToFirebase()
    {
        string result = "Unhandled Exception! Highly likely platform exception!";
        try
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
                if (task.Result != DependencyStatus.Available)
                {
                    result = $"Result: {task.Result} || exception: {task.Exception}";
                    Debug.Log(result);
                    tmpText.text = result;
                }
                else
                {
                    result = FirebaseApp.DefaultInstance.Options.ProjectId;
                    Debug.Log(result);                    tmpText.text = result;
                    FirebaseAuthCurrentUser();
                }
            });
        }
        catch (Exception e)
        {
            result = $"Failed with exception: {e}";
            Debug.Log(result);
            tmpText.text = result;
        }
    }
    
    
}



