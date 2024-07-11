using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class DailyRewards : MonoBehaviour
{
    public Button claimButton;
    public Text countdownText; // Text component to display the countdown

    public DateTime lastRewardTime;
    private TimeSpan rewardInterval = new TimeSpan(24, 0, 0); // 24 hours interval
    public int currentStreak;
    private TimeManager timeManager;
    private DateTime serverTime; // Variable to store the fetched server time

    public MainMenu menuu;
    void Start()
    {
        timeManager = GetComponent<TimeManager>();
        LoadData();
        timeManager.GetServerTime(CheckForReward);
    }
    void CheckForReward(DateTime fetchedServerTime)
    {
        serverTime = fetchedServerTime; // Store the fetched server time
        if (IsNewDay(serverTime))
        {
            claimButton.interactable = true;
            countdownText.text = "You can claim your reward now!";
        }
        else
        {
            claimButton.interactable = false;
            StartCoroutine(UpdateCountdown());
        }
    }

    bool IsNewDay(DateTime currentServerTime)
    {
        if (lastRewardTime == DateTime.MinValue)
        {
            return true;
        }

        return (currentServerTime - lastRewardTime) > rewardInterval;
    }

    public void ClaimReward()
    {
        claimButton.interactable = false;

        currentStreak++;
        GiveReward();
        SaveData();
        timeManager.GetServerTime(time =>
        {
            serverTime = time; // Update serverTime after claiming the reward
            StopCoroutine(UpdateCountdown()); // Stop the countdown coroutine
            StartCoroutine(UpdateCountdown()); // Restart the countdown coroutine
        });
        countdownText.text = "You can claim your reward now!";
    }

    void GiveReward()
    {
        StartCoroutine(menuu.GetPlayerCoins(menuu.playerId));
        menuu.UpdatePlayerCoins(menuu.playerId, menuu.coins + 0.05f);
        menuu.coins += 0.05f;

    }

    void LoadData()
    {
            StartCoroutine(menuu.GetTime(menuu.playerId));
    }

    void SaveData()
    {
        lastRewardTime = serverTime; // Use the server time to save the last reward time
        StartCoroutine(menuu.UpdateTime(menuu.playerId, lastRewardTime.ToBinary().ToString(), currentStreak));
    }
    IEnumerator UpdateCountdown()
    {
        while (true)
        {
            TimeSpan timeUntilNextReward = (lastRewardTime + rewardInterval) - serverTime;
            if (timeUntilNextReward <= TimeSpan.Zero)
            {
                countdownText.text = "You can claim your reward now!";
                claimButton.interactable = true;
                yield break;
            }
            else
            {
                countdownText.text = "Time until next reward: " + timeUntilNextReward.ToString(@"hh\:mm\:ss");
                claimButton.interactable = false;
            }

            // Fetch server time every minute, update local time every second
            yield return new WaitForSeconds(1);
            serverTime = serverTime.AddSeconds(1);

            if (serverTime.Second % 60 == 0)
            {
                timeManager.GetServerTime(time => serverTime = time);
            }
        }
    }

}