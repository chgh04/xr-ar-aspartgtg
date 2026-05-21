using UnityEngine;

public enum FishType
{
    Herring,
    Carp,
    Mackerel,
    SeaBream,
    Hairtail,
    BlueTang,
    LargeheadHairtail,
    BluefinTuna,
    OliveFlounder
}

public class FishTypeComponent : MonoBehaviour
{
    public FishType fishType;
    public Sprite fishImage;
    public Sprite fishTier;
    public int fishScore;

    public void ReturnFishInfo(out FishType fishType, out Sprite fishImage, out Sprite fishTier, out int fishScore)
    {
        fishType = this.fishType;
        fishImage = this.fishImage;
        fishTier = this.fishTier;
        fishScore = this.fishScore; 
    }
}


