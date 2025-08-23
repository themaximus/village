using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// ��������� ������������� UI ������� (����) �� ������.
/// �������� �� ��� RectTransform, ������� ������ ���� ���������������.
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
    /// ���� ����� ����������, ����� �� "�������" ���� ������.
    /// �� ���������� ���� �� �������� ����.
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        rectTransform.SetAsLastSibling();
    }

    /// <summary>
    /// ���� ����� ����������, ����� �� ������� ���� � ������� �������.
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        // eventData.delta �������� ��������� ������� ���� � �������� �����.
        // �� ��������� ��� ��������� � ������� ������� ����.
        // ������� �� canvas.scaleFactor ���������� ��� ���������� ������
        // ��� ������ ����������� ������.
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }
}
