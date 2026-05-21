using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FishingUIManager : MonoBehaviour
{
    // UI관련 
    private const string catchMessage = "You caught a ";
    public GameObject fishingCanvas;
    public TMP_Text fishInfoText;
    public Image fishImageUI;
    public Image fishTierImage;
    public TMP_Text scoreText;
    public FishingRodControll rod;

    // 물고기 정보 관련


    void Start()
    {
        fishingCanvas.gameObject.SetActive(false);
    }

    public void PopupSuccessUI(FishType fishType, Sprite fishImage, Sprite fishTier, int score)
    {
        // UI활성화
        fishingCanvas.gameObject.SetActive(true);

        // 낚싯대 조작 방지
        if (rod != null)
            rod.DisconnectInputSystem();

        string fishTypeName = fishType.ToString();

        // UI 값변경
        if (fishType == FishType.SeaBream)
            fishTypeName = "Sea bream";
        else if (fishType == FishType.BlueTang)
            fishTypeName = "Blue tang";
        else if (fishType == FishType.LargeheadHairtail)
            fishTypeName = "\nLargehead Hairtail";
        else if (fishType == FishType.BluefinTuna)
            fishTypeName = "Bluefin Tuna";
        else if (fishType == FishType.OliveFlounder)
            fishTypeName = "\nOliver Flounder";

        fishInfoText.text = catchMessage + fishTypeName + "!";
        fishImageUI.sprite = fishImage;
        fishTierImage.sprite = fishTier;
        scoreText.text = "+ " + score.ToString();
    }

    public void ClosePopupUI()
    {
        fishingCanvas.gameObject.SetActive(false);

        // 낚싯대 입력 재연결
        if (rod != null)
            rod.ConnectInputSystem();
    }
 }
