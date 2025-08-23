using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragItemDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // ������ ��� ����� �� ����� �������������
    public UniversalSlotUI slot;
    private GameObject dragGhost;
    private ItemData draggedItem;
    private Inventory inventory;

    void Awake()
    {
        // �������� ������ �� ����, �� ������� ����� ���� ������
        slot = GetComponent<UniversalSlotUI>();
        if (slot != null)
        {
            inventory = slot.Inventory;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (slot.IsEmpty()) return;

        draggedItem = slot.Item;

        // ������� "��������" ��� UI Image, ����� �� �� ���������� ������� ����
        dragGhost = new GameObject("DragGhost");
        dragGhost.transform.SetParent(GetComponentInParent<Canvas>().transform, false);
        dragGhost.transform.SetAsLastSibling();

        var image = dragGhost.AddComponent<Image>();
        image.sprite = draggedItem.icon;
        image.color = new Color(1f, 1f, 1f, 0.8f);
        image.raycastTarget = false; // "�������" �� ����� ����������� �����

        dragGhost.GetComponent<RectTransform>().sizeDelta = GetComponent<RectTransform>().sizeDelta;

        slot.iconImage.color = new Color(1f, 1f, 1f, 0.5f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragGhost != null)
        {
            dragGhost.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        slot.iconImage.color = new Color(1f, 1f, 1f, 1f);
        if (dragGhost != null)
        {
            Destroy(dragGhost);
        }

        GameObject dropTarget = eventData.pointerEnter;

        // ���������, ��� �� ������� ������ �� ����� ������������� ����.
        // ���� ��, �� OnDrop ����� ����� ��� ����������, ��� ������.
        if (dropTarget != null && dropTarget.GetComponentInParent<UniversalSlotUI>() != null)
        {
            return;
        }

        // ���� ������� ������ ����, ����������� ��� � ���.
        if (inventory != null)
        {
            inventory.HandleItemDrop(slot, null);
        }
    }
}
