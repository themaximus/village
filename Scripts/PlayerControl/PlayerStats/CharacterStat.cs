using UnityEngine;
using System;

[System.Serializable]
public class CharacterStat
{
    public event Action<float, float> OnValueChanged;

    [SerializeField] private float maxValue;
    private float currentValue;

    public float CurrentValue
    {
        get { return currentValue; }
        // Сеттер остается приватным для обычной игровой логики
        private set
        {
            currentValue = Mathf.Clamp(value, 0, maxValue);
            OnValueChanged?.Invoke(currentValue, maxValue);
        }
    }

    public float MaxValue
    {
        get { return maxValue; }
    }

    public void Initialize(float max)
    {
        maxValue = max;
        CurrentValue = maxValue;
    }

    public void Add(float amount)
    {
        CurrentValue += amount;
    }

    public void Remove(float amount)
    {
        CurrentValue -= amount;
    }

    // --- НОВЫЙ МЕТОД ДЛЯ СИСТЕМЫ СОХРАНЕНИЙ ---
    /// <summary>
    /// Устанавливает текущее значение напрямую.
    /// Используется системой сохранений для восстановления состояния.
    /// </summary>
    public void SetCurrentValue(float value)
    {
        CurrentValue = value;
    }
}
