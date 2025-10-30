using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    [Tooltip("오브젝트가 위아래로 움직이는 최대 높이입니다.")]
    public float amplitude = 0.2f;

    [Tooltip("오브젝트가 위아래로 움직이는 속도입니다.")]
    public float speed = 1.0f;

    private float startY;

    private float timeOffset;

    void Start()
    {
        startY = transform.position.y;

        timeOffset = Random.Range(0f, 2f * Mathf.PI);
    }

    void Update()
    {
        float sinWave = Mathf.Sin(Time.time * speed + timeOffset);
        float newY = startY + sinWave * amplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}