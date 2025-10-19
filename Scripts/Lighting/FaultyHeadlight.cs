using UnityEngine;
// ��� ������������ ���� ����� ��� ������ � 2D ������ � URP
using UnityEngine.Rendering.Universal;
// ��� ������������ ���� ����� ��� Coroutines
using System.Collections;

[RequireComponent(typeof(Light2D))]
public class FaultyLightSync : MonoBehaviour
{
    [Header("�������������")]
    [Tooltip("���������� ���� ������ ��������� �����, ������� ������ ������ ���������")]
    public Light2D[] otherLightsToSync;

    [Header("���������")]
    [Tooltip("�������� �� ����?")]
    public bool isLightOn = true;

    [Tooltip("���������� ������� ����, ����� ��� �� �����")]
    public float baseIntensity = 1.5f;

    [Header("��������� ����")]
    [Tooltip("����������� ����� ����� ������ (������)")]
    public float minTimeBetweenGlitches = 3.0f;
    [Tooltip("������������ ����� ����� ������ (������)")]
    public float maxTimeBetweenGlitches = 15.0f;

    [Space]
    [Tooltip("����������� ������������ ������ ���� (������)")]
    public float minGlitchDuration = 0.1f;
    [Tooltip("������������ ������������ ������ ���� (������)")]
    public float maxGlitchDuration = 0.4f;

    [Header("������� �� ����� ����")]
    [Tooltip("����������� ������� �� ����� '��������� ���������'")]
    public float minGlitchIntensity = 0.0f;
    [Tooltip("������������ ������� �� ����� '��������� ���������'")]
    public float maxGlitchIntensity = 0.5f;

    // --- ��������� ���������� ---
    private Light2D mainLight; // "�������" ���� (�� ���� �������)
    private float glitchTimer;
    private bool isGlitching = false;
    private bool lastKnownLightState; // ��� ������������ ��������� � ����������

    void Start()
    {
        // 1. ������� "�������" ����
        mainLight = GetComponent<Light2D>();

        // 2. ������������� ��������� ���������
        lastKnownLightState = isLightOn;
        ApplyBaseStateToAll();

        // 3. ��������� ������
        ResetGlitchTimer();
    }

    void Update()
    {
        // ���������, �� ������� �� ������������ 'isLightOn' � ����������
        if (isLightOn != lastKnownLightState)
        {
            lastKnownLightState = isLightOn;

            if (!isLightOn)
            {
                // ���� ���� ���������, ������������� ������������� ��� ����
                StopAllCoroutines(); // ������������� �����������
                isGlitching = false;
                ResetGlitchTimer(); // ���������� ������ �� ������ ������
            }
            // ��������� ����� ��������� (��� ��� ����) �� ����
            ApplyBaseStateToAll();
        }

        // ���� ���� ��������� ��� ��� ����� - ������ �� ������
        if (!isLightOn || isGlitching) return;

        // ������ ������� �� ����
        glitchTimer -= Time.deltaTime;

        if (glitchTimer <= 0)
        {
            // ����� ������!
            StartCoroutine(FlickerGlitchSequence());
        }
    }

    // �����������, ���������� �� ��� ������� "�������"
    IEnumerator FlickerGlitchSequence()
    {
        isGlitching = true;

        float glitchDuration = Random.Range(minGlitchDuration, maxGlitchDuration);
        float timer = 0;

        while (timer < glitchDuration)
        {
            // 1. �������� ���� ��������� ������� ��� ����
            float randomIntensity = Random.Range(minGlitchIntensity, maxGlitchIntensity);

            // 2. ��������� �� �� ����
            ApplyIntensityToAll(randomIntensity);

            // 3. ����
            float flickerDelay = Random.Range(0.02f, 0.07f);
            yield return new WaitForSeconds(flickerDelay);

            timer += flickerDelay;
        }

        // ���� �������
        isGlitching = false;
        ApplyBaseStateToAll(); // ���������� ���������� �������
        ResetGlitchTimer(); // ��������� ������ �� ���������� ����
    }

    // --- ��������������� ������� ---

    /// <summary>
    /// ��������� ������� ������� (��� 0) �� ���� �����
    /// </summary>
    void ApplyBaseStateToAll()
    {
        float targetIntensity = isLightOn ? baseIntensity : 0;
        ApplyIntensityToAll(targetIntensity);
    }

    /// <summary>
    /// ������������� ���������� ������� ���� �����
    /// </summary>
    void ApplyIntensityToAll(float intensity)
    {
        // 1. ��������� � "�������" ����
        if (mainLight != null)
        {
            mainLight.intensity = intensity;
        }

        // 2. ��������� �� ���� "�������" ����� �� ������
        foreach (Light2D light in otherLightsToSync)
        {
            if (light != null) // ��������, �� ������ ���� �� ������� ����,
            {                  // � �� ������ ������ ������
                light.intensity = intensity;
            }
        }
    }

    void ResetGlitchTimer()
    {
        glitchTimer = Random.Range(minTimeBetweenGlitches, maxTimeBetweenGlitches);
    }
}