using UnityEngine;
using UnityEngine.UI;

public class LureSetting : MonoBehaviour
{
    //GameObject debugText;
    public bool biteByFish = false;
    public bool hookUp = false;

    // UI
    public GameObject attractImage;
    public GameObject resistImage;

    // Fishing Line
    private GameObject lineObject;
    private LineConnector lineConnector;

    // Rigidbody
    private Rigidbody rb;

    private void OnEnable()
    {
        //debugText = GameObject.Find("DebugText");
        //debugText.GetComponent<Text>().text = "lure enabled";
        attractImage.SetActive(false);
        resistImage.SetActive(false);

        // 낚싯줄 관련
        if (GameManager.instance != null)
            GameManager.instance.connectLineWithRod(this.transform);

        // arplane에서 정지키기 위한 리지드바디
        rb = gameObject.GetComponent<Rigidbody>();

        ResetPhysics();
        biteByFish = false;
        hookUp = false; 
    }

    private void OnDisable()
    {
        //debugText.GetComponent<Text>().text = "lure Disabled";
        if (GameManager.instance != null)
            GameManager.instance.connectLineWithRod(null);
    }

    private void ResetPhysics()
    {
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.Sleep();
        }
    }

    void OnCollisionEnter(Collision collision)  // 바닥과 만났다면 그곳에서 움직이지 않음
    {
        if (collision.gameObject.CompareTag("ARPlane"))
        {
            rb.isKinematic = true;
            ResetPhysics();
        }
    }
}
