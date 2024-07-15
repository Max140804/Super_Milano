using UnityEngine;
using UnityEngine.EventSystems;

public class DominoBottomHalf : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Grid grid;
    private Vector3 offset;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = FindObjectOfType<Canvas>();
        canvasGroup = GetComponentInParent<CanvasGroup>();
        grid = FindObjectOfType<Grid>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        Vector3 worldMousePos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, eventData.position, canvas.worldCamera, out worldMousePos);
        offset = rectTransform.position - worldMousePos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 worldMousePos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, eventData.position, canvas.worldCamera, out worldMousePos);
        rectTransform.position = worldMousePos + offset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        Collider2D hitCollider = Physics2D.OverlapPoint(eventData.position);

        if (hitCollider != null)
        {
            GameObject cellObject = hitCollider.gameObject;
            if (cellObject.transform.parent == grid.transform)
            {
                Vector2Int cellIndex = grid.GetCellIndexFromCellObject(cellObject);

                if (grid.IsValidCellIndex(cellIndex) && !grid.IsCellLocked(cellIndex))
                {
                    RectTransform cellRectTransform = grid.GetCellRectTransform(cellIndex);
                    rectTransform.SetParent(cellRectTransform, false);
                    rectTransform.localPosition = Vector3.zero;

                    grid.LockCell(cellIndex);

                    CardData cardData = GetComponentInParent<CardData>();
                    int bottomValue = cardData.GetBottomValue();

                    grid.UpdateCellBottomValue(cellIndex, bottomValue);

                    Debug.Log($"Snapped to cell {cellIndex}. Bottom Value: {bottomValue}");
                }
                else
                {
                    rectTransform.anchoredPosition = Vector3.zero;
                    Debug.Log("Invalid or occupied cell.");
                }
            }
        }
        else
        {
            rectTransform.anchoredPosition = Vector3.zero;
            Debug.Log("No collider hit.");
        }
    }
}
