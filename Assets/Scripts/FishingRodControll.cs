using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class FishingRodControll : MonoBehaviour
{
    public InputActionReference pullFishingrod; // 인풋액션
    public GameObject lureFac;      // 루어의 프리팹
    public GameObject fishingRod;   // 낚싯대 오브젝트
    public Transform lureTrasform;  // 루어의 발사위치

    Rigidbody lureRB;           // 루어의 리지드바디
    Coroutine rotateCoroutine;  // 낚싯대 던지기/당기기 코루틴
    bool lureIsReady;           // 루어가 준비되었을때
    bool isCharging;            // 낚시대를 뒤로 당기는중인지 => 당길때의 차지 여부
    bool isThrowing;            // 루어가 던져져 있는지 구분 => 던져져있을때 당기면 낚싯대 들어올리기
    float chargeForce;          // 낚시대를 당기며 모으는 힘
    Vector3 rodRotationBeforeCharge;    // 낚싯대 회전 전 기본 위치
    GameObject lure;            // 생성된 루어 게임오브젝트
    //private float lureReadyTimer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    void OnEnable()
    {
        pullFishingrod.action.started += PullFishingrod;        // Input System 이벤트 연결
        pullFishingrod.action.canceled += CancelPullFishingrod;

        lureIsReady = true;
        isCharging = false;
        isThrowing = false;

        lure = Instantiate(lureFac);    // 루어 오브젝트 미리생성
        if (lureRB == null)
            lureRB = lure.GetComponent<Rigidbody>();
        lureRB.isKinematic = true;
        lure.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (isCharging == false)
        {
            SetFishingrodPosition(Camera.main.transform);
        }
    }

    void SetFishingrodPosition(Transform anchor)
    {
        if (isCharging == false)                                // 던지는 중이 아닐때 카메라를 움직이면 낚싯대 따라옴
        {
            Vector3 offset = anchor.forward * 0.8f + anchor.up * -0.5f;
            transform.position = anchor.position + offset;

            transform.rotation = anchor.rotation;
        }

        /*if (isThrowing == false && lureIsReady == true)         // 던져저 있지 않을때, 루어가 준비되었을때만 루어의 위치 조정
        {
            lure.transform.position = lureTrasform.position;    // 루어의 위치는 낚시대의 위치와 동기화
        }*/
    }

    void PullFishingrod(InputAction.CallbackContext context)
    {
        rodRotationBeforeCharge = transform.rotation.eulerAngles;   // 당기기 전 회전값 저장
        float x = rodRotationBeforeCharge.x;
        float y = rodRotationBeforeCharge.y;
        float z = rodRotationBeforeCharge.z;

        if (isThrowing == false)
        {
            isCharging = true;  // 당길때는 힘 차지
            lureIsReady = true;
            if (rotateCoroutine != null) StopCoroutine(rotateCoroutine);
            rotateCoroutine = StartCoroutine(RotateToTarget(new Vector3(x - 20f, y - 0f, z - 0f), 2f));
        }
        else
        {
            isCharging = false; // 낚아채는건 차지가 아님
            if (rotateCoroutine != null) StopCoroutine(rotateCoroutine);    // 낚싯대 들어올리기
            rotateCoroutine = StartCoroutine(RotateToTarget(new Vector3(x - 10f, y - 0f, z - 0f), 0.1f));
        }

        Debug.Log("Touch Pressed");
        //debugText.text = "Touch Pressed";
    }

    void CancelPullFishingrod(InputAction.CallbackContext context)
    {   
        isCharging = false;     // 던질때는 차지하지 않음
        float x = rodRotationBeforeCharge.x;
        float y = rodRotationBeforeCharge.y;
        float z = rodRotationBeforeCharge.z;
        if (rotateCoroutine != null) StopCoroutine(rotateCoroutine);
        rotateCoroutine = StartCoroutine(RotateToTarget(new Vector3(x + 40f, y, z), 0.5f));

        Debug.Log("Touch Canceled");
        //debugText.text = "Touch Calceled";
    }

    IEnumerator RotateToTarget(Vector3 targetEulerAngles, float duration)   // 당기기, 던지기 코루틴
    {
        Quaternion startRotation = fishingRod.transform.rotation;
        Quaternion endRotation = Quaternion.Euler(targetEulerAngles);

        if (isThrowing == true) // 루어가 던져져 있다면 낚싯대 들어올리기
        {
            HookUpLure();
        }

        float time = 0f;

        while (time <= duration)        // 당기기 혹은 던지기 시간만큼 진행
        {
            time += Time.deltaTime;
            if (isCharging == true)     // 차지중이라면, 힘을 모음 => 낚시대 당기는 중, 들어올리기는 isCharging = false이므로 차지하지 않음
                chargeForce = time * 0.4f;
            float t = Mathf.Clamp01(time / duration);
            fishingRod.transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);    // 낚싯대 원래 회전으로
            yield return null;
        }

        // 만약 차지중이지 않고, 끝까지 낚싯대를 던졌고, 루어가 준비되고, 루어를 던져저 있지 않는다면 루어 발사, 들어올릴때 루어가 준비되지 않았으므로 던지지 않음
        if (isCharging == false && time >= duration && lureIsReady == true && isThrowing == false)
        {
            ThrowLure();
        }

        lure.GetComponent<LureSetting>().hookUp = false;
        gameObject.transform.rotation = endRotation;
    }

    void ThrowLure()    // 루어 던지기, 던지는중true, 루어준비false, 던진 후 던지는 힘 초기화
    {
        lure.SetActive(true);
        lure.transform.position = lureTrasform.position;

        isThrowing = true;      // 루어가 던져져 있으므로, true
        lureIsReady = false;    // 루어가 던져졌으므로 루어준비 안됨
        lureRB.isKinematic = false;
        lureRB.AddForce(transform.forward * chargeForce, ForceMode.Impulse);

        chargeForce = 0f;       // 던지는 힘 초기화
    }

    void HookUpLure()   // 루어 끌어올리기, 끌어올린 후에는 루어를 새로 껴야 함
    {
        lure.GetComponent<LureSetting>().hookUp = true; // 루어의 상태변환, 물고기가 인식하고 잡힘

        lure.SetActive(false);  // 루어 비활성화
        isThrowing = false;
    }

    public void ConnectInputSystem()
    {
        pullFishingrod.action.started += PullFishingrod;        // Input System 이벤트 연결
        pullFishingrod.action.canceled += CancelPullFishingrod;
    }

    public void DisconnectInputSystem()
    {
        pullFishingrod.action.started -= PullFishingrod;        // Input System 이벤트 연결
        pullFishingrod.action.canceled -= CancelPullFishingrod;
    }
}
