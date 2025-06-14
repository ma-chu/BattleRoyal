using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Inventory))]
public class InventoryEditor : Editor       
{
    private bool[] showItemSlots = new bool[Inventory.NumItemSlots];     // показывать ли слоты Item-ов расширенно?
    
    private SerializedProperty _itemImagesProperty;                      // свойство, которое будет описывать массив компонентов-Image для показа пунктов инвентаря (по сути, ссылка, которая отображается в редакторе, как ей скажут)
    private SerializedProperty _itemsProperty;                           // свойство, которое будет описывать сам массив пунктов инвентаря
    // Информация о предмете при клике на него
    private SerializedProperty _itemNameProperty;                    
    private SerializedProperty _seriesStarProperty;
    private SerializedProperty _itemDescriptionObjectProperty;

    // Соглашение об именах подобных констант: Inventory - класс , ItemImages - свойство, на кот. ссылаемся, Name - тип свойства = строка
    private const string InventoryPropItemImagesName = "itemImages";
    private const string InventoryPropItemsName = "items";
    private const string InventoryPropItemNamePropertyName = "itemImage";
    private const string InventoryPropSeriesStarName = "seriesStar";
    private const string InventoryPropItemDescriptionObjectName = "itemDescriptionObject";
    
    // private SerializedProperty _itemImageProperty;                   
    // private SerializedProperty _itemDescriptionProperty;             
    // private SerializedProperty _itemDescriptionObject;
    
    private void OnEnable()
    {
        // Cache the SerializedProperties.
        _itemImagesProperty = serializedObject.FindProperty (InventoryPropItemImagesName);   // serializedObject указывает на класс Inventory
        _itemsProperty = serializedObject.FindProperty (InventoryPropItemsName);
        _seriesStarProperty = serializedObject.FindProperty(InventoryPropSeriesStarName);   
        _itemDescriptionObjectProperty = serializedObject.FindProperty(InventoryPropItemDescriptionObjectName);
        _itemNameProperty = serializedObject.FindProperty(InventoryPropItemNamePropertyName);
    }

    public override void OnInspectorGUI()
    {
        // Pull all the information from the target into the serializedObject.
        serializedObject.Update ();         // актуализируем информацию в сериализированном объекте - почти всегда хорошо делать в начале

        // Display GUI for each Item slot.
        for (int i = 0; i < Inventory.NumItemSlots; i++)
        {
            ItemSlotGUI (i);
        }

        EditorGUILayout.PropertyField(_itemDescriptionObjectProperty); 
        EditorGUILayout.PropertyField(_itemNameProperty);
        EditorGUILayout.PropertyField(_seriesStarProperty);     

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
            EditorGUILayout.PropertyField (_itemImagesProperty.GetArrayElementAtIndex (index));  // показать свойство (но не весь массив, а один элемент)
            EditorGUILayout.PropertyField (_itemsProperty.GetArrayElementAtIndex (index));       // и еще одно
        }

        EditorGUI.indentLevel--;                        // и досюда 
        EditorGUILayout.EndVertical ();                 // и до сих
    }
}
