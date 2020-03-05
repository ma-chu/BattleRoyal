using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class AllItems : ScriptableObject
{
    public Item[] items;                // Все существующие в игре предметы инвентаря должны быть здесь

    private static AllItems instance;   // ссылка на себя, сигнализирующая, создан ли (единственный - "singleton") инстанс этого класса или нет

    private const string loadPath = "AllItems";                             // The path within the Resources folder that 

    // свойство,  по которому мы получаем доступ (не произвольный, а определённый м-ми get и set) к приавтному полю "private static AllItems instance": get - находим уже существующий инстанс этого типа или создаем этот инстанс (из памяти или подгружаем его из asset-ов)
    public static AllItems Instance                // The public accessor for the singleton instance.
    {
        get
        {
            // If the instance is currently null, try to find an AllItems instance already in memory.
            if (!instance)
            {
                instance = FindObjectOfType<AllItems>();          // найти класс AllItems в памяти (и присвоить ссыль свойству instance)
                //Debug.LogError("AllItems найден в памяти");
            }
            // If the instance is still null, try to load it from the Resources folder.
            if (!instance)
            {
                instance = Resources.Load<AllItems>(loadPath);    // подгрузить класс AllItems из asset-а (путь указывать относительно папки Resources)
                //Debug.LogError("AllItems подгружен из ресурсов");
            }
            // If the instance is still null, debug it.
            if (!instance) 
                Debug.LogError("AllItems has not been created yet.  Go to Assets > Create > AllItems.");
            return instance;
        }
        set { instance = value; }                                 // есть set, => свойству Instance можно присваивать (не read-only)
    }
}
