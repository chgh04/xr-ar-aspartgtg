using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class RecordUIManager : MonoBehaviour
{
    public GameObject recordCanvas; // UI 팝업창
    public TMP_Text[] fishingCountRecordText;   // UI의 Text 오브젝트
    public string[] recordingFishNames; // 기록할 물고기의 이름
    public TMP_Text scoreText;
    public FishingRodControll rod;
    private Dictionary<string, int> fishingCountRecord = new Dictionary<string, int>();  // 물고기의 이름과, 그 물고기를 잡은 횟수 딕셔너리
    private Dictionary<string, TMP_Text> fish_UI_Connector = new Dictionary<string, TMP_Text>();    // UI 연결용 딕셔너리
    private string[] originString;  // 기존 메뉴 문자열을 저장하기 위한 배열
    private int score;  // 플레이어 스코어
    public int Score    // 스코어 속성
    {
        get { return score; }
        set
        {
            score = value;
            SavePlayerRecodPref();
            SetRecordText();    
        }
    }

    private void Start()
    {
        if (fishingCountRecordText.Length > 0)
        {
            originString = new string[fishingCountRecordText.Length];   // UI개수만큼 배열크기 정의
            for (int i = 0; i < fishingCountRecordText.Length; i++)
            {
                originString[i] = fishingCountRecordText[i].text;       // 기존 UI에 적힌 문자열을 저장
            }
        }

        if (recordingFishNames.Length > 0)
        {
            foreach (var fish in recordingFishNames)    // fishingCountRecord 딕셔너리 채우기
            {
                fishingCountRecord[fish] = 0;   // 점수 딕셔너리에 물고기의 이름을 먼저 넣고, 값은 임시로 0 입력 
            }
            LoadPayerRecordPref();  // 데이터 불러오기

            for (int i = 0; i < fishingCountRecordText.Length; i++)
            {
                fish_UI_Connector[recordingFishNames[i]] = fishingCountRecordText[i];   // 물고기 이름과 UI텍스트를 1대1 대응
            }
            SetRecordText();    // UI에 변경사항 저장
        }

        recordCanvas.SetActive(false);
    }

    void OnEnable()
    {
        //SetRecordText();    // 기록메뉴를 열때마다 정보 최신화
    }

    public void OpenAndCloseHistoryUI()
    {
        if (recordCanvas.activeInHierarchy)
        {
            recordCanvas.SetActive(false);
            rod.ConnectInputSystem();
        }
        else
        {
            recordCanvas.SetActive(true);
            rod.DisconnectInputSystem();
            SetRecordText();
        }  
    }

    void SavePlayerRecodPref()
    {
        foreach (KeyValuePair<string, int> pair in fishingCountRecord)
        {
            string saveKeyword = pair.Key;

            PlayerPrefs.SetInt(saveKeyword, pair.Value);
            PlayerPrefs.Save();
        }

        PlayerPrefs.SetInt("Score", score);
    }

    void LoadPayerRecordPref()
    {
        foreach (var key in fishingCountRecord.Keys.ToList())
        {
            string loadKeyword = key;

            fishingCountRecord[loadKeyword] = PlayerPrefs.GetInt(loadKeyword, 0);
        }

        score = PlayerPrefs.GetInt("Score", 0);
    }

    public void AddFishingRecord(FishType fishType, int score)
    {
        string capturedFish = fishType.ToString();

        if (fishingCountRecord.ContainsKey(capturedFish))
        {
            fishingCountRecord[capturedFish]++; // 잡은 횟수 1증가
        }
        this.score += score;    // 점수증가
        SavePlayerRecodPref();  // 저장
    }

    void SetRecordText()
    {
        for (int i = 0; i < fishingCountRecordText.Length; i++)
        {
            fishingCountRecordText[i].text = originString[i];   // 텍스트 원상복구
        }

        foreach (KeyValuePair<string, int> pair in fishingCountRecord)
        {
            fish_UI_Connector[pair.Key].text += pair.Value.ToString();  // 문자열에 값을 대입한 새로운 텍스트
        }

        scoreText.text = "Your Score: " + score.ToString();
    }
}
