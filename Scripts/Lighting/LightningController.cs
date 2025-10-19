using UnityEngine;
// ��� ������������ ���� ����� ��� ������ � 2D ������ � URP
using UnityEngine.Rendering.Universal;
// ��� ������������ ���� ����� ��� Coroutines
using System.Collections;

[RequireComponent(typeof(Light2D))]
public class LightningController : MonoBehaviour
{
    private Light2D lightningLight;
    private float strikeTimer; // ������ �� ���������� �����
    private bool isStriking = false; // ����, ��� ������ ���� ����� ������

    [Header("��������� ������� (� ��������)")]
    [Tooltip("����������� ����� �������� ����� ������� ������")]
    public float minTimeBetweenStrikes = 5.0f;
    [Tooltip("������������ ����� �������� ����� ������� ������")]
    public float maxTimeBetweenStrikes = 20.0f;

    [Header("��������� �������")]
    [Tooltip("����������� ������� �������")]
    public float minIntensity = 3.0f;
    [Tooltip("������������ ������� �������")]
    public float maxIntensity = 7.0f;

    [Tooltip("������� ��� ������ '������' �� ���� ���� (�������)")]
    public int minFlashes = 2;
    [Tooltip("������� ��� ������ '������' �� ���� ���� (��������)")]
    public int maxFlashes = 5;


    void Start()
    {
        lightningLight = GetComponent<Light2D>();
        // ��������, ��� ���� �������� �������
        lightningLight.intensity = 0;

        // ��������� ������ � ������ ���
        ResetStrikeTimer();
    }

    void Update()
    {
        // ���� ������ ��� ����, �� �� ������� ������
        if (isStriking) return;

        // ����� �������� ������
        strikeTimer -= Time.deltaTime;

        // ���� ����� �����
        if (strikeTimer <= 0)
        {
            // ��������� Coroutine (�����������) ��� ����� �������
            StartCoroutine(LightningStrikeSequence());
        }
    }

    // ���������� ������ �� ����� ��������� ��������
    void ResetStrikeTimer()
    {
        strikeTimer = Random.Range(minTimeBetweenStrikes, maxTimeBetweenStrikes);
    }

    // IEnumerator - ��� ���, ����������� ��� Coroutine.
    // ��� �������, ������� ����� "������� ���� �� �����".
    IEnumerator LightningStrikeSequence()
    {
        isStriking = true; // ��������, ��� �� � �������� �����

        // ��������, ������� �������� ������� ����� � ���� �����
        int flashCount = Random.Range(minFlashes, maxFlashes + 1);

        // ���� �������
        for (int i = 0; i < flashCount; i++)
        {
            // 1. ������� (���)
            lightningLight.intensity = Random.Range(minIntensity, maxIntensity);
            // 2. ����� (��������� ���� ���������� �� ����� �������� �����)
            yield return new WaitForSeconds(Random.Range(0.03f, 0.1f));

            // 3. ����� (����)
            lightningLight.intensity = 0;
            // 4. ����� (��������� ���� �����������)
            yield return new WaitForSeconds(Random.Range(0.02f, 0.07f));
        }

        // --- (�����������) ����� �������� ���� ������� ��������� ������� ---
        // lightningLight.intensity = maxIntensity * 1.5f; // �����-�����
        // yield return new WaitForSeconds(0.1f);
        // lightningLight.intensity = 0;
        // ------------------------------------------------------------------

        // ���� ��������
        isStriking = false;
        ResetStrikeTimer(); // ����� ���������� ������ �� ���������� �����
    }
}