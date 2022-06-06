// Напишем собственный редактор для удобства показа Inventory в редакторе юнити
using UnityEngine;
using UnityEditor;                          // для работы с редактором юнити (классами, его описывающими)
using System.Linq;                          // для метода Cast<Item>()        

[CustomEditor(typeof(Inventory))]           // целевой класс для редактора - Inventory
public class InventoryEditor : Editor       
{
    private bool[] showItemSlots = new bool[Inventory.numItemSlots];    // показывать ли слоты Item-ов расширенно?// Whether the GUI for each Item slot is expanded.
    private SerializedProperty itemImagesProperty;                      // свойство, которое будет описывать массив компонентов-Image для показа пунктов инвентаря (по сути, ссылка, которая отображается в редакторе, как ей скажут) // Represents the array of Image components to display the Items.
    private SerializedProperty itemsProperty;                           // свойство, которое будет описывать сам массив пунктов инвентаря // Represents the array of Items.
    // Информация о предмете при клике на него
    private SerializedProperty itemNameProperty;                    
    private SerializedProperty itemImageProperty;                   
    private SerializedProperty itemDescriptionProperty;             
    private SerializedProperty itemDescriptionObject;
    
    // Соглашение об именах подобных констант: inventory - это член такого класса, Prop - это свойство,  ItemImages - свойство, на кот. ссылаемся, Name - это строка
    private const string inventoryPropItemImagesName = "itemImages";    // The name of the field that is an array of Image components.
    private const string inventoryPropItemsName = "items";              // The name of the field that is an array of Items.


    private SerializedProperty seriesStarProperty;
    private const string inventoryPropseriesStarName = "seriesStar";
    private SerializedProperty itemDescriptionObjectProperty;
    private const string inventoryPropitemDescriptionObjectName = "itemDescriptionObject";
    private const string inventoryPropitemNamePropertyName = "itemImage";

    private void OnEnable ()
    {
        // Cache the SerializedProperties.
        itemImagesProperty = serializedObject.FindProperty (inventoryPropItemImagesName);   // serializedObject указывает на класс Inventory
        itemsProperty = serializedObject.FindProperty (inventoryPropItemsName);
        seriesStarProperty = serializedObject.FindProperty(inventoryPropseriesStarName);   
        itemDescriptionObjectProperty = serializedObject.FindProperty(inventoryPropitemDescriptionObjectName);
        itemNameProperty = serializedObject.FindProperty(inventoryPropitemNamePropertyName);
    }

    // в редакторе по-умолчанию этот метод вызывается по умалчанию, а в кастомном нет /вызывается каждый фрейм/
    public override void OnInspectorGUI ()  // переопределим стандартное отношение редактора юнити к этому классу (InventoryEditor) /так как в классе Editor этод метод виртуальный/
    {
        // Pull all the information from the target into the serializedObject.
        serializedObject.Update ();         // актуализируем информацию в сериализированном объекте - почти всегда хорошо делать в начале

        // Display GUI for each Item slot.
        for (int i = 0; i < Inventory.numItemSlots; i++)
        {
            ItemSlotGUI (i);
        }

        EditorGUILayout.PropertyField(itemDescriptionObjectProperty); 
        EditorGUILayout.PropertyField(itemNameProperty);
        EditorGUILayout.PropertyField(seriesStarProperty);     

        // Push all the information from the serializedObject back into the target.
        serializedObject.ApplyModifiedProperties ();    // записать изменения в сериализованном объекте в исходный объект (runtime) - почти всегда хорошо делать в конце
    }


    private void ItemSlotGUI (int index)
    {
        EditorGUILayout.BeginVertical (GUI.skin.box);   // упорядочивать объекты вертикально (и в коробочках): от сих
        EditorGUI.indentLevel++;                        // отступ отсюда

        // Foldout - рисуем выпадающий список (раскрытый/закрытый в зависимости от первого параметра showItemSlots[index] с названиями "Item slot 0..3"). Результат присваиваем опять переменной showItemSlots[index] - "скрыть/раскрыть", которая изменяется при клике на нее
        // Display a foldout to determine whether the GUI should be shown or not.
        showItemSlots[index] = EditorGUILayout.Foldout (showItemSlots[index], "Item slot " + index);

        // If the foldout is open then display default GUI for the specific elements in each array.
        if (showItemSlots[index])                       // и если список раскрыт
        {
            EditorGUILayout.PropertyField (itemImagesProperty.GetArrayElementAtIndex (index));  // показать свойство (но не весь массив, а один элемент)
            EditorGUILayout.PropertyField (itemsProperty.GetArrayElementAtIndex (index));       // и еще одно
        }

        EditorGUI.indentLevel--;                        // и досюда 
        EditorGUILayout.EndVertical ();                 // и до сих
    }

    // Конструктор AllItems ставим именно здесь, в классе-редакторе, который запускается только в Unity-editor-е:
    // в рантайме на андроиде вывалит кучу ошибок про отсутствие MenuItem и AssetDatabase
    // подгрузил в папку Resources айтемов, вызвал повторно конструктор - и готово
    private const string creationPath = "Assets/Resources/AllItems.asset";  // The path that the AllItems asset is created at.
    [MenuItem("Assets/Create/_EF/AllItems")]                                    // Call this function when the menu item is selected.

    public static void CreateAllItemsAsset()
    {
        // Удалим старый (а потом создадим новый)
        if (AllItems.Instance) AssetDatabase.DeleteAsset(creationPath);

        // Create an instance of the AllItems object and make an asset for it.
        AllItems instance_ = CreateInstance<AllItems>();
        AssetDatabase.CreateAsset(instance_, creationPath);

        // Set this as the singleton instance.
        AllItems.Instance = instance_;

        // Create an array of all existing Items.
        AllItems.Instance.items = Resources.LoadAll("Items", typeof(Item)).Cast<Item>().ToArray(); // Загрузить всё из ресурсов и отобрать объекты класса Item
    }
}
