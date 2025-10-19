using UnityEngine;
// Это пространство имен нужно для работы с 2D светом в URP
using UnityEngine.Rendering.Universal;
// Это пространство имен нужно для Coroutines
using System.Collections;

[RequireComponent(typeof(Light2D))]
public class LightningController : MonoBehaviour
{
    private Light2D lightningLight;
    private float strikeTimer; // Таймер до следующего удара
    private bool isStriking = false; // Флаг, что молния бьет прямо сейчас

    [Header("Настройки Времени (в секундах)")]
    [Tooltip("Минимальное время ожидания между ударами молнии")]
    public float minTimeBetweenStrikes = 5.0f;
    [Tooltip("Максимальное время ожидания между ударами молнии")]
    public float maxTimeBetweenStrikes = 20.0f;

    [Header("Настройки Вспышки")]
    [Tooltip("Минимальная яркость вспышки")]
    public float minIntensity = 3.0f;
    [Tooltip("Максимальная яркость вспышки")]
    public float maxIntensity = 7.0f;

    [Tooltip("Сколько раз молния 'мигнет' за один удар (минимум)")]
    public int minFlashes = 2;
    [Tooltip("Сколько раз молния 'мигнет' за один удар (максимум)")]
    public int maxFlashes = 5;


    void Start()
    {
        lightningLight = GetComponent<Light2D>();
        // Убедимся, что свет выключен вначале
        lightningLight.intensity = 0;

        // Запускаем таймер в первый раз
        ResetStrikeTimer();
    }

    void Update()
    {
        // Если молния уже бьет, мы не считаем таймер
        if (isStriking) return;

        // Ведем обратный отсчет
        strikeTimer -= Time.deltaTime;

        // Если время вышло
        if (strikeTimer <= 0)
        {
            // Запускаем Coroutine (сопрограмму) для самой вспышки
            StartCoroutine(LightningStrikeSequence());
        }
    }

    // Сбрасывает таймер на новое случайное значение
    void ResetStrikeTimer()
    {
        strikeTimer = Random.Range(minTimeBetweenStrikes, maxTimeBetweenStrikes);
    }

    // IEnumerator - это тип, необходимый для Coroutine.
    // Это функция, которая может "ставить себя на паузу".
    IEnumerator LightningStrikeSequence()
    {
        isStriking = true; // Сообщаем, что мы в процессе удара

        // Выбираем, сколько коротких вспышек будет в этом ударе
        int flashCount = Random.Range(minFlashes, maxFlashes + 1);

        // Цикл вспышек
        for (int i = 0; i < flashCount; i++)
        {
            // 1. Вспышка (ВКЛ)
            lightningLight.intensity = Random.Range(minIntensity, maxIntensity);
            // 2. Пауза (оставляем свет включенным на очень короткое время)
            yield return new WaitForSeconds(Random.Range(0.03f, 0.1f));

            // 3. Пауза (ВЫКЛ)
            lightningLight.intensity = 0;
            // 4. Пауза (оставляем свет выключенным)
            yield return new WaitForSeconds(Random.Range(0.02f, 0.07f));
        }

        // --- (Опционально) Можно добавить одну большую финальную вспышку ---
        // lightningLight.intensity = maxIntensity * 1.5f; // Супер-яркая
        // yield return new WaitForSeconds(0.1f);
        // lightningLight.intensity = 0;
        // ------------------------------------------------------------------

        // Удар завершен
        isStriking = false;
        ResetStrikeTimer(); // Сразу сбрасываем таймер до следующего удара
    }
}