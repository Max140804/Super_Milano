using UnityEngine;
using UnityEngine.EventSystems;

public class DragAndDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private CardData cardData;
    private Grid grid;
    private Transform originalParent;
    private DominoHand originalHand;
    private DominoBoneYard originalBoneYard;

    private Vector3 offset;

    public RectTransform topHalf;
    public RectTransform bottomHalf;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = FindObjectOfType<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        grid = FindObjectOfType<Grid>();
        cardData = GetComponent<CardData>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!TurnManager.Instance.IsMyTurn()) return;

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
        if (!TurnManager.Instance.IsMyTurn()) return;

        Vector3 worldMousePos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, eventData.position, canvas.worldCamera, out worldMousePos);
        rectTransform.position = worldMousePos + offset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!TurnManager.Instance.IsMyTurn()) return;

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
                    TurnManager.Instance.EndTurn();
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
    }

    private bool CanSnapToCell(Vector2Int cellIndex)
    {
        return grid.IsValidCellIndex(cellIndex) && !grid.IsCellLocked(cellIndex);
    }

    private void SnapToCells(Vector2Int cellIndex)
    {
        Vector2Int topHalfCellIndex = cellIndex;
        Vector2Int bottomHalfCellIndex = GetBottomHalfCellIndex(cellIndex);

        if (CanSnapToCell(topHalfCellIndex))
        {
            SnapToCell(topHalf, topHalfCellIndex);
            SnapToCell(rectTransform, topHalfCellIndex);
            grid.LockCell(topHalfCellIndex);
        }

        if (CanSnapToCell(bottomHalfCellIndex))
        {
            SnapToCell(bottomHalf, bottomHalfCellIndex);
            grid.LockCell(bottomHalfCellIndex);
        }

        if (originalHand != null)
        {
            originalHand.RemoveFromHand(gameObject);
        }
        else if (originalBoneYard != null)
        {
            originalBoneYard.RemoveFromBoneYard(gameObject);
        }
    }

    private Vector2Int GetBottomHalfCellIndex(Vector2Int cellIndex)
    {
        if (cardData.isRotated)
        {
            return new Vector2Int(cellIndex.x - 1, cellIndex.y);
        }
        else
        {
            return new Vector2Int(cellIndex.x, cellIndex.y - 1);
        }
    }

    private void SnapToCell(RectTransform halfRectTransform, Vector2Int cellIndex)
    {
        RectTransform cellRectTransform = grid.GetCellRectTransform(cellIndex);
        halfRectTransform.SetParent(cellRectTransform, false);
        halfRectTransform.localPosition = Vector3.zero;

        grid.LockCell(cellIndex);

        if (halfRectTransform == topHalf)
        {
            int topValue = cardData.GetTopValue();
            grid.UpdateCellTopValue(cellIndex, topValue);

            Debug.Log($"Snapped top half to cell {cellIndex}. Top Value: {topValue}");
        }
        else if (halfRectTransform == bottomHalf)
        {
            int bottomValue = cardData.GetBottomValue();
            grid.UpdateCellBottomValue(cellIndex, bottomValue);

            Debug.Log($"Snapped bottom half to cell {cellIndex}. Bottom Value: {bottomValue}");
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
}
