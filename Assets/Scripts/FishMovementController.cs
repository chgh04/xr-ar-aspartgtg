using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public enum fishState  // 물고기 상태머신
    {
        NORMAL,
        ATTRACTED,
        RESIST
    }

public class FishMovementController : MonoBehaviour
{
    //public GameObject //debugText;

    private fishState state = fishState.NORMAL;
    private NavMeshAgent agent;
    public FishStatusUIManager statusImage;

    // 낚시에 실패했을때, 다시 미끼를 보는데 걸리는 시간
    private bool attractFishAgain;   // true: 물고기가 미끼에 유인당함, false: 물고기가 미끼에 유인당하지 않음
    public float attractFishAgainTime = 5f;
    private float attractFishAgainTimer;

    // 물고기 기본상태
    public float moveRadius = 5f;
    public float waitTime = 3f;
    private float moveTimer;

    // 물고기 유인상태
    public Transform lureTarget;
    public float detectionRange;
    public float detectionInterval;
    public float detectionProbability;
    public float snapDistance;      // 물고기가 이 거리 안까지 유인되면 잡힘
    public float MousePositionAdjustment = 0.2f;    // 물고기가 미끼를 물었을때, 미끼를 입에 맞추기 위한 보정치
    //private bool isAttracted;
    private float lureCheckTimer;
    public float fishBiteLureDelay;

    // 물고기 저항상태
    private bool isBiting;  // 현재 미끼를 무는중인지, 코루틴 연속호출 방지를 위해 사용
    public float resistTime;
    private float resistTimer;
    public float jumpHeight = 1f;
    private bool isCaptured;    // 코루틴 연속호출 방지를 위해 사용

    // 플레이어 업그레이드 반영
    float upgradeAdd_prob;
    float upgradeAdd_delay;
    float newDetectionProbability;
    float newResistTime;

    // 실제 plane에서 생성될때를 구분
    public bool nowReadyForSpawn = false;

    void OnEnable()
    {
        //debugText = GameObject.Find("//debugText2");

        agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = true;
        agent.stoppingDistance = 0.3f;
        agent.avoidancePriority = Random.Range(0, 99); // agent 경로회피 우선순위 임의지정
        //agent.updateRotation = false;

        MoveToRandomPoint();
        attractFishAgainTimer = 0f;
        moveTimer = 0;
        lureCheckTimer = 0f;
        resistTimer = 0f;
        state = fishState.NORMAL;

        attractFishAgain = true;
        //isAttracted = false;
        isBiting = false;
        isCaptured = false;

        if (nowReadyForSpawn == true)
        {
            upgradeAdd_prob = GameManager.instance.ProvideUpgradeInfo_Prob();
            upgradeAdd_delay = GameManager.instance.ProvideUpgradeInfo_Del();
            newResistTime = resistTime + upgradeAdd_delay;
            newDetectionProbability = detectionProbability + upgradeAdd_prob;
        }
    }

    void Update()
    {
        if (attractFishAgain == false)  // 다시 유인당할때까지 타이머
        {
            attractFishAgainTimer += Time.deltaTime;
            if (attractFishAgainTimer >= attractFishAgainTime)
            {
                attractFishAgain = true;
                attractFishAgainTimer = 0f;
            }
        }

        // 물고기 상태머신
        switch (state)
        {
            // 일반적인 상태일때 (미끼를 던지기 전 물고기 움직임)
            case fishState.NORMAL:
                if (!agent.pathPending)
                {
                    // 물고기의 기본적 움직임
                    if (agent.remainingDistance > agent.stoppingDistance)   // 움지이는 방향으로 오브젝트 회전
                    {
                        Vector3 direction = agent.velocity.normalized;
                        if (direction.sqrMagnitude > 0.01f)
                        {
                            direction.y = 0f;
                            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
                        }
                    }
                    else    // 임의 목적지로 도착했다면 새로운 목적지 탐색
                    {
                        moveTimer += Time.deltaTime;
                        if (moveTimer >= waitTime)
                        {
                            moveTimer = 0f;
                            MoveToRandomPoint();
                        }
                    }

                    if (attractFishAgain == false) break;   // 낚시 실패 후라면 미끼에 걸리지 않음

                    // 물고기가 루어를 체크
                    lureCheckTimer += Time.deltaTime;
                    if (lureCheckTimer >= detectionInterval)  // 특정 시간마다 루어탐색
                    {
                        GameObject lureObject = GameObject.FindWithTag("Lure");
                        if (lureObject != null)
                            lureTarget = GameObject.FindWithTag("Lure").transform;
                        else
                            lureTarget = null;
                        lureCheckTimer = 0f;
                        CheckLure();
                    }
                }
                break;

            // 미끼를 던진 후 물고기가 미끼를 인식하였을때의 움직임
            case fishState.ATTRACTED:   
                lureCheckTimer += Time.deltaTime;
                float attractedCheckInterval = (detectionInterval / 2);
                attractedCheckInterval = Mathf.Ceil(attractedCheckInterval * 100f) / 100f;
                if (lureCheckTimer >= attractedCheckInterval)  // 루어는 계속 다시 탐색
                {   
                    //debugText.GetComponent<Text>().text = "Check Lure again, " + lureTarget.gameObject.ToString();
                    lureCheckTimer = 0f;
                    MoveToLure();
                }
                break;

            // 미끼를 문 후 물고기가 저항할때의 움직임
            case fishState.RESIST:
                resistTimer += Time.deltaTime;
                bool hookUp = lureTarget.gameObject.GetComponent<LureSetting>().hookUp;
                if (resistTimer > newResistTime)   // resistTime을 초과했다면 낚시 실패
                {
                    resistTimer = 0f;
                    isBiting = false;
                    //isAttracted = false;
                    attractFishAgain = false;

                    agent.isStopped = false;    // navMesh 재활성화
                    lureTarget.gameObject.GetComponent<LureSetting>().biteByFish = false;
                    // lureTarget.gameObject.GetComponent<LureSetting>().ResetImage(); // 루어 표식 초기화
                    statusImage.ResetImage(); // 상태 UI 초기화
                    state = fishState.NORMAL;
                    //debugText.GetComponent<Text>().text = "Fish State Changed: RESIST -> NORMAL";
                    break;
                }
                else if (resistTimer <= newResistTime && hookUp == true)   // resistTime이 지나기 전에 낚아챘다면 잡힘
                {
                    //debugText.GetComponent<Text>().text = "Fish State Changed: CAPTURED";
                    Captured();
                }
                break;
        }
    }

    void MoveToRandomPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * moveRadius;
        randomDirection += transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, moveRadius, NavMesh.AllAreas))
        {
            Vector3 targetPos = hit.position;
            //targetPos.y -= 0.5f;
            agent.SetDestination(targetPos);
        }
    }

    // NORMAL 상태에서 실행됨   
    void CheckLure()
    {
        float lureDistance = 99f;
        if (lureTarget != null) // 루어가 있다면 거리 계산
        {
            lureDistance = Vector3.Distance(transform.position, lureTarget.position);
        }
        else                    // 없다면 break
        {
            return;
        }

        if (lureDistance < detectionRange)  // 루어가 감지거리보다 가깝다면 실행
        {
            float rand = Random.Range(0f, 1f);  // 랜덤값 설정
            if (rand <= newDetectionProbability)   // 랜덤값이 감지확률보다 작다면(=감지확률만큼)
            {
                //isAttracted = true;         // 유인당하지 않음
                //debugText.GetComponent<Text>().text = "Fish State Changed: NORMAL -> ATTRACTED";
                //lureTarget.gameObject.GetComponent<LureSetting>().AttractFishImage(); // 루어 표식 유인상태로 변경
                statusImage.AttractFishImage(); // 물고기 상태UI 유인상태로 변경
                state = fishState.ATTRACTED;    // 물고기 상태를 유인상태로 변경
            }
            else                                // 유인에 실패했다면 일반상태로 돌아감
            {
                //isAttracted = false;
                attractFishAgain = false;
                //debugText.GetComponent<Text>().text = "Fish State Changed: MISS LURE";
                //lureTarget.gameObject.GetComponent<LureSetting>().ResetImage(); // 루어 표식 리셋
                statusImage.ResetImage();   // 상태UI 초기화
                return;
            }
        }
    }

    // ATTACTED 상태에서 실행됨
    void MoveToLure()
    {   
        GameObject lureObj = GameObject.FindWithTag("Lure");
        if (lureObj != null && lureObj.GetComponent<LureSetting>().biteByFish == false)
        {   
            lureTarget = lureObj.transform;

            if (Vector3.Distance(agent.destination, lureTarget.position) > 0.2f) agent.SetDestination(lureTarget.position);  // 루어로 목표 변경
            float distance = Vector3.Distance(transform.position, lureTarget.position);

            Vector3 lookDir = (lureTarget.position - transform.position).normalized;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 0.2f);
            }   // 루어를 바라봄

            if (distance < snapDistance && isBiting == false)    // 일정거리 이내로 들어오면 잡힙(snap) 
            {
                StartCoroutine(FishBite()); // 입질 시간 후 미끼를 물음
            }
        }
        else    // 루어가 없다거나, 이미 다른 물고기가 물었다면 일반 상태로 되돌아감
        {
            //isAttracted = false;
            state = fishState.NORMAL;
            statusImage.ResetImage();   // 상태UI 초기화
            //debugText.GetComponent<Text>().text = "Fish State Changed: ATTRACTED -> NORMAL";
            return;
        }
    }

    IEnumerator FishBite()
    {
        isBiting = true;
        agent.isStopped = true;
        yield return new WaitForSeconds(fishBiteLureDelay); // 루어 근처까지 오면 일정시간 대기 후 루어 물음 

        Vector3 snapPosition = lureTarget.position;
        snapPosition.z -= MousePositionAdjustment;  // 입 위치 고려한 보정위치
        agent.Warp(snapPosition);   // 해당 위치로 강제이동
        transform.LookAt(lureTarget);   // 루어 보도록

        //lureTarget.gameObject.GetComponent<LureSetting>().ResistFishImage();    // 루어 표식 저항상태로 변경
        statusImage.ResistFishImage();  // 상태Ui를 저항상태로 변경
        lureTarget.gameObject.GetComponent<LureSetting>().biteByFish = true;    // 루어의 속성 변경
        resistTimer = 0f;
        //debugText.GetComponent<Text>().text = "Fish State Changed: ATTRACTED -> RESIST";

        state = fishState.RESIST;
    }

    void Captured()
    {
        if (agent != null)
        {
            agent.enabled = false;
        }
        else return;    // 에러리턴

        if (isCaptured == false)    // 코루틴 연속호출 방지
        {   
            if (GameManager.instance != null)
                GameManager.instance.connectLineWithRod(this.transform);    // 낚싯줄과 오브젝트 연결 
            statusImage.ResetImage();   // 상태UI 초기화
            Vector3 jumpTarget = transform.position + Vector3.up * jumpHeight;
            StartCoroutine(JumpToPosition(jumpTarget, 0.6f));
        }
    }

    IEnumerator JumpToPosition(Vector3 targetPos, float duration)
    {
        isCaptured = true;  // 잡혔을때의 bool
        Vector3 start = transform.position;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);   // 진행 시간에 따라 0~1 사이의 값 리턴
            transform.position = Vector3.Lerp(start, targetPos, t); // 시작점과 목표점 사이의 위치
            yield return null;
        }
        FishTypeComponent fishTypeComponent = gameObject.GetComponent<FishTypeComponent>();

        if (fishTypeComponent != null)
        {
            FishType fishType;
            Sprite fishImage;
            Sprite fishTierImage;
            int fishScore;
            fishTypeComponent.ReturnFishInfo(out fishType, out fishImage, out fishTierImage, out fishScore);

            if (GameManager.instance != null)
            {
                GameManager.instance.SendCapturedFishInfoToUI(fishType, fishImage, fishTierImage, fishScore);  // 게임메니저 호출
                GameManager.instance.connectLineWithRod(null);    // 낚싯줄과 연결 해재
            }
        }
        gameObject.SetActive(false);    // 오브젝트 비활성화
    }
}
