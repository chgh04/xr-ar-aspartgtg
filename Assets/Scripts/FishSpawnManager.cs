using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Linq;

public class FishSpawnManager : MonoBehaviour
{
    //public Text debugText;
    public ARPlaneManager arPlaneManager;
    public List<GameObject> spawnPrefabs;   // 생성할 물고기 프리팹
    public float belowDistance = 0.4f;      // 얼마나 아래에 생성할건지
    public int maxPrefabCount = 10;         // 미리 생성할 최대 물고기 개수
    public int prefabCountPerPlane = 3;     // 한 plane마다 생성할 물고기 수
    public List<GameObject> fishPool;       // 물고기 오브젝트 풀 
    public List<GameObject> activeFish;     // 생성된 물고기의 오브젝트 리스트
    private Dictionary<ARPlane, int> spawnedFishPerPlane = new Dictionary<ARPlane, int>();    // ARPlane이 가진 프리팹의 수
    private float fishCountCheckTimer = 0f;

    void OnEnable()
    {
        if (arPlaneManager != null)
        {
            arPlaneManager.trackablesChanged.AddListener(OnPlanesChanged);
        }

        /*foreach (var fish in spawnPrefabs)
        {
            for (int i = 0; i < maxPrefabCount; i++)
            {
                GameObject selectedPrefab = Instantiate(spawnPrefabs[Random.Range(0, spawnPrefabs.Count)]); // 프리팹 중 랜덤하게 선택
                fishPool.Add(selectedPrefab);       // 선택된 랜덤 프리팹은 오브젝트 풀에 들어감
                selectedPrefab.GetComponent<FishPoolReturnManager>().parentObject = this.gameObject;    // 생성된 오브젝트의 부모오브젝트를 자신으로 설정
                selectedPrefab.SetActive(false);    // 비활성화
            }
        }*/
        for (int i = 0; i < maxPrefabCount; i++)
        {
            GameObject selectedPrefab = Instantiate(spawnPrefabs[Random.Range(0, spawnPrefabs.Count)]); // 프리팹 중 랜덤하게 선택
            //fishPool.Add(selectedPrefab);     // 선택된 랜덤 프리팹은 오브젝트 풀에 들어감, <= 프리팹이 비활성화될때 스스로 부모의 풀에 들어감
            selectedPrefab.GetComponent<FishPoolReturnManager>().parentObject = this.gameObject;    // 생성된 오브젝트의 부모오브젝트를 자신으로 설정
            selectedPrefab.SetActive(false);    // 비활성화
        }

    }

    void OnDisable()
    {
        if (arPlaneManager != null)
        {
            arPlaneManager.trackablesChanged.RemoveListener(OnPlanesChanged);
        }
    }

    void Update()
    {
        fishCountCheckTimer += Time.deltaTime;
        if (fishCountCheckTimer >= 10f)
        {
            //fishCountCheckTimer = 0f;
            FishCountCheck();
        }
    }

    void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> eventArgs)
    {
        //debugText.text = eventArgs.added.Count.ToString();
        foreach (ARPlane addedPlane in eventArgs.added) // 각각의 평면마다
        {
            if (spawnedFishPerPlane.ContainsKey(addedPlane)) continue;  // 이미 있는 플레인은 스킵
            else spawnedFishPerPlane[addedPlane] = 0;                   // 아닐경우 해당 플레인의 프리팹 수는 0

            for (int i = 0; i < prefabCountPerPlane; i++)   // prefabCount만큼 반복 => 그만큼 스폰
            {
                if (fishPool.Count > 0)
                {
                    SpawnFish(addedPlane);  // 물고기 소환
                }
            }
        }
    }

    void FishCountCheck()
    {
        fishCountCheckTimer = 0f;
        foreach (KeyValuePair<ARPlane, int> pair in spawnedFishPerPlane)    // ARPlane, 프리팹 수 쌍 순회
        {
            if (pair.Value < prefabCountPerPlane)   // 만약 한 플레인의 프리팹 수가 플레인당 최대 프리팹 수보다 작다면
            {
                if (fishPool.Count > 0)
                {
                    SpawnFish(pair.Key);    // 프리팹 생성
                }
                else
                {
                    return; // 풀에 더이상 오브젝트가 없다면 반환
                }
            }
        }
    }

    void SpawnFish(ARPlane addedPlane)
    {
        GameObject selectedPrefab = fishPool[0];    // pool의 첫 번째 오브젝트 가져오기

        selectedPrefab.GetComponent<FishMovementController>().nowReadyForSpawn = true;  // 생성 준비 완료 - 업그레이드 반영을 위한 bool값

        selectedPrefab.SetActive(true); // 오브젝트 활성화

        selectedPrefab.GetComponent<FishPoolReturnManager>().SetPlane(addedPlane);  // addedplane에 속하도록 설정

        Vector3 spawnPosition = addedPlane.transform.position + Vector3.down * belowDistance;   // 스폰 위치 지정

        spawnPosition += new Vector3(Random.Range(-0.7f, 0.7f), 0, Random.Range(-0.7f, 0.7f));  // 스폰 위치 랜덤화

        selectedPrefab.transform.position = spawnPosition;  // 스폰 위치로 위치 변경

        fishPool.RemoveAt(0);   // 생성한 프리팹은 pool에서 삭제

        spawnedFishPerPlane[addedPlane]++;  // 해당 플레인의 프리팹 수 증가
    }

    public void SetSpawnedFishPerPlaneCount(ARPlane plane, int num)
    {
        if (spawnedFishPerPlane.ContainsKey(plane))
        {
            spawnedFishPerPlane[plane] += num;
        }
    }

    public void ResetAllFishes()
    {
        for (int i = activeFish.Count - 1; i >= 0; i--)
        {
            activeFish[i].SetActive(false);
        }

        FishCountCheck();
    }
}
