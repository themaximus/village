using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragItemDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // Меняем тип слота на новый универсальный
    public UniversalSlotUI slot;
    private GameObject dragGhost;
    private ItemData draggedItem;
    private Inventory inventory;

    void Awake()
    {
        // Получаем ссылку на слот, на котором висит этот скрипт
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

        // Создаем "призрака" как UI Image, чтобы он не блокировал события мыши
        dragGhost = new GameObject("DragGhost");
        dragGhost.transform.SetParent(GetComponentInParent<Canvas>().transform, false);
        dragGhost.transform.SetAsLastSibling();

        var image = dragGhost.AddComponent<Image>();
        image.sprite = draggedItem.icon;
        image.color = new Color(1f, 1f, 1f, 0.8f);
        image.raycastTarget = false; // "Призрак" не будет блокировать клики

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

        // Проверяем, был ли предмет брошен на ЛЮБОЙ универсальный слот.
        // Если да, то OnDrop этого слота сам разберется, что делать.
        if (dropTarget != null && dropTarget.GetComponentInParent<UniversalSlotUI>() != null)
        {
            return;
        }

        // Если предмет брошен мимо, выбрасываем его в мир.
        if (inventory != null)
        {
            inventory.HandleItemDrop(slot, null);
        }
    }
}
