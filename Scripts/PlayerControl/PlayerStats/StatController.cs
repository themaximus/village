using UnityEngine;
using System;
using System.Collections.Generic; // ���������� ��� Dictionary
using Newtonsoft.Json.Linq; // ���������� ��� JObject

// "����������� ��������" ISaveable
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

    // --- ���������� ���������� ISaveable ---

    /// <summary>
    /// "�������������" ������� ��������� ���� �������������.
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
    /// ��������������� ��������� ������������� �� "����������".
    /// </summary>
    public void RestoreState(object state)
    {
        // Newtonsoft.Json �� ��������� ����������� ������� � JObject,
        // ������� ��� ����� ������� ������������� �� �������.
        var statsData = ((JObject)state).ToObject<Dictionary<string, float>>();

        // ��������������� ������ ��������������, ���� ��� ���� � ����������
        if (statsData.TryGetValue("Health", out float health)) Health.SetCurrentValue(health);
        if (statsData.TryGetValue("Hunger", out float hunger)) Hunger.SetCurrentValue(hunger);
        if (statsData.TryGetValue("Thirst", out float thirst)) Thirst.SetCurrentValue(thirst);
        if (statsData.TryGetValue("Sanity", out float sanity)) Sanity.SetCurrentValue(sanity);
        if (statsData.TryGetValue("Vigor", out float vigor)) Vigor.SetCurrentValue(vigor);
    }
}
