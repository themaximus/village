using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public WeaponData currentWeapon;

    public void EquipWeapon(WeaponData newWeapon)
    {
        currentWeapon = newWeapon;
        Debug.Log("������ � ������ � ����� " + newWeapon.itemName);
        // ����� ��� ������ ������ ������, �������� �����, ���� � �.�.
    }
}
