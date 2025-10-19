using UnityEngine;
// ��� ������������ ���� ����� ��� ������ � 2D ������ � URP
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class CampfireLightFlicker : MonoBehaviour
{
    private Light2D lightSource;

    // �������� ��������
    private float baseIntensity;
    private Vector3 basePosition;

    [Header("��������� �������������")]
    [Tooltip("��������� ������ �������� ������� (0 = ���)")]
    public float intensityFlickerAmount = 0.2f;
    [Tooltip("��� ������ �������� �������")]
    public float intensityFlickerSpeed = 3.0f;

    [Header("��������� ��������")]
    [Tooltip("��������� ������ ��������� ���� (0 = ���)")]
    public float positionFlickerAmount = 0.1f;
    [Tooltip("��� ������ ��������� ����")]
    public float positionFlickerSpeed = 2.0f;

    // ��������� "�����" ��� ����, ����� ������ ������ ���� ���� ��-�������
    private float intensityNoiseOffset;
    private float positionXNoiseOffset;
    private float positionYNoiseOffset;


    void Start()
    {
        lightSource = GetComponent<Light2D>();

        // ���������� ������� ��������
        baseIntensity = lightSource.intensity;
        basePosition = transform.localPosition;

        // ���������� ��������� ��������� ����� ��� ����
        intensityNoiseOffset = Random.Range(0f, 1000f);
        positionXNoiseOffset = Random.Range(0f, 1000f);
        positionYNoiseOffset = Random.Range(0f, 1000f);
    }

    void Update()
    {
        // --- 1. �������� ������������� ---
        if (intensityFlickerAmount > 0)
        {
            // �������� �������� ���� (��� ������ ����� 0.0 � 1.0)
            float noiseValue = Mathf.PerlinNoise(Time.time * intensityFlickerSpeed, intensityNoiseOffset);

            // ���������� �������� [0, 1] � [-1, 1] � �������� �� ���� ����
            float intensityFlicker = (noiseValue * 2.0f - 1.0f) * intensityFlickerAmount;

            // ��������� � ������� �������������
            lightSource.intensity = baseIntensity + intensityFlicker;
        }

        // --- 2. �������� (��������) ---
        if (positionFlickerAmount > 0)
        {
            // �������� ��� ������ �������� ���� ��� X � Y
            float xNoise = Mathf.PerlinNoise(Time.time * positionFlickerSpeed, positionXNoiseOffset);
            float yNoise = Mathf.PerlinNoise(Time.time * positionFlickerSpeed, positionYNoiseOffset);

            // ���������� [0, 1] � [-1, 1] � �������� �� ����
            float xFlicker = (xNoise * 2.0f - 1.0f) * positionFlickerAmount;
            float yFlicker = (yNoise * 2.0f - 1.0f) * positionFlickerAmount;

            // ��������� � ������� �������. 
            // ����� ������������ localPosition, ���� ���� - �������� ������.
            transform.localPosition = basePosition + new Vector3(xFlicker, yFlicker, 0);
        }
    }
}