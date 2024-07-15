using UnityEngine;
using UnityEngine.UI;

public class CardData : MonoBehaviour
{
    public DominoCard cardData;

    private SpriteRenderer spriteRenderer; 
    public Image image;
    [HideInInspector] public int topValue;
    [HideInInspector] public int bottomValue;
    public bool isRotated;

    private DragAndDrop dragAndDrop;
    private DominoGameManager gameManager;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        dragAndDrop = GetComponent<DragAndDrop>();
        gameManager = FindObjectOfType<DominoGameManager>();
    }

    public void AssignCardData(DominoCard newCardData)
    {
        cardData = newCardData;

        image.sprite = cardData.cardVisual;

        topValue = newCardData.topValue;
        bottomValue = newCardData.bottomValue;

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
        DominoBoneYard boneYard = gameManager.GetBoneYard();

        if (boneYard != null && boneYard.Contains(this.gameObject))
        {
            boneYard.RemoveFromBoneYard(this.gameObject);
            DominoHand hand = FindObjectOfType<DominoHand>();

            if (hand != null)
            {
                hand.AddToHand(this.gameObject);

            }
            else
            {
                Debug.LogWarning("Could not find DominoHand component in the parent hierarchy.");
            }
        }
        else
        {
            Debug.LogWarning("This card is not in the boneyard.");
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
