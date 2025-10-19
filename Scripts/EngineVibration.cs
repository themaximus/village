using UnityEngine;

public class EngineVibration : MonoBehaviour
{
    [Header("��������� ��������")]
    [Tooltip("��� ������ ������ ��������� �� X � Y (� ������)")]
    public float positionShakeAmount = 0.02f;

    [Tooltip("��� ������ ������ �������������� (� ��������)")]
    public float rotationShakeAmount = 0.1f;

    [Tooltip("��� ������ ���������� ��������. ��� '����' ����� ������� ��������.")]
    public float shakeSpeed = 40.0f;

    // �������� (�������) ��������
    private Vector3 basePosition;
    private Quaternion baseRotation;

    // ��������� "�����" ��� ����, ����� �������� � �������
    // �� ���� ����������� (� ����� ������ ��������� �������� ��-�������)
    private float noiseOffsetX;
    private float noiseOffsetY;
    private float noiseOffsetZ;

    void Start()
    {
        // ���������� �������� ��������� � �������
        // ����� ������������ localPosition/localRotation,
        // ���� ������ �������� �������� ��������
        basePosition = transform.localPosition;
        baseRotation = transform.localRotation;

        // ���������� ��������� ��������� ����� ��� ����
        float randomSeed = Random.Range(0f, 1000f);
        noiseOffsetX = randomSeed;
        noiseOffsetY = randomSeed + 100f; // �������, ����� Y �� ��� ����� X
        noiseOffsetZ = randomSeed + 200f; // �������, ����� Z �� ��� ����� X ��� Y
    }

    void Update()
    {
        // --- 1. ��������� �������� ---

        // Time.time * shakeSpeed - ��� ��, ��� ������ �� "�����" �� ����� ����
        // noiseOffsetX - ��� "�������" �� ����� ����, ������� �� ������
        float xNoise = Mathf.PerlinNoise(Time.time * shakeSpeed, noiseOffsetX);
        float yNoise = Mathf.PerlinNoise(Time.time * shakeSpeed, noiseOffsetY);

        // Mathf.PerlinNoise ���������� [0, 1].
        // ��� ����� [-1, 1], ����� �� ��� �������� � ��� �������.
        // (xNoise * 2f - 1f) ������ ��� ��������������.
        // ����� �������� �� ���� ���� (Amount).

        float xOffset = (xNoise * 2f - 1f) * positionShakeAmount;
        float yOffset = (yNoise * 2f - 1f) * positionShakeAmount;

        // ��������� �������� � ������� �������
        transform.localPosition = basePosition + new Vector3(xOffset, yOffset, 0);


        // --- 2. ��������� ������� ---

        float zRotNoise = Mathf.PerlinNoise(Time.time * shakeSpeed, noiseOffsetZ);

        // ����������� [0, 1] � [-1, 1] � �������� �� ���� � ��������
        float zOffset = (zRotNoise * 2f - 1f) * rotationShakeAmount;

        // ��������� ������� � ��������.
        // Quaternion.Euler(0, 0, zOffset) ������� "�������-��������"
        // �� �������� ��� �� baseRotation, ����� "��������" ���
        transform.localRotation = baseRotation * Quaternion.Euler(0, 0, zOffset);
    }
}