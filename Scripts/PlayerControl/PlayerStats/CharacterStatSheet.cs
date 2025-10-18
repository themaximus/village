using UnityEngine;

// Ётот атрибут добавл€ет опцию дл€ создани€ нового "шаблона" в меню Assets -> Create
[CreateAssetMenu(fileName = "New Stat Sheet", menuName = "Stats/Character Stat Sheet")]
public class CharacterStatSheet : ScriptableObject
{
    [Header("Base Stat Values")]
    public float maxHealth = 100f;
    public float maxHunger = 100f;
    public float maxThirst = 100f;
    public float maxSanity = 100f;
    public float maxVigor = 100f; // ћаксимальна€ бодрость

    [Header("Passive Stat Decay Rates")]
    [Tooltip("—колько единиц голода тер€етс€ в секунду.")]
    public float hungerDecayRate = 1f;
    [Tooltip("—колько единиц жажды тер€етс€ в секунду.")]
    public float thirstDecayRate = 2f;
    // ћожно добавить и другие пассивные изменени€, например, восстановление бодрости во сне.
}
