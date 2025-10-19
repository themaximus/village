using UnityEngine;
// Это пространство имен нужно для работы с 2D светом в URP
using UnityEngine.Rendering.Universal;
// Это пространство имен нужно для Coroutines
using System.Collections;

[RequireComponent(typeof(Light2D))]
public class FaultyLightSync : MonoBehaviour
{
    [Header("Синхронизация")]
    [Tooltip("Перетащите сюда ДРУГИЕ источники света, которые должны мигать синхронно")]
    public Light2D[] otherLightsToSync;

    [Header("Состояние")]
    [Tooltip("Включены ли фары?")]
    public bool isLightOn = true;

    [Tooltip("Нормальная яркость фары, когда она не сбоит")]
    public float baseIntensity = 1.5f;

    [Header("Настройки сбоя")]
    [Tooltip("Минимальное время МЕЖДУ сбоями (секунд)")]
    public float minTimeBetweenGlitches = 3.0f;
    [Tooltip("Максимальное время МЕЖДУ сбоями (секунд)")]
    public float maxTimeBetweenGlitches = 15.0f;

    [Space]
    [Tooltip("Минимальная длительность самого сбоя (секунд)")]
    public float minGlitchDuration = 0.1f;
    [Tooltip("Максимальная длительность самого сбоя (секунд)")]
    public float maxGlitchDuration = 0.4f;

    [Header("Яркость во время сбоя")]
    [Tooltip("Минимальная яркость во время 'короткого замыкания'")]
    public float minGlitchIntensity = 0.0f;
    [Tooltip("Максимальная яркость во время 'короткого замыкания'")]
    public float maxGlitchIntensity = 0.5f;

    // --- Приватные переменные ---
    private Light2D mainLight; // "Главный" свет (на этом объекте)
    private float glitchTimer;
    private bool isGlitching = false;
    private bool lastKnownLightState; // Для отслеживания изменений в инспекторе

    void Start()
    {
        // 1. Находим "главный" свет
        mainLight = GetComponent<Light2D>();

        // 2. Устанавливаем начальное состояние
        lastKnownLightState = isLightOn;
        ApplyBaseStateToAll();

        // 3. Запускаем таймер
        ResetGlitchTimer();
    }

    void Update()
    {
        // Проверяем, не изменил ли пользователь 'isLightOn' в инспекторе
        if (isLightOn != lastKnownLightState)
        {
            lastKnownLightState = isLightOn;

            if (!isLightOn)
            {
                // Если фары выключили, принудительно останавливаем все сбои
                StopAllCoroutines(); // Останавливаем сопрограмму
                isGlitching = false;
                ResetGlitchTimer(); // Сбрасываем таймер на всякий случай
            }
            // Применяем новое состояние (ВКЛ или ВЫКЛ) ко всем
            ApplyBaseStateToAll();
        }

        // Если фары выключены или уже сбоят - ничего не делаем
        if (!isLightOn || isGlitching) return;

        // Отсчет таймера до сбоя
        glitchTimer -= Time.deltaTime;

        if (glitchTimer <= 0)
        {
            // Время пришло!
            StartCoroutine(FlickerGlitchSequence());
        }
    }

    // Сопрограмма, отвечающая за сам процесс "мигания"
    IEnumerator FlickerGlitchSequence()
    {
        isGlitching = true;

        float glitchDuration = Random.Range(minGlitchDuration, maxGlitchDuration);
        float timer = 0;

        while (timer < glitchDuration)
        {
            // 1. Выбираем ОДНУ случайную яркость для ВСЕХ
            float randomIntensity = Random.Range(minGlitchIntensity, maxGlitchIntensity);

            // 2. Применяем ее ко всем
            ApplyIntensityToAll(randomIntensity);

            // 3. Ждем
            float flickerDelay = Random.Range(0.02f, 0.07f);
            yield return new WaitForSeconds(flickerDelay);

            timer += flickerDelay;
        }

        // Сбой окончен
        isGlitching = false;
        ApplyBaseStateToAll(); // Возвращаем нормальную яркость
        ResetGlitchTimer(); // Запускаем таймер до СЛЕДУЮЩЕГО сбоя
    }

    // --- Вспомогательные функции ---

    /// <summary>
    /// Применяет базовую яркость (или 0) ко всем фарам
    /// </summary>
    void ApplyBaseStateToAll()
    {
        float targetIntensity = isLightOn ? baseIntensity : 0;
        ApplyIntensityToAll(targetIntensity);
    }

    /// <summary>
    /// Устанавливает ОДИНАКОВУЮ яркость всем фарам
    /// </summary>
    void ApplyIntensityToAll(float intensity)
    {
        // 1. Применяем к "главной" фаре
        if (mainLight != null)
        {
            mainLight.intensity = intensity;
        }

        // 2. Применяем ко всем "ведомым" фарам из списка
        foreach (Light2D light in otherLightsToSync)
        {
            if (light != null) // Проверка, на случай если вы удалили фару,
            {                  // а из списка убрать забыли
                light.intensity = intensity;
            }
        }
    }

    void ResetGlitchTimer()
    {
        glitchTimer = Random.Range(minTimeBetweenGlitches, maxTimeBetweenGlitches);
    }
}