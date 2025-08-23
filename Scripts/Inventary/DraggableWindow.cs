using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Позволяет перетаскивать UI элемент (окно) по экрану.
/// Вешается на тот RectTransform, который должен быть перетаскиваемым.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class DraggableWindow : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    /// <summary>
    /// Этот метод вызывается, когда мы "хватаем" окно мышкой.
    /// Он перемещает окно на передний план.
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        rectTransform.SetAsLastSibling();
    }

    /// <summary>
    /// Этот метод вызывается, когда мы двигаем мышь с зажатой кнопкой.
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        // eventData.delta содержит изменение позиции мыши с прошлого кадра.
        // Мы добавляем это изменение к текущей позиции окна.
        // Деление на canvas.scaleFactor необходимо для корректной работы
        // при разных разрешениях экрана.
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }
}
