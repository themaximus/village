using System.Collections.Generic; // Ќеобходимо дл€ использовани€ Dictionary

/// <summary>
/// Ётот атрибут позвол€ет Unity видеть и обрабатывать этот класс,
/// даже если он не €вл€етс€ компонентом (MonoBehaviour).
/// </summary>
[System.Serializable]
public class PlayerSaveData
{
    // ѕоле дл€ хранени€ позиции игрока.
    // ћы используем массив float[3], так как Vector3 не всегда хорошо сериализуетс€.
    public float[] position;

    // —ловарь дл€ хранени€ текущих значений всех характеристик.
    //  люч - это название характеристики (например, "Health"), значение - ее текущее количество.
    public Dictionary<string, float> stats;

    /// <summary>
    ///  онструктор дл€ удобного создани€ объекта с данными.
    /// </summary>
    public PlayerSaveData()
    {
        position = new float[3];
        stats = new Dictionary<string, float>();
    }
}
