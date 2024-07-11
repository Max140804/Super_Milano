using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class Playera : MonoBehaviour
{
    public GameObject CardPanelBG;
    public PlayerCards cardsPanel;

    public PlayerCards seccardsPanel;
    public PlayerCards thirdcardsPanel;
    public PlayerCards forthcardsPanel;



    public PlayerCards two_playersseccardsPanel;

    public PlayerCards three_firstcardsPanel;

    public PlayerCards three_playersseccardsPanel;
    public PlayerCards three_thirdcardsPanel;


    public string playerName;
    public bool isUserPlayer;
    public Image avatarImage;
    public Text avatarName;
    public Text messageLbl;
    public ParticleSystem starParticleSystem;

    public Text messageLblsec;
    public ParticleSystem starParticleSystemsec; 
    public Text messageLblthird;
    public ParticleSystem starParticleSystemthird; 
    public Text messageLblfourth;
    public ParticleSystem starParticleSystemfourth;




    public ParticleSystem two_starParticleSystemsec;
    public Text two_messageLblsec;


    public ParticleSystem three_starParticleSystemfirst;
    public Text three_messageLblfirst;

    public ParticleSystem three_starParticleSystemsec;
    public Text three_messageLblsec;


    public ParticleSystem three_starParticleSystemthird;
    public Text three_messageLblthird;



    public Image timerImage;
    public GameObject timerOjbect;
    public PhotonView pv;

    private float totalTimer = 15f;


    public bool assas;
    [HideInInspector]
    public bool pickFromDeck, unoClicked, choosingColor;
    [HideInInspector]
    public bool isInRoom = true;

    public int ai;
    void Start()
    {
        ai = PlayerPrefs.GetInt("ai");
        Timer = false;
    }



    public void SetAvatarProfile(AvatarProfile p)
    {
        playerName = p.avatarName;
        if (avatarName != null)
        {
            avatarName.text = p.avatarName;
            avatarName.GetComponent<EllipsisText>().UpdateText();
        }
        if (avatarImage != null)
            avatarImage.sprite = Resources.Load<Sprite>("Avatar/" + p.avatarIndex);
    }

    public bool Timer
    {
        get
        {
            return timerOjbect.activeInHierarchy;
        }
        set
        {
            CancelInvoke("UpdateTimer");
            timerOjbect.SetActive(value);
            if (value)
            {
                if(ai == 1)
                {
                timerImage.fillAmount = 0.2f;
                }
                if (ai == 0)
                {
                    timerImage.fillAmount = 5f;
                }
                InvokeRepeating("UpdateTimer", 0f, .1f);
            }
            else
            {
                timerImage.fillAmount = 0f;
            }
        }
    }

    private void Update()
    {
        assas = Timer;
        GameObject manager = GameObject.Find("GamePlay");

        if (manager.GetComponent<GamePlayManager>().numberofplayers == 4)
        {
            if (manager.GetComponent<GamePlayManager>().playercode == 2001)
            {
                cardsPanel = seccardsPanel;

                messageLbl = messageLblsec;
                starParticleSystem = starParticleSystemsec;

            }
            if (manager.GetComponent<GamePlayManager>().playercode == 3001)
            {
                cardsPanel = thirdcardsPanel;

                messageLbl = messageLblthird;
                starParticleSystem = starParticleSystemthird;
            }
            if (manager.GetComponent<GamePlayManager>().playercode == 4001)
            {
                cardsPanel = forthcardsPanel;

                messageLbl = three_messageLblthird;
                starParticleSystem = three_starParticleSystemthird;
            }
        }






        if (manager.GetComponent<GamePlayManager>().numberofplayers == 3)
        {
            if (manager.GetComponent<GamePlayManager>().playercode == 1001)
            {
                cardsPanel = three_firstcardsPanel;

                messageLbl = three_messageLblfirst;
                starParticleSystem = three_starParticleSystemfirst;

            }

            if (manager.GetComponent<GamePlayManager>().playercode == 2001)
            {
                cardsPanel = three_playersseccardsPanel;

                messageLbl = three_messageLblsec;
                starParticleSystem = three_starParticleSystemsec;

            }
            if (manager.GetComponent<GamePlayManager>().playercode == 3001)
            {
                cardsPanel = three_thirdcardsPanel;

                messageLbl = three_messageLblthird;
                starParticleSystem = three_starParticleSystemthird;
            }
        }








        if (manager.GetComponent<GamePlayManager>().numberofplayers == 2)
        {
            if (manager.GetComponent<GamePlayManager>().playercode == 2001)
            {
                cardsPanel = two_playersseccardsPanel;

                messageLbl = two_messageLblsec;
                starParticleSystem = two_starParticleSystemsec;

            }
        }






    }
    void UpdateTimer()
    {
        GameObject manager = GameObject.Find("GamePlay");

        timerImage.fillAmount -= 0.1f / totalTimer;
        if (timerImage.fillAmount <= 0)
        {
            if (choosingColor)
            {
                if (isUserPlayer)
                {
                    GamePlayManager.instance.colorChoose.HidePopup();
                }
                if(manager.GetComponent<GamePlayManager>().playercode == 1001)
                {
                    ChooseBestColor();

                }
            }
            else if (GamePlayManager.instance.IsDeckArrow)
            {
                if (manager.GetComponent<GamePlayManager>().playercode == 1001)
                {
                    GamePlayManager.instance.OnDeckClickkk();
                }
            }
            else if (cardsPanel.AllowedCard.Count > 0)
            {
                if (manager.GetComponent<GamePlayManager>().playercode == 1001)
                {
                    OnCardClick(FindBestPutCard());
                }
            }
            else
            {
                pv.RPC("OnTurnEnd", RpcTarget.All);
            }
        }
    }

    public void OnTurn()
    {
        unoClicked = false;
        pickFromDeck = false;
        Timer = true;

        if (isUserPlayer)
        {
            UpdateCardColor();
            if (cardsPanel.AllowedCard.Count == 0)
            {
                GamePlayManager.instance.EnableDeckClick();
            }
        }
        else
        {
            StartCoroutine(DoComputerTurn());
        }
    }

    public void UpdateCardColor()
    {
        if (isUserPlayer)
        {
            foreach (var item in cardsPanel.AllowedCard)
            {
                item.SetGaryColor(false);
                item.IsClickable = true;
            }
            foreach (var item in cardsPanel.DisallowedCard)
            {
                item.SetGaryColor(true);

                item.IsClickable = false;
            }
            if (cardsPanel.AllowedCard.Count > 0 && cardsPanel.cards.Count == 2)
            {
                GamePlayManager.instance.EnableUnoBtn();
            }
            else
            {
                GamePlayManager.instance.DisableUnoBtn();
            }
        }
    }



    public void AddCard(Card c)
    {
            cardsPanel.cards.Add(c);
            c.transform.SetParent(cardsPanel.transform);
            if (isUserPlayer)
            {
                c.onClick = OnCardClick;
                c.IsClickable = false;
            }
    }

    public void RemoveCard(Card c)
    {
        cardsPanel.cards.Remove(c);
        c.onClick = null;
        c.IsClickable = false;
    }

    public void OnCardClick(Card c)
    {
        if (Timer)
        {
            Debug.Log(c.name);
            Debug.Log(this.name);
            GamePlayManager.instance.PutCardToWastePiless(c, this);
            pv.RPC("OnTurnEnd", RpcTarget.All);
        }
    }

    [PunRPC]
    public void onclick()
    {

    }
    [PunRPC]
    public void OnTurnEnd()
    {
        if (!choosingColor) Timer = false;
        cardsPanel.UpdatePos();
        foreach (var item in cardsPanel.cards)
        {
            item.SetGaryColor(false);
        }
    }

    public void ShowMessage(string message, bool playStarParticle = false)
    {
        messageLbl.text = message;
        messageLbl.GetComponent<Animator>().SetTrigger("show");
        if (playStarParticle)
        {
            starParticleSystem.gameObject.SetActive(true);
            starParticleSystem.Emit(30);
        }
    }

    public IEnumerator DoComputerTurn()
    {
        if (cardsPanel.AllowedCard.Count > 0)
        {
            StartCoroutine(ComputerTurnHasCard(0.25f));
        }
        else
        {
            yield return new WaitForSeconds(Random.Range(1f, totalTimer * .3f));
            GamePlayManager.instance.EnableDeckClick();
            GamePlayManager.instance.OnDeckClickkk();

            if (cardsPanel.AllowedCard.Count > 0)
            {
                StartCoroutine(ComputerTurnHasCard(0.2f));
            }
        }
    }

    private IEnumerator ComputerTurnHasCard(float unoCoef)
    {
        bool unoClick = false;
        float unoPossibality = GamePlayManager.instance.UnoProbability / 100f;

        if (Random.value < unoPossibality && cardsPanel.cards.Count == 2)
        {
            yield return new WaitForSeconds(Random.Range(1f, totalTimer * unoCoef));
            GamePlayManager.instance.OnUnoClickkk();
            unoClick = true;
        }

        yield return new WaitForSeconds(Random.Range(1f, totalTimer * (unoClick ? unoCoef : unoCoef * 2)));
        OnCardClick(FindBestPutCard());
    }

    public Card FindBestPutCard()
    {
        List<Card> allow = cardsPanel.AllowedCard;
        allow.Sort((x, y) => y.Type.CompareTo(x.Type));
        return allow[0];
    }

    public void ChooseBestColor()
    {
        GameObject manager = GameObject.Find("GamePlay");

        if (manager.GetComponent<GamePlayManager>().playercode == 1001)
        {
            CardType temp = CardType.Other;
            if (cardsPanel.cards.Count == 1)
            {
                temp = cardsPanel.cards[0].Type;
            }
            else
            {
                int max = 1;
                for (int i = 0; i < 5; i++)
                {
                    if (cardsPanel.GetCount((CardType)i) > max)
                    {
                        max = cardsPanel.GetCount((CardType)i);
                        temp = (CardType)i;
                    }
                }
            }

            if (temp == CardType.Other)
            {
                GamePlayManager.instance.SelectColor(Random.Range(1, 5), true);
            }
            else
            {

                if (Random.value < 0.7f)
                    GamePlayManager.instance.SelectColor((int)temp, true);
                else
                    GamePlayManager.instance.SelectColor(Random.Range(1, 5), true);
            }
        }
    }

    public int GetTotalPoints()
    {
        int total = 0;
        foreach(var c in cardsPanel.cards)
        {
            total += c.point;
        }
        return total;
    }
}
