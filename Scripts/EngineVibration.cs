using UnityEngine;

public class EngineVibration : MonoBehaviour
{
    [Header("Настройки Вибрации")]
    [Tooltip("Как сильно спрайт смещается по X и Y (в юнитах)")]
    public float positionShakeAmount = 0.02f;

    [Tooltip("Как сильно спрайт поворачивается (в градусах)")]
    public float rotationShakeAmount = 0.1f;

    [Tooltip("Как быстро происходит вибрация. Для 'гула' нужно высокое значение.")]
    public float shakeSpeed = 40.0f;

    // Исходные (базовые) значения
    private Vector3 basePosition;
    private Quaternion baseRotation;

    // Случайные "зерна" для шума, чтобы смещение и поворот
    // не были синхронными (и чтобы разные грузовики тряслись по-разному)
    private float noiseOffsetX;
    private float noiseOffsetY;
    private float noiseOffsetZ;

    void Start()
    {
        // Запоминаем исходное положение и поворот
        // Важно использовать localPosition/localRotation,
        // если спрайт является дочерним объектом
        basePosition = transform.localPosition;
        baseRotation = transform.localRotation;

        // Генерируем случайные стартовые точки для шума
        float randomSeed = Random.Range(0f, 1000f);
        noiseOffsetX = randomSeed;
        noiseOffsetY = randomSeed + 100f; // Смещаем, чтобы Y не был равен X
        noiseOffsetZ = randomSeed + 200f; // Смещаем, чтобы Z не был равен X или Y
    }

    void Update()
    {
        // --- 1. Вычисляем смещение ---

        // Time.time * shakeSpeed - это то, как быстро мы "бежим" по карте шума
        // noiseOffsetX - это "строчка" на карте шума, которую мы читаем
        float xNoise = Mathf.PerlinNoise(Time.time * shakeSpeed, noiseOffsetX);
        float yNoise = Mathf.PerlinNoise(Time.time * shakeSpeed, noiseOffsetY);

        // Mathf.PerlinNoise возвращает [0, 1].
        // Нам нужно [-1, 1], чтобы он мог трястись в обе стороны.
        // (xNoise * 2f - 1f) делает это преобразование.
        // Затем умножаем на нашу силу (Amount).

        float xOffset = (xNoise * 2f - 1f) * positionShakeAmount;
        float yOffset = (yNoise * 2f - 1f) * positionShakeAmount;

        // Применяем смещение к базовой позиции
        transform.localPosition = basePosition + new Vector3(xOffset, yOffset, 0);


        // --- 2. Вычисляем поворот ---

        float zRotNoise = Mathf.PerlinNoise(Time.time * shakeSpeed, noiseOffsetZ);

        // Преобразуем [0, 1] в [-1, 1] и умножаем на силу в градусах
        float zOffset = (zRotNoise * 2f - 1f) * rotationShakeAmount;

        // Применяем поворот к базовому.
        // Quaternion.Euler(0, 0, zOffset) создает "поворот-смещение"
        // Мы умножаем его на baseRotation, чтобы "добавить" его
        transform.localRotation = baseRotation * Quaternion.Euler(0, 0, zOffset);
    }
}