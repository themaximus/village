using UnityEngine;
using System;
using System.Collections.Generic; // Необходимо для Dictionary
using Newtonsoft.Json.Linq; // Необходимо для JObject

// "Подписываем контракт" ISaveable
public class StatController : MonoBehaviour, ISaveable
{
    [Header("Stat Configuration")]
    public CharacterStatSheet statSheet;

    [Header("Character Stats")]
    public CharacterStat Health;
    public CharacterStat Hunger;
    public CharacterStat Thirst;
    public CharacterStat Sanity;
    public CharacterStat Vigor;

    public event Action OnDeath;

    void Awake()
    {
        Health.OnValueChanged += HandleHealthChange;
    }

    void Start()
    {
        if (statSheet == null)
        {
            Debug.LogError("Stat Sheet is not assigned on " + gameObject.name);
            this.enabled = false;
            return;
        }

        Health.Initialize(statSheet.maxHealth);
        Hunger.Initialize(statSheet.maxHunger);
        Thirst.Initialize(statSheet.maxThirst);
        Sanity.Initialize(statSheet.maxSanity);
        Vigor.Initialize(statSheet.maxVigor);
    }

    void Update()
    {
        if (statSheet == null) return;

        Hunger.Remove(statSheet.hungerDecayRate * Time.deltaTime);
        Thirst.Remove(statSheet.thirstDecayRate * Time.deltaTime);
    }

    private void HandleHealthChange(float currentHealth, float maxHealth)
    {
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void TakeDamage(float damage)
    {
        Health.Remove(damage);
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " has died.");
        OnDeath?.Invoke();
    }

    void OnDestroy()
    {
        Health.OnValueChanged -= HandleHealthChange;
    }

    // --- РЕАЛИЗАЦИЯ ИНТЕРФЕЙСА ISaveable ---

    /// <summary>
    /// "Фотографирует" текущее состояние всех характеристик.
    /// </summary>
    public object CaptureState()
    {
        var statsData = new Dictionary<string, float>
        {
            { "Health", Health.CurrentValue },
            { "Hunger", Hunger.CurrentValue },
            { "Thirst", Thirst.CurrentValue },
            { "Sanity", Sanity.CurrentValue },
            { "Vigor", Vigor.CurrentValue }
        };
        return statsData;
    }

    /// <summary>
    /// Восстанавливает состояние характеристик из "фотографии".
    /// </summary>
    public void RestoreState(object state)
    {
        // Newtonsoft.Json по умолчанию преобразует словари в JObject,
        // поэтому нам нужно сначала преобразовать их обратно.
        var statsData = ((JObject)state).ToObject<Dictionary<string, float>>();

        // Восстанавливаем каждую характеристику, если она есть в сохранении
        if (statsData.TryGetValue("Health", out float health)) Health.SetCurrentValue(health);
        if (statsData.TryGetValue("Hunger", out float hunger)) Hunger.SetCurrentValue(hunger);
        if (statsData.TryGetValue("Thirst", out float thirst)) Thirst.SetCurrentValue(thirst);
        if (statsData.TryGetValue("Sanity", out float sanity)) Sanity.SetCurrentValue(sanity);
        if (statsData.TryGetValue("Vigor", out float vigor)) Vigor.SetCurrentValue(vigor);
    }
}
