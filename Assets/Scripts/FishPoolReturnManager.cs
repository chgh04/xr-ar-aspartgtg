using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class FishPoolReturnManager : MonoBehaviour
{
    public GameObject parentObject; // 자신이 들어갈 오브젝트 풀을 가진 게임오브젝트, 부모 오브젝트가 생성시 지정함
    //public GameObject debugText;
    private ARPlane belongPlane;
    private FishSpawnManager spawnManager;

    void OnEnable()
    {   
        if (parentObject != null)
        {       
            spawnManager = parentObject.GetComponent<FishSpawnManager>();

            if (!spawnManager.activeFish.Contains(this.gameObject))
            {
                spawnManager.activeFish.Add(this.gameObject);  // 활성화될때 이 오브젝트르 활성화된 오브젝트 리스트에 추가
            }
        }
    }

    void OnDisable()
    {   
        if (parentObject != null)
        {   
            spawnManager = parentObject.GetComponent<FishSpawnManager>();
            spawnManager.fishPool.Add(this.gameObject);    // 비활성화 될 때 부모 오브젝트의 풀에 스스로 들어감
            spawnManager.activeFish.Remove(this.gameObject);   // 비활성화되면 활성화된 오브젝트 리스트에서 스스로 제거
            if (belongPlane != null)
                spawnManager.SetSpawnedFishPerPlaneCount(belongPlane, -1); // 비활성화될 때 플레인에 속한 프리팹의 수 1 감소
            //debugText.GetComponent<Text>().text = "return to pool";
        }
    }

    public void SetPlane(ARPlane plane) // 자신이 존재하는 plane 설정
    {
        this.belongPlane = plane;
    }
}
