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

        if(type == "Uno")
        {
            uno.SetActive(true);
            domino.SetActive(false);
            image.color = new Color32(241, 101, 137, 255);

        }
        if (type == "Domino")
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
    }

    void Start()
    {
        Events_Manager even = GameObject.Find("Tournments").GetComponent<Events_Manager>();
        MainMenu main = GameObject.Find("Main_Menu").GetComponent<MainMenu>();

        button.onClick.AddListener(() => even.jointournment(main.usernamee, nameText.text));
    }

}
