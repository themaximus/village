using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public WeaponData currentWeapon;

    public void EquipWeapon(WeaponData newWeapon)
    {
        currentWeapon = newWeapon;
        Debug.Log("Теперь у игрока в руках " + newWeapon.itemName);
        // можно тут менять спрайт оружия, скорость атаки, урон и т.п.
    }
}
