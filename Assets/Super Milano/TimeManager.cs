using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

public class TimeManager : MonoBehaviour
{
    private const string timeApiUrl = "https://worldtimeapi.org/api/timezone/etc/utc";

    public void GetServerTime(Action<DateTime> onTimeReceived)
    {
        StartCoroutine(FetchServerTime(onTimeReceived));
    }

    private IEnumerator FetchServerTime(Action<DateTime> onTimeReceived)
    {
        UnityWebRequest request = UnityWebRequest.Get(timeApiUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error fetching time: " + request.error);
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            DateTime serverTime = ParseTime(jsonResponse);
            onTimeReceived?.Invoke(serverTime);
        }
    }

    private DateTime ParseTime(string json)
    {
        var jsonObj = JsonUtility.FromJson<TimeResponse>(json);
        return DateTime.Parse(jsonObj.utc_datetime);
    }

    [Serializable]
    private class TimeResponse
    {
        public string utc_datetime;
    }
}
