// ������� ����������� �������� ��� �������� ������ Inventory � ��������� �����
using UnityEngine;
using UnityEditor;                          // ��� ������ � ���������� ����� (��������, ��� ������������)
using System.Linq;                          // ��� ������ Cast<Item>()        

// OFFTOP
// Type - �������� ����� ��� �������������� ������������ ��������� (������� ����������� ����� �� ����� ����������) � �������� ������ ������� � ����������
// typeof(myClass) �������� ���������� ���� Type �� ������ �������� ������ (�������). ��������, ���������� �� ����������? myClass.GetType �������� ���������� ���� Type, �� ���� ������������ ���� �� ����������?
// typeof(myClass) ����������, ��������� �� ��� myClass � ����� ������, � ���������� ������ �� ��������������� runtime-������.

[CustomEditor(typeof(Inventory))]           // ������� ����� ��� ��������� - Inventory. ���� �������� CustomEditor �� �������, ������ �� ���������
public class InventoryEditor : Editor       // ��������� �� ������ Editor, � �� MonoBehaviour 
{
    private bool[] showItemSlots = new bool[Inventory.numItemSlots];    // ���������� �� ����� Item-�� ����������?// Whether the GUI for each Item slot is expanded.
    private SerializedProperty itemImagesProperty;                      // ��������, ������� ����� ��������� ������ �����������-Image ��� ������ ������� ��������� (�� ����, ������, ������� ������������ � ���������, ��� �� ������) // Represents the array of Image components to display the Items.
    private SerializedProperty itemsProperty;                           // ��������, ������� ����� ��������� ��� ������ ������� ��������� // Represents the array of Items.
    // ���������� � �������� ��� ����� �� ����
    private SerializedProperty itemNameProperty;                    // ����� ��� �������� � ��������
    private SerializedProperty itemImageProperty;                   // �������� ��� ������ ��������
    private SerializedProperty itemDescriptionProperty;             // ����� ��� ������ ��������
    private SerializedProperty itemDescriptionObject;               // ������ ��� ������ ��������

    // ���������� �� ������ �������� ��������: inventory - ��� ���� ������ ������, Prop - ��� ��������,  ItemImages - ��������, �� ���. ���������, Name - ��� ������
    private const string inventoryPropItemImagesName = "itemImages";    // The name of the field that is an array of Image components.
    private const string inventoryPropItemsName = "items";              // The name of the field that is an array of Items.

    private void OnEnable ()
    {
        // Cache the SerializedProperties.
        itemImagesProperty = serializedObject.FindProperty (inventoryPropItemImagesName);   // serializedObject ��������� �� ����� Inventory
        itemsProperty = serializedObject.FindProperty (inventoryPropItemsName);
    }

    // � ��������� ��-��������� ���� ����� ���������� �� ���������, � � ��������� ���. /���������� ������ �����/
    public override void OnInspectorGUI ()  // ������������� ����������� ��������� ��������� ����� � ����� ������ (InventoryEditor) /��� ��� � ������ Editor ���� ����� �����������/
    {
        // Pull all the information from the target into the serializedObject.
        serializedObject.Update ();         // ������������� ���������� � ����������������� ������� - ����� ������ ������ ������ � ������

        // Display GUI for each Item slot.
        for (int i = 0; i < Inventory.numItemSlots; i++)
        {
            ItemSlotGUI (i);
        }

        // Push all the information from the serializedObject back into the target.
        serializedObject.ApplyModifiedProperties ();    // �������� ��������� � ��������������� ������� � �������� ������ (runtime) - ����� ������ ������ ������ � �����
    }


    private void ItemSlotGUI (int index)
    {
        EditorGUILayout.BeginVertical (GUI.skin.box);   // ������������� ������� ����������� (� � ����������): �� ���
        EditorGUI.indentLevel++;                        // ������ ������

        // Foldout - ������ ���������� ������ (���������/�������� � ����������� �� ������� ��������� showItemSlots[index] � ���������� "Item slot 0..3"). ��������� ����������� ����� ���������� showItemSlots[index] - "������/��������", ������� ���������� ��� ����� �� ���
        // Display a foldout to determine whether the GUI should be shown or not.
        showItemSlots[index] = EditorGUILayout.Foldout (showItemSlots[index], "Item slot " + index);

        // If the foldout is open then display default GUI for the specific elements in each array.
        if (showItemSlots[index])                       // � ���� ������ �������
        {
            EditorGUILayout.PropertyField (itemImagesProperty.GetArrayElementAtIndex (index));  // �������� �������� (�� �� ���� ������, � ���� �������)
            EditorGUILayout.PropertyField (itemsProperty.GetArrayElementAtIndex (index));       // � ��� ����
        }

        EditorGUI.indentLevel--;                        // � ������ 
        EditorGUILayout.EndVertical ();                 // � �� ���
    }

    // ��� �-�� ������ ������ �����, � ������-���������, ������� ����������� ������ � Unity-editor-�:
    // � �������� �� �������� ������� ���� ������ ��� ���������� MenuItem � AssetDatabase
    private const string creationPath = "Assets/Resources/AllItems.asset";  // The path that the AllItems asset is created at.
    [MenuItem("Assets/Create/AllItems")]    // Call this function when the menu item is selected.
    public static void CreateAllItemsAsset()
    {
        // If there's already an AllItems asset, do nothing.
        // if (AllItems.Instance) return;
        // ���, ����� ������ (� ����� �������� �����)
        if (AllItems.Instance) AssetDatabase.DeleteAsset(creationPath);

        // Create an instance of the AllItems object and make an asset for it.
        AllItems instance = CreateInstance<AllItems>();
        AssetDatabase.CreateAsset(instance, creationPath);

        // Set this as the singleton instance.
        AllItems.Instance = instance;

        // Create an array of all existing Items.
        instance.items = Resources.LoadAll("Items", typeof(Item)).Cast<Item>().ToArray(); // ��������� �� � �������� ������� ������ Item
        // instance1.items = Resources.FindObjectsOfTypeAll (typeof(Item)) as Item[];  - �������� ����������� - �� ������� Items � ����� Assets/Resources/Items, ���� ��� ����� �� ������ � Project-� � �� ����������� ��� ������    
    }
}
