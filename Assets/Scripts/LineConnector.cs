using UnityEngine;

public class LineConnector : MonoBehaviour
{
    public Transform objectA = null;
    public Transform objectB = null;
    private LineRenderer lineRenderer;

    void Start()
    {
        // LineRenderer 컴포넌트 확인, 없다면 추가
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        
        lineRenderer.positionCount = 2;
        //lineRenderer.startWidth = 0.02f;
        //lineRenderer.endWidth = 0.02f;
        //lineRenderer.material = new Material(Shader.Find("Sprites/Default"));  // 기본 쉐이더
        //lineRenderer.startColor = Color.cyan;
        //lineRenderer.endColor = Color.cyan;
    }

    // Update is called once per frame
    void Update()
    {
        if (objectA != null && objectB != null)
        {
            if (lineRenderer.enabled == false)
                lineRenderer.enabled = true;

            lineRenderer.SetPosition(0, objectA.position);
            lineRenderer.SetPosition(1, objectB.position);
        }
        else
        {
            lineRenderer.enabled = false;   
        }
    }
}
