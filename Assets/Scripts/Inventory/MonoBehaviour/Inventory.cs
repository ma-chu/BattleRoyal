using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    //static Item[] AllItems = new Item[];                    // массив всех возможных предметов в игре
    // Зачем вся эта басня с разными массивами? Почему не сделать один массив Item[]? Для производительности? Да нет, похоже, поле Sprite и компонент Image - разные вещи
    public Image[] itemImages = new Image[numItemSlots];    // The Image components that display the Items.
    public Item[] items = new Item[numItemSlots];           // The Items that are carried by the player. Cам тип Item (наследует от ScriptableObject) описан в одноименном файле

    public const int numItemSlots = 3;                      // количество слотов в Inventory // The number of items that can be carried.  This is a constant so that the number of Images and Items are always the same.

    // Где выводится информация о предмете при клике на него
    private Image itemImage;                                 // картинка для вывода изо предмета инвентаря
    private Text itemName;                                   // текст для вывода имени предмета инвентаря
    private Text itemDescription;                            // текст для вывода описания предмета инвентаря
    private GameObject itemDescriptionObject;                // объект-родитель, содержащий все эти поля (для вкл/выкл информации)

    public void Awake()
    {
        itemDescriptionObject = GameObject.FindGameObjectWithTag("ItemDescription");
        itemImage = itemDescriptionObject.GetComponentInChildren<Image>();
        itemName = itemDescriptionObject.GetComponentsInChildren<Text>()[1];    // почему этот в массиве первый, а тот нулевой? х.з.
        itemDescription = itemDescriptionObject.GetComponentsInChildren<Text>()[0];
    }

        // This function is called by the ?GameManager? in order to add an item to the inventory.
    public int AddItem(Item itemToAdd)                     // поместить пункт в слот инвентория
    {
        // Go through all the item slots...
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == itemToAdd) return -2;           // такой предмет уже есть - не добавляем
            if (items[i] == null)                           // в слоте ничего нет   // ... if the item slot is empty...
            {
                // ... set it to the picked up item and set the image component to display the item's sprite.
                items[i] = itemToAdd;                       // помещаемсам item в массив item-ов
                itemImages[i].sprite = itemToAdd.sprite;    // картиночку - в массив картинок
                itemImages[i].enabled = true;               // показываем картинку, false - чтобы при пустом слоте было пусто, а не белый фон
                return i;
            }
        }
        return -1;  // не удалось добавить по причине отсутствия свободных слотов
    }


    // This function is called by the ?GameManager? in order to remove an item from the inventory.
    public void RemoveItem (Item itemToRemove)              // удалить item из слота инвентория
    {
        // Go through all the item slots...
        for (int i = 0; i < items.Length; i++)
        {
            // ... if the item slot has the item to be removed...
            if (items[i] == itemToRemove)
            {
                // ... set the item slot to null and set the image component to display nothing.
                items[i] = null;                            // сам item долой из массива
                itemImages[i].sprite = null;                // картиночку - из массива картинок
                itemImages[i].enabled = false;              // чтобы при пустом слоте было пусто, а не белый фон
                return;
            }
        }
    }

    // для вывода описания объекта в слоте инвентаря
    public void ShowItemDescription (int index)
    {
        if (items[index] == null) return;
        if (index == -1) return;    // защита от показывания свойств недобавленного (ввиду избытка) объекта - костыль
        else
        {
            itemName.text = items[index].name;
            itemDescription.text = items[index].description;
            itemImage.sprite = items[index].sprite;
            itemDescriptionObject.SetActive(true);
        }
    }
    // закрыть описание объекта в слоте инвентаря
    public void CloseItemDescription()
    {
        itemDescriptionObject.SetActive(false);
    }
}
