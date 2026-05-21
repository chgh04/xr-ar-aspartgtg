using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Game Managers
    public FishingUIManager UIManager;
    public RecordUIManager recordUIManager;
    public LineConnector lineConnector;
    public ShopManager shopManager;

    int probabilityUpgradeLevel;
    int delayUpgradLevel;

    public static GameManager instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {

    }

    void Update()
    {

    }

    public void SendCapturedFishInfoToUI(FishType fishType, Sprite fishImage, Sprite fishTier, int score)
    {
        UIManager.PopupSuccessUI(fishType, fishImage, fishTier, score);
        recordUIManager.AddFishingRecord(fishType, score);
    }

    public void connectLineWithRod(Transform objectTransform)
    {
        lineConnector.objectB = objectTransform;
    }

    public float ProvideUpgradeInfo_Prob()
    {
        shopManager.loadLevel(out probabilityUpgradeLevel, out _);
        if (probabilityUpgradeLevel == 0)
            return 0;
        else
            return ((float)probabilityUpgradeLevel / 10);
    }

    public float ProvideUpgradeInfo_Del()
    {
        shopManager.loadLevel(out _, out delayUpgradLevel);
        if (delayUpgradLevel == 0)
            return 0;
        else
            return ((float)delayUpgradLevel / 10);
    }

}
