using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class DragAndDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPunObservable
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private CardData cardData;
    private Grid grid;
    private Transform originalParent;
    private DominoHand originalHand;
    private DominoBoneYard originalBoneYard;
    public bool isBottomHalf;
    private Vector3 offset;
    private PhotonView photonView;

    public RectTransform topHalf;
    public RectTransform bottomHalf;
    private int topValue;
    private int bottomValue;
    public CardVisibilityManager visibilityManager;
    TurnManager turn;

    private Vector3 networkPosition;
    private Quaternion networkRotation;
    public bool isMine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = FindObjectOfType<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        grid = FindObjectOfType<Grid>();
        turn = FindObjectOfType<TurnManager>();
        cardData = GetComponent<CardData>();
        visibilityManager = GetComponent<CardVisibilityManager>();
        photonView = GetComponentInParent<PhotonView>();
    }

    private void Start()
    {
        topValue = cardData.topValue;
        bottomValue = cardData.bottomValue;

        if (!photonView.IsMine)
        {
            networkPosition = rectTransform.position;
            networkRotation = rectTransform.rotation;
        }
    }

    private void Update()
    {
        if(photonView == null)
        {
            return;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!photonView.IsMine || !turn.IsMyTurn())
        {
            Debug.Log("Not your turn or not your object!");
            return;
        }

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        Vector3 worldMousePos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, eventData.position, canvas.worldCamera, out worldMousePos);
        offset = rectTransform.position - worldMousePos;

        originalParent = transform.parent;
        originalHand = originalParent.GetComponentInParent<DominoHand>();
        originalBoneYard = originalParent.GetComponentInParent<DominoBoneYard>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!photonView.IsMine || !turn.IsMyTurn())
        {
            return;
        }

        Vector3 worldMousePos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, eventData.position, canvas.worldCamera, out worldMousePos);
        rectTransform.position = worldMousePos + offset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!photonView.IsMine || !turn.IsMyTurn())
        {
            return;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        Collider2D hitCollider = Physics2D.OverlapPoint(eventData.position);

        if (hitCollider != null)
        {
            GameObject cellObject = hitCollider.gameObject;

            if (cellObject.transform.parent == grid.transform)
            {
                Vector2Int cellIndex = grid.GetCellIndexFromCellObject(cellObject);

                if (grid.IsValidCellIndex(cellIndex))
                {
                    SnapToCells(cellIndex);
                    SetCellValues(cellObject);
                    visibilityManager.opponentCardImage.SetActive(false);
                }
                else
                {
                    ResetPosition();
                    Debug.Log("Invalid cell index: " + cellIndex);
                }
            }
        }
        else
        {
            ResetPosition();
            Debug.Log("No collider hit.");
        }

        photonView.RPC("DistributeCardPositions", RpcTarget.All);

        turn.EndTurn();
    }

    private bool CanSnapToCell(Vector2Int cellIndex, int halfValue, bool isTopHalf)
    {
        if (!grid.IsValidCellIndex(cellIndex) || grid.IsCellLocked(cellIndex)) return false;

        int cellValue = isTopHalf ? topValue : bottomValue;

        return cellValue == halfValue || cellValue == -1;
    }

    private void SnapToCells(Vector2Int cellIndex)
    {
        Vector2Int topHalfCellIndex = cellIndex;
        Vector2Int bottomHalfCellIndex = GetBottomHalfCellIndex(cellIndex);

        topValue = cardData.GetTopValue();
        bottomValue = cardData.GetBottomValue();

        bool canSnapTop = CanSnapToCell(topHalfCellIndex, topValue, true);
        bool canSnapBottom = CanSnapToCell(bottomHalfCellIndex, bottomValue, false);

        if (canSnapTop && canSnapBottom)
        {
            SnapToCell(topHalf, topHalfCellIndex);
            if (!cardData.isRotated)
            {
                SnapToCell(rectTransform, topHalfCellIndex, new Vector3(0, -15, 0));
            }
            else
            {
                SnapToCell(rectTransform, topHalfCellIndex, new Vector3(15, 0, 0));
            }

            SnapToCell(bottomHalf, bottomHalfCellIndex);

            grid.LockCell(topHalfCellIndex);
            grid.LockCell(bottomHalfCellIndex);

            if (originalHand != null)
            {
                originalHand.RemoveFromHand(gameObject);
            }
            else if (originalBoneYard != null)
            {
                originalBoneYard.RemoveFromBoneYard(gameObject);
            }

            SetCellValues(grid.GetCellObject(topHalfCellIndex));
            SetCellValues(grid.GetCellObject(bottomHalfCellIndex));
        }
        else
        {
            ResetPosition();
            Debug.Log("Cannot snap both halves to valid cells.");
        }
    }

    private Vector2Int GetBottomHalfCellIndex(Vector2Int cellIndex)
    {
        if (cardData.isRotated)
        {
            return new Vector2Int(cellIndex.x + 1, cellIndex.y);
        }
        else
        {
            return new Vector2Int(cellIndex.x, cellIndex.y + 1);
        }
    }

    private void SnapToCell(RectTransform halfRectTransform, Vector2Int cellIndex, Vector3? offset = null)
    {
        RectTransform cellRectTransform = grid.GetCellRectTransform(cellIndex);
        halfRectTransform.SetParent(cellRectTransform, false);
        halfRectTransform.localPosition = offset ?? Vector3.zero;
        halfRectTransform.localScale = Vector3.one;
    }

    private void SetCellValues(GameObject cellObject)
    {
        Cell cell = cellObject.GetComponent<Cell>();

        if (cell != null)
        {
            if (isBottomHalf)
            {
                cell.cellValue = bottomValue;
                Debug.Log($"Set cell {cellObject.name} bottom value to {bottomValue}");
            }
            else if (!isBottomHalf)
            {
                cell.cellValue = topHalf.GetComponent<GetValue>().topValue;
                Debug.Log($"Set cell {cellObject.name} top value to {topValue}");
            }
        }
    }

    private void ResetPosition()
    {
        rectTransform.anchoredPosition = Vector3.zero;
    }

    public void AdjustSnapPositions()
    {
        Vector2Int currentCellIndex = grid.GetCellIndexFromPosition(rectTransform.position);
        SnapToCells(currentCellIndex);
    }

    [PunRPC]
    private void DistributeCardPositions()
    {
        DominoHand[] allPlayers = FindObjectsOfType<DominoHand>();

        foreach (DominoHand player in allPlayers)
        {
            List<GameObject> playerDominoes = player.GetDominoesInHand();

            for (int i = 0; i < playerDominoes.Count; i++)
            {
                GameObject domino = playerDominoes[i];
                domino.transform.localPosition = GetPositionForDomino(player, i);
            }
        }
    }

    private Vector3 GetPositionForDomino(DominoHand player, int index)
    {
        float spacing = 1.5f;
        return new Vector3(index * spacing, 0, 0); // Distribute horizontally
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(rectTransform.position);
            stream.SendNext(rectTransform.rotation);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
        }
    }

    /*private void Update()
    {
        if (!photonView.IsMine)
        {
            rectTransform.position = Vector3.Lerp(rectTransform.position, networkPosition, Time.deltaTime * 10);
            rectTransform.rotation = Quaternion.Lerp(rectTransform.rotation, networkRotation, Time.deltaTime * 10);
        }
        isMine = photonView.IsMine;
    }*/
}
