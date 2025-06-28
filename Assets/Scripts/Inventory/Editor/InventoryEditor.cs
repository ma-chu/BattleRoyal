using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Inventory))]
public class InventoryEditor : Editor       
{
    private bool[] showItemSlots = new bool[Inventory.NumItemSlots];    // показывать ли слоты Item-ов расширенно
    
    private SerializedProperty _itemImagesProperty;
    private SerializedProperty _itemsProperty;
    
    private SerializedProperty _itemDescriptionNameProperty;
    private SerializedProperty _itemDescriptionImageProperty;
    private SerializedProperty _itemDescriptionTextProperty;
    private SerializedProperty _itemDescriptionCanvasProperty;
    
    private SerializedProperty _seriesStarProperty;

    // Соглашение об именах подобных констант: Inventory - класс , ItemImages - свойство, на кот. ссылаемся, Name - тип свойства = строка
    private const string InventoryPropItemImagesName = "itemImages";
    private const string InventoryPropItemsName = "items";
    private const string InventoryPropItemDescriptionNameName = "itemDescriptionName";
    private const string InventoryPropItemDescriptionImageName = "itemDescriptionImage";
    private const string InventoryPropItemDescriptionTextName = "itemDescriptionText";
    private const string InventoryPropItemDescriptionCanvasName = "itemDescriptionCanvas";
    private const string InventoryPropSeriesStarName = "seriesStar";
    
    private void OnEnable()
    {
        _itemImagesProperty = serializedObject.FindProperty (InventoryPropItemImagesName);   // serializedObject указывает на класс Inventory
        _itemsProperty = serializedObject.FindProperty (InventoryPropItemsName);
        _seriesStarProperty = serializedObject.FindProperty(InventoryPropSeriesStarName);   
        _itemDescriptionNameProperty = serializedObject.FindProperty(InventoryPropItemDescriptionNameName);
        _itemDescriptionImageProperty = serializedObject.FindProperty(InventoryPropItemDescriptionImageName);
        _itemDescriptionTextProperty = serializedObject.FindProperty(InventoryPropItemDescriptionTextName);
        _itemDescriptionCanvasProperty = serializedObject.FindProperty(InventoryPropItemDescriptionCanvasName);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update(); // актуализируем информацию в сериализированном объекте - почти всегда хорошо делать в начале

        for (var i = 0; i < Inventory.NumItemSlots; i++)
        {
            ItemSlotGUI(i);
        }

        EditorGUILayout.PropertyField(_itemDescriptionNameProperty);
        EditorGUILayout.PropertyField(_itemDescriptionImageProperty); 
        EditorGUILayout.PropertyField(_itemDescriptionTextProperty); 
        EditorGUILayout.PropertyField(_itemDescriptionCanvasProperty); 
        EditorGUILayout.PropertyField(_seriesStarProperty);

        serializedObject.ApplyModifiedProperties(); // записать изменения в сериализованном объекте в исходный объект (runtime) - почти всегда хорошо делать в конце
    }


    private void ItemSlotGUI (int index)
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);    // упорядочивать объекты вертикально (и в коробочках): от сих
        EditorGUI.indentLevel++;                        // отступ отсюда

        // Foldout - рисуем выпадающий список (раскрытый/закрытый в зависимости
        // от первого параметра showItemSlots[index] с названием "Item slot 0..3").
        // Результат присваиваем опять переменной showItemSlots[index] - "скрыть/раскрыть", которая изменяется при клике на нее
        showItemSlots[index] = EditorGUILayout.Foldout(showItemSlots[index], "Item slot " + index);

        if (showItemSlots[index])                       // и если список раскрыт
        {
            EditorGUILayout.PropertyField(_itemImagesProperty.GetArrayElementAtIndex(index)); // показать свойство (но не весь массив, а один элемент)
            EditorGUILayout.PropertyField(_itemsProperty.GetArrayElementAtIndex(index));      // и еще одно
        }

        EditorGUI.indentLevel--;                        // и досюда 
        EditorGUILayout.EndVertical();                  // и до сих
    }
}
