using UnityEngine;

// Этот атрибут добавит опцию для создания нового "Действия" в меню Assets -> Create
[CreateAssetMenu(fileName = "New Restore Stat Action", menuName = "Item Actions/Restore Stat")]
public class RestoreStatAction : ItemAction
{
    // Перечисление для выбора, какую именно характеристику мы хотим восстановить.
    // Это создаст удобный выпадающий список в инспекторе.
    public enum StatToRestore { Health, Hunger, Thirst, Sanity, Vigor }

    [Header("Action Settings")]
    public StatToRestore statToRestore;
    public float amount = 10f; // Количество, на которое нужно восстановить

    /// <summary>
    /// Выполняет логику восстановления характеристики.
    /// </summary>
    public override void Execute(GameObject performer)
    {
        // Пытаемся найти компонент StatController на объекте, который использовал предмет
        StatController statController = performer.GetComponent<StatController>();
        if (statController == null)
        {
            Debug.LogWarning("StatController not found on " + performer.name);
            return;
        }

        // Используем конструкцию switch для определения, какую характеристику нужно пополнить
        switch (statToRestore)
        {
            case StatToRestore.Health:
                statController.Health.Add(amount);
                Debug.Log("Restored " + amount + " Health.");
                break;
            case StatToRestore.Hunger:
                statController.Hunger.Add(amount);
                Debug.Log("Restored " + amount + " Hunger.");
                break;
            case StatToRestore.Thirst:
                statController.Thirst.Add(amount);
                Debug.Log("Restored " + amount + " Thirst.");
                break;
            case StatToRestore.Sanity:
                statController.Sanity.Add(amount);
                Debug.Log("Restored " + amount + " Sanity.");
                break;
            case StatToRestore.Vigor:
                statController.Vigor.Add(amount);
                Debug.Log("Restored " + amount + " Vigor.");
                break;
        }
    }
}
