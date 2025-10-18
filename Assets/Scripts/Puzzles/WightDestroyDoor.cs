using UnityEngine;
using System.Collections; // �ڷ�ƾ ����� ���� �߰�

// WeightSensor �̺�Ʈ�� �����Ͽ� ���� �ı�(��Ȱ��ȭ)�ϴ� ������Ʈ
public class WeightDoorDestructible : MonoBehaviour
{
    [Header("�ı� ����")]
    [Tooltip("�ı� ȿ���� ���� ��ƼŬ �ý��� (�ɼ�)")]
    public ParticleSystem destructionEffect;

    [Tooltip("�ı�(��Ȱ��ȭ)�Ǳ������ ������ �ð�")]
    public float destructionDelay = 0.5f;

    private bool isDestroyed = false;

    public void DestroyDoor()
    {
        if (isDestroyed) return; // �̹� �ı��� ��� �ߺ� ���� ����

        isDestroyed = true;
        StartCoroutine(DestructionSequence());
    }

    private IEnumerator DestructionSequence()
    {
        // 1. �ı� �� ������
        yield return new WaitForSeconds(destructionDelay);

        // 2. ��ƼŬ ȿ�� ��� (����Ǿ� �ִٸ�)
        if (destructionEffect != null)
        {
            // �� ��ġ���� ��ƼŬ ���
            destructionEffect.transform.position = transform.position;
            destructionEffect.Play();
        }

        // 3. �� ������Ʈ ��Ȱ��ȭ (������ �ʰ� ��)
        gameObject.SetActive(false);

        // 4. (������) ��ƼŬ�� ���� �� ������ ���� (��� ����)
        if (destructionEffect != null)
        {
            Destroy(destructionEffect.gameObject, destructionEffect.main.duration);
        }
    }

}