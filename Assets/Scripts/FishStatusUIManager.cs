using UnityEngine;

public class FishStatusUIManager : MonoBehaviour
{
    public Canvas statusUI;
    public GameObject attractImage;
    public GameObject resistImage;

    private void OnEnable()
    {
        attractImage.SetActive(false);
        resistImage.SetActive(false);
    }

    void Update()
    {
        if (Camera.main != null)
        {
            statusUI.transform.LookAt(Camera.main.transform);
            statusUI.transform.Rotate(0f, 180f, 0f);
        }
    }

    public void AttractFishImage()
    {
        attractImage.SetActive(true);
    }

    public void ResistFishImage()
    {
        attractImage.SetActive(false);
        resistImage.SetActive(true);
    }

    public void ResetImage()
    {
        attractImage.SetActive(false);
        resistImage.SetActive(false);
    }
    
}
