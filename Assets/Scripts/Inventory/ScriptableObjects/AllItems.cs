using UnityEngine;
using System.Linq;
using UnityEditor;

public class AllItems : ScriptableObject    // конструктор находится в InventoryEditor
{                                       
    public Item[] items;                            // Все возможные в игре предметы инвентаря должны быть здесь

    private static AllItems instance;               // ссылка на себя, сигнализирующая, создан ли (единственный - "singleton") инстанс этого класса или нет

    private const string loadPath = "AllItems";     // The path within the Resources folder
 
    public static AllItems Instance                 // The public accessor for the singleton instance.
    {
        get
        {
            // If the instance is currently null, try to find an AllItems instance already in memory.
            if (!instance)
            {
                if (instance = FindObjectOfType<AllItems>())          // найти класс AllItems в памяти (и присвоить ссыль свойству instance)
                    Debug.Log("AllItems найден в памяти");
            }
            // If the instance is still null, try to load it from the Resources folder.
            if (!instance)
            {
                if(instance = Resources.Load<AllItems>(loadPath))    // подгрузить класс AllItems из asset-а (путь указывать относительно папки Resources)
                    Debug.Log("AllItems подгружен из ресурсов");
            }
            // If the instance is still null, debug it.
            if (!instance)
                Debug.LogError("AllItems has not been created yet.  Go to Assets > Create > AllItems.");

            instance.items = Resources.LoadAll("Items", typeof(Item)).Cast<Item>().ToArray(); // загрузить всё из ресурсов и отобрать объекты класса Item
            return instance;
        }
        set { instance = value; }                                 
    }
    

    // Без #if UNITY_EDITOR в рантайме на андроиде вывалит кучу ошибок про отсутствие MenuItem и AssetDatabase
    // подгрузил в папку Resources айтемов, вызвал повторно конструктор - и готово
#if UNITY_EDITOR
    private const string creationPath = "Assets/Resources/AllItems.asset";  
    [MenuItem("Assets/Create/_EF/AllItems")]                                    
    public static void CreateAllItemsAsset()
    {
        // Удалим старый (а потом создадим новый)
        /*if (AllItems.Instance)*/ AssetDatabase.DeleteAsset(creationPath);

        // Create an instance of the AllItems object and make an asset for it.
        AllItems instance = CreateInstance<AllItems>();
        AssetDatabase.CreateAsset(instance, creationPath);

        // Set this as the singleton instance.
        AllItems.Instance = instance;

        // Create an array of all existing Items.
        AllItems.Instance.items = Resources.LoadAll("Items", typeof(Item)).Cast<Item>().ToArray(); // Загрузить всё из ресурсов и отобрать объекты класса Item
    }
#endif
}


