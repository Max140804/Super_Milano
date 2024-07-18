using UnityEngine;
using UnityEngine.UI;

public class Tournament : MonoBehaviour
{
    public Text nameText;
    public Text typeText;
    public Text bidText;
    public Text playerstext;
    public Button button;

    public GameObject uno;
    public GameObject domino;
    public GameObject coin_icon;
    public GameObject loading;

    public Image image;

    public void SetData(string name, string type, int players, float bid)
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

        // Remove any existing listeners to avoid multiple subscriptions
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClickJoin);
    }

    public void OnClickJoin()
    {
        Events_Manager eventsManager = GameObject.FindObjectOfType<Events_Manager>();
        MainMenu mainMenu = GameObject.FindObjectOfType<MainMenu>();

        if (eventsManager != null && mainMenu != null)
        {
            eventsManager.JoinTournament(nameText.text);
        }
        else
        {
            Debug.LogWarning("Events_Manager or MainMenu not found in the scene.");
        }
    }
}
