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
    private DominoBoneYard yard;
    private DominoHand hand;
    private GetValue[] values;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        dragAndDrop = GetComponent<DragAndDrop>();
        gameManager = FindObjectOfType<DominoGameManager>();
        yard = FindObjectOfType<DominoBoneYard>();
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
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            //photonView.RPC("RPC_AddDominoToPlayerHand", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
            Player player = PhotonNetwork.CurrentRoom.GetPlayer(PhotonNetwork.LocalPlayer.ActorNumber);
            DominoHand playerHand = GetPlayerHand(player);
            yard.AddToHand(playerHand);
        }
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
        int currentPlayerIndex = 0;
        if (PhotonNetwork.IsMasterClient)
        {
            currentPlayerIndex = 0;
        }
        else if (!PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                if (PhotonNetwork.PlayerList[i].ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    currentPlayerIndex = i;
                    break;
                }
            }
        }

        Player targetPlayer = PhotonNetwork.PlayerList[currentPlayerIndex];

        GameObject playerObject = GameObject.Find(targetPlayer.ActorNumber.ToString());
        if (playerObject != null)
        {
            return playerObject.GetComponentInChildren<DominoHand>();
        }
        else
        {
            Debug.Log($"Player object with ActorNumber {targetPlayer.ActorNumber} not found.");
            return null;
        }
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
}
