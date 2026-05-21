using TMPro;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public GameObject shopCanvas;
    public TMP_Text scoreText;
    public TMP_Text probabilityUpgradeLevelText;
    public TMP_Text delayUpgradLevelText;
    public RecordUIManager recordUIManager;
    public int[] upgradeRequireScore = new int[2];

    private int probabilityUpgradeLevel;
    private int delayUpgradLevel;

    void Start()
    {
        loadUpgradeLevel();
        shopCanvas.SetActive(false);
    }

    void OnEnable()
    {
        SetButtonText();
    }

    void saveUpgradLevel()
    {
        PlayerPrefs.SetInt("PULevel", probabilityUpgradeLevel);
        PlayerPrefs.SetInt("DULevel", delayUpgradLevel);
        PlayerPrefs.Save();
    }

    void loadUpgradeLevel()
    {
        probabilityUpgradeLevel = PlayerPrefs.GetInt("PULevel", 0);
        delayUpgradLevel = PlayerPrefs.GetInt("DULevel", 0);
    }

    public void loadLevel(out int probabilityUpgradeLevel, out int delayUpgradLevel)
    {
        probabilityUpgradeLevel = this.probabilityUpgradeLevel;
        delayUpgradLevel = this.delayUpgradLevel;
    }

    public void OpenAndCloseShopUI()
    {
        if (shopCanvas.activeInHierarchy)
        {
            shopCanvas.SetActive(false);
        }
        else
        {
            shopCanvas.SetActive(true);
            SetButtonText();
        }
    }

    public void UpgradeProbability()
    {
        if (probabilityUpgradeLevel < 2)
        {
            if (recordUIManager.Score >= upgradeRequireScore[probabilityUpgradeLevel])
            {
                recordUIManager.Score -= upgradeRequireScore[probabilityUpgradeLevel];
                probabilityUpgradeLevel++;
                SetButtonText();
                saveUpgradLevel();
            }
        }
        else
        {
            return;
        }
    }

    public void UpgradeDelay()
    {
        if (delayUpgradLevel < 2)
        {
            if (recordUIManager.Score >= upgradeRequireScore[delayUpgradLevel])
            {
                recordUIManager.Score -= upgradeRequireScore[delayUpgradLevel];
                delayUpgradLevel++;
                SetButtonText();
                saveUpgradLevel();
            }
        }
        else
        {
            return;
        }
    }

    void SetButtonText()
    {
        // 유인 확률 강화
        if (probabilityUpgradeLevel < 2)
        {
            probabilityUpgradeLevelText.text = "Attract probability\nLevel "
                                        + probabilityUpgradeLevel.ToString()
                                        + " > " + (probabilityUpgradeLevel + 1).ToString()
                                        + " " + upgradeRequireScore[probabilityUpgradeLevel];
        }
        else
        {
            probabilityUpgradeLevelText.text = "Attract probability\nLevel max";
        }

        // 입질 시간 강화
        if (delayUpgradLevel < 2)
        {
            delayUpgradLevelText.text = "Snatch delay\nLevel "
                                        + delayUpgradLevel.ToString()
                                        + " > " + (delayUpgradLevel + 1).ToString()
                                        + " " + upgradeRequireScore[delayUpgradLevel];
        }
        else
        {
            delayUpgradLevelText.text = "Snatch delay\nLevel max";
        }

        // 점수 최신화
        scoreText.text = "Your Score: " + recordUIManager.Score;
    }

}
