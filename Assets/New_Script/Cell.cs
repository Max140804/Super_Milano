using UnityEngine;

public class Cell : MonoBehaviour
{
    public int cellValue;

    void Update()
    {
        //CheckForChildObject();
    }

    public void CheckForChildObject()
    {
        string childName = "Image";
        Transform childTransform = transform.Find(childName);

        if (childTransform != null && childName == "Image")
        {
            Debug.Log($"Child object {childName} found!");
        }
        else if (childTransform != null && childName == "Image_1")
        {
            Debug.Log($"Child object {childName} found!");
        }
        else
        {
            Debug.Log($"Child object {childName} not found.");
        }
    }
}
