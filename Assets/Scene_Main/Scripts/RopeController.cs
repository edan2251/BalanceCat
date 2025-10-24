using UnityEngine;
using System.Collections.Generic;

public class RopeController : MonoBehaviour
{
    public Transform anchorPoint;
    public Transform connectedObject;
    public LineRenderer lineRenderer;

    [Header("Rope & Curve Settings")]
    public int controlPointCount = 4;
    public float maxSag = 1f;
    public int curveSegments = 20;
    public float smoothness = 0.5f;

    private Rigidbody2D connectedRb;
    private Vector3 currentSagDirection = Vector3.zero;
    public float sagDirectionSmoothness = 5f; // 처짐 방향이 바뀌는 부드러움 정도

    void Start()
    {
        // Rigidbody2D 컴포넌트 가져오기
        if (connectedObject != null)
        {
            connectedRb = connectedObject.GetComponent<Rigidbody2D>();
        }
    }

    void Update()
    {
        // 1. Z 좌표 통일
        if (connectedObject.position.z != anchorPoint.position.z)
        {
            connectedObject.position = new Vector3(
                connectedObject.position.x,
                connectedObject.position.y,
                anchorPoint.position.z
            );
        }

        Vector3 startPos = anchorPoint.position;
        Vector3 endPos = connectedObject.position;

        Vector3 calculatedPerpendicular = CalculateDynamicSagDirection(startPos, endPos);

        currentSagDirection = Vector3.Lerp(currentSagDirection, calculatedPerpendicular, Time.deltaTime * sagDirectionSmoothness);
        Vector3 finalPerpendicular = currentSagDirection.normalized; 

        List<Vector3> pathPoints = new List<Vector3>();
        pathPoints.Add(anchorPoint.position);

        for (int i = 1; i <= controlPointCount; i++)
        {
            float t = (float)i / (controlPointCount + 1);
            Vector3 linearPosition = Vector3.Lerp(startPos, endPos, t);
            float sagFactor = 4f * (t - t * t);

            Vector3 sagVector = finalPerpendicular * maxSag * sagFactor;

            pathPoints.Add(linearPosition + sagVector);
        }

        pathPoints.Add(connectedObject.position);

        if (pathPoints.Count < 2) return;

        lineRenderer.positionCount = curveSegments;

        for (int i = 0; i < curveSegments; i++)
        {
            float t = (float)i / (curveSegments - 1);
            Vector3 point = GetCatmullRomPosition(t, pathPoints.ToArray(), smoothness);
            lineRenderer.SetPosition(i, point);
        }
    }

    Vector3 CalculateDynamicSagDirection(Vector3 start, Vector3 end)
    {
        float distance = Vector3.Distance(start, end);
        float minRopeLengthForSag = 0.5f; 

        if (distance < minRopeLengthForSag)
        {
            return Vector3.zero;
        }

        if (connectedRb != null && connectedRb.velocity.magnitude > 0.05f)
        {
            return -connectedRb.velocity.normalized;
        }

        return Vector3.zero;
    }


    Vector3 GetCatmullRomPosition(float t, Vector3[] points, float alpha)
    {
        if (t <= 0f) return points[0];
        if (t >= 1f) return points[points.Length - 1];

        float numPointsF = (float)points.Length - 1;
        float segmentF = t * numPointsF;
        int segment = Mathf.FloorToInt(segmentF);
        float segmentT = segmentF - segment;

        int p0Index = Mathf.Clamp(segment - 1, 0, points.Length - 1);
        int p1Index = Mathf.Clamp(segment, 0, points.Length - 1);
        int p2Index = Mathf.Clamp(segment + 1, 0, points.Length - 1);
        int p3Index = Mathf.Clamp(segment + 2, 0, points.Length - 1);

        Vector3 p0 = points[p0Index];
        Vector3 p1 = points[p1Index];
        Vector3 p2 = points[p2Index];
        Vector3 p3 = points[p3Index];

        float tt = segmentT * segmentT;
        float ttt = tt * segmentT;

        float x = 0.5f * (p1.x * 2.0f + (-p0.x + p2.x) * segmentT + (2.0f * p0.x - 5.0f * p1.x + 4.0f * p2.x - p3.x) * tt + (-p0.x + 3.0f * p1.x - 3.0f * p2.x + p3.x) * ttt);
        float y = 0.5f * (p1.y * 2.0f + (-p0.y + p2.y) * segmentT + (2.0f * p0.y - 5.0f * p1.y + 4.0f * p2.y - p3.y) * tt + (-p0.y + 3.0f * p1.y - 3.0f * p2.y + p3.y) * ttt);

        return new Vector3(x, y, p1.z);
    }
}