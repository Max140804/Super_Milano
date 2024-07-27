using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections;

public class Tournament : MonoBehaviour
{
    public Text nameText;
    public Text typeText;
    public Text bidText;
    public Text playerstext;
    public Text TimeRemainingTillStart;
    public Button button;

    public GameObject uno;
    public GameObject domino;
    public GameObject coin_icon;
    public GameObject loading;

    public Image image;

    public bool canJoin = true;

    private Coroutine countdownCoroutine;
    private float countdownTime; // Track the remaining time

    public void SetData(string name, string type, int players, float bid, float time)
    {
        nameText.text = name;

        if (type == "Uno")
        {
            uno.SetActive(true);
            domino.SetActive(false);
            image.color = new Color32(241, 101, 137, 255);
        }
        else if (type == "Domino")
        {
            domino.SetActive(true);
            uno.SetActive(false);
            image.color = new Color32(120, 199, 199, 255);
        }

        typeText.text = type;
        bidText.text = bid.ToString();
        playerstext.text = players.ToString() + " Players";
        button.interactable = true;
        coin_icon.SetActive(true);
        loading.SetActive(false);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClickJoin);

        // Update the countdown timer
        UpdateCountdown(time);
    }

    private void UpdateCountdown(float time)
    {
        countdownTime = time;

        // Stop any existing countdown coroutine
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }

        countdownCoroutine = StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        float startTime = Time.time;
        float endTime = startTime + countdownTime;

        while (Time.time < endTime)
        {
            float remainingTime = endTime - Time.time;
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            TimeRemainingTillStart.text = "Starts in: " + string.Format("{0:D2}:{1:D2}", minutes, seconds);

            yield return new WaitForSeconds(1f);
        }

        TimeRemainingTillStart.text = "Tournament Started!";
        button.interactable = false;
    }

    public void OnClickJoin()
    {
        Events_Manager eventsManager = GameObject.FindObjectOfType<Events_Manager>();
        MainMenu mainMenu = GameObject.FindObjectOfType<MainMenu>();

        if (eventsManager != null && mainMenu != null && canJoin)
        {
            eventsManager.Join(nameText.text);
            eventsManager.tourrName.text = nameText.text;
        }
        else
        {
            eventsManager.tourr.SetActive(true);
            eventsManager.tourrName.text = nameText.text;
        }
    }

    public void DisableEntry()
    {
        canJoin = false;
        Events_Manager eventsManager = GameObject.FindObjectOfType<Events_Manager>();

        this.transform.parent = eventsManager.currentTour.transform;
    }

    private void OnDestroy()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }
    }
}
