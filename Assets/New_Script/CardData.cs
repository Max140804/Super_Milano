using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class CardData : MonoBehaviourPun
{
    public DominoCard cardData;

    private SpriteRenderer spriteRenderer;
    public Image image;
    [HideInInspector] public int topValue;
    [HideInInspector] public int bottomValue;
    public bool isRotated;

    private DragAndDrop dragAndDrop;
    private DominoGameManager gameManager;
    private GetValue[] values;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        dragAndDrop = GetComponent<DragAndDrop>();
        gameManager = FindObjectOfType<DominoGameManager>();
        values = GetComponentsInChildren<GetValue>();
    }

    private void Start()
    {
        // Assign values from ValueStore if they are not default values
        if (ValueStore.TopValue != -1 && ValueStore.BottomValue != -1)
        {
            topValue = ValueStore.TopValue;
            bottomValue = ValueStore.BottomValue;
        }
    }

    private void OnDestroy()
    {
        // Save current values to ValueStore when this object is destroyed
        ValueStore.TopValue = topValue;
        ValueStore.BottomValue = bottomValue;
    }

    public void AssignCardData(DominoCard newCardData)
    {
        cardData = newCardData;
        image.sprite = cardData.cardVisual;
        topValue = newCardData.topValue;
        bottomValue = newCardData.bottomValue;

        foreach (var getValue in values)
        {
            getValue.topValue = newCardData.topValue;
            getValue.bottomValue = newCardData.bottomValue;
        }

        Debug.Log($"Assigned CardData: TopValue = {topValue}, BottomValue = {bottomValue}");
    }

    public int GetTopValue()
    {
        return topValue;
    }

    public int GetBottomValue()
    {
        return bottomValue;
    }

    public void SetCardVisual(Sprite newVisual)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = newVisual;
        }
        else if (image != null)
        {
            image.sprite = newVisual;
        }
    }

    public void Rotate()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.Rotate(0, 0, -90);
        isRotated = !isRotated;
        dragAndDrop.AdjustSnapPositions();
        Debug.Log("Card rotated 90 degrees clockwise");
    }

    public void AddFromBoneYard()
    {
        OnAddButtonClick();
    }

    private void OnAddButtonClick()
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            photonView.RPC("RPC_AddDominoToPlayerHand", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    [PunRPC]
    private void RPC_AddDominoToPlayerHand(int playerActorNumber)
    {
        Player player = PhotonNetwork.CurrentRoom.GetPlayer(playerActorNumber);
        if (player != null)
        {
            DominoHand playerHand = GetPlayerHand(player);
            GameObject domino = GetDominoFromBoneYard();
            if (playerHand != null && domino != null)
            {
                playerHand.AddToHand(domino);
            }
        }
    }

    private DominoHand GetPlayerHand(Player player)
    {
        
        return GameObject.Find($"Player{player.ActorNumber}Hand").GetComponent<DominoHand>();
       
    }

    private GameObject GetDominoFromBoneYard()
    {
        DominoBoneYard boneYard = gameManager.GetBoneYard();
        if (boneYard != null && boneYard.Contains(this.gameObject))
        {
            boneYard.RemoveFromBoneYard(this.gameObject);
            return this.gameObject;
        }
        else
        {
            Debug.LogWarning("This card is not in the boneyard.");
            return null;
        }
    }

    private void ToggleAddButton(GameObject domino, bool active)
    {
        GameObject addButton = domino.transform.Find("AddButton").gameObject;
        if (addButton != null)
        {
            addButton.SetActive(active);
        }
    }
}
