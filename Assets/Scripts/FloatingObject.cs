using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    [Tooltip("������Ʈ�� ���Ʒ��� �����̴� �ִ� �����Դϴ�.")]
    public float amplitude = 0.2f;

    [Tooltip("������Ʈ�� ���Ʒ��� �����̴� �ӵ��Դϴ�.")]
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