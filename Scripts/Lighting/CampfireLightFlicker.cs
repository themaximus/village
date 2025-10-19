using UnityEngine;
// Это пространство имен нужно для работы с 2D светом в URP
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class CampfireLightFlicker : MonoBehaviour
{
    private Light2D lightSource;

    // Исходные значения
    private float baseIntensity;
    private Vector3 basePosition;

    [Header("Настройки Интенсивности")]
    [Tooltip("Насколько сильно меняется яркость (0 = нет)")]
    public float intensityFlickerAmount = 0.2f;
    [Tooltip("Как быстро меняется яркость")]
    public float intensityFlickerSpeed = 3.0f;

    [Header("Настройки Движения")]
    [Tooltip("Насколько сильно смещается свет (0 = нет)")]
    public float positionFlickerAmount = 0.1f;
    [Tooltip("Как быстро смещается свет")]
    public float positionFlickerSpeed = 2.0f;

    // Случайные "зерна" для шума, чтобы разные костры вели себя по-разному
    private float intensityNoiseOffset;
    private float positionXNoiseOffset;
    private float positionYNoiseOffset;


    void Start()
    {
        lightSource = GetComponent<Light2D>();

        // Запоминаем базовые значения
        baseIntensity = lightSource.intensity;
        basePosition = transform.localPosition;

        // Генерируем случайные стартовые точки для шума
        intensityNoiseOffset = Random.Range(0f, 1000f);
        positionXNoiseOffset = Random.Range(0f, 1000f);
        positionYNoiseOffset = Random.Range(0f, 1000f);
    }

    void Update()
    {
        // --- 1. Мерцание Интенсивности ---
        if (intensityFlickerAmount > 0)
        {
            // Получаем значение шума (оно всегда между 0.0 и 1.0)
            float noiseValue = Mathf.PerlinNoise(Time.time * intensityFlickerSpeed, intensityNoiseOffset);

            // Превращаем диапазон [0, 1] в [-1, 1] и умножаем на нашу силу
            float intensityFlicker = (noiseValue * 2.0f - 1.0f) * intensityFlickerAmount;

            // Применяем к базовой интенсивности
            lightSource.intensity = baseIntensity + intensityFlicker;
        }

        // --- 2. Движение (смещение) ---
        if (positionFlickerAmount > 0)
        {
            // Получаем два разных значения шума для X и Y
            float xNoise = Mathf.PerlinNoise(Time.time * positionFlickerSpeed, positionXNoiseOffset);
            float yNoise = Mathf.PerlinNoise(Time.time * positionFlickerSpeed, positionYNoiseOffset);

            // Превращаем [0, 1] в [-1, 1] и умножаем на силу
            float xFlicker = (xNoise * 2.0f - 1.0f) * positionFlickerAmount;
            float yFlicker = (yNoise * 2.0f - 1.0f) * positionFlickerAmount;

            // Применяем к базовой позиции. 
            // Важно использовать localPosition, если свет - дочерний объект.
            transform.localPosition = basePosition + new Vector3(xFlicker, yFlicker, 0);
        }
    }
}