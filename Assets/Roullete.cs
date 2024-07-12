using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Roullete : MonoBehaviour
{
    public int rewardSize;
    public float timeToRotate;
    public float rotAmount;
    public int freeSpins;
    public Transform parent;
    public AnimationCurve anim;
    public Text freeSpinText;

    const float _CIRCLE = 360.0f;
    float angleOfOneReward;
    float currentTime;
    bool rotating;

    private void Start()
    {
        angleOfOneReward = _CIRCLE / rewardSize;
        SetPositionData();
        UpdateFreeSpinText();
        rotating = false;
    }

    IEnumerator SpinWheel()
    {
        rotating = true;
        float startAngle = transform.eulerAngles.z;
        currentTime = 0;
        int indexRandomReward = Random.Range(1, rewardSize);

        switch (indexRandomReward)
        {
            case 1:
                Debug.Log("Reward received = 2 extra lives.");
                break;
            case 2:
                Debug.Log("Reward received = 5000 coins.");
                break;
            case 3:
                Debug.Log("Reward received = Free Spin.");
                freeSpins++;
                break;
            case 4:
                Debug.Log("Reward received = 100 coins.");
                break;
            case 5:
                Debug.Log("Reward received = Double point Booster.");
                break;
            case 6:
                Debug.Log("Reward received = Exclusive emoji.");
                break;
            case 7:
                Debug.Log("Reward received = Rare Card Pick.");
                break;
            case 8:
                Debug.Log("Reward received = Mp Membership.");
                break;
            case 9:
                Debug.Log("Reward received = Special avatar skin.");
                break;
        }

        Debug.Log("Reward received = " + indexRandomReward);

        float angleToRot = (rotAmount * _CIRCLE) + angleOfOneReward * indexRandomReward;
        float targetAngle = angleToRot - startAngle;
        float previousAngle = startAngle;

        while (currentTime < timeToRotate)
        {
            yield return new WaitForEndOfFrame();
            currentTime += Time.deltaTime;

            float t = currentTime / timeToRotate;
            float currentAngle = Mathf.Lerp(0, targetAngle, anim.Evaluate(t));
            this.transform.eulerAngles = new Vector3(0, 0, currentAngle + startAngle);

            previousAngle = currentAngle + startAngle;
        }
        transform.eulerAngles = new Vector3(0, 0, startAngle + targetAngle);

        rotating = false;
    }

    void SetPositionData()
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            parent.GetChild(i).eulerAngles = new Vector3(0, 0, -_CIRCLE / rewardSize * i);
            parent.GetChild(i).GetChild(0).GetComponent<TextMeshPro>().text = (i + 1).ToString();
        }
    }

    public void Spin()
    {
        if (!rotating && freeSpins > 0)
        {
            StartCoroutine(SpinWheel());
            freeSpins--;
            UpdateFreeSpinText();
        }
        else if (freeSpins == 0)
        {
            Debug.Log("No more FreeSpins");
        }
    }

    void UpdateFreeSpinText()
    {
        if (freeSpins == 1)
        {
            freeSpinText.text = "You Have: " + freeSpins.ToString() + " Spin";
        }
        else
        {
            freeSpinText.text = "You Have: " + freeSpins.ToString() + " Spins";
        }
    }
}
