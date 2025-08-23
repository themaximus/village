using System.Collections.Generic;

/// <summary>
/// Главный класс-контейнер, который хранит все данные для одного сохранения.
/// </summary>
[System.Serializable]
public class SaveData
{
    // Имя сцены, в которой было сделано сохранение
    public string sceneName;

    // Словарь, хранящий состояние всех сохраняемых объектов на этой сцене
    public Dictionary<string, object> sceneObjectsState;

    public SaveData()
    {
        sceneObjectsState = new Dictionary<string, object>();
    }
}
