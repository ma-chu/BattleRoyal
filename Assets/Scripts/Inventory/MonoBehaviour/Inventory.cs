using EF.Localization;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public Image[] itemImages = new Image[numItemSlots];    
    public Item[] items = new Item[numItemSlots];           

    public const int numItemSlots = 3;                       // количество слотов

    // Где выводится информация о предмете при клике на него
    [SerializeField] private GameObject itemDescriptionObject;// объект-родитель (холст), содержащий все эти поля (для вкл/выкл информации)
    [SerializeField] private Image itemImage;                 // картинка для вывода изо предмета инвентаря
    private Text itemName;                                   // текст для вывода имени предмета инвентаря
    private Text itemDescription;                            // текст для вывода описания предмета инвентаря
    private Canvas itemCanvas;
    
    [SerializeField] private Sprite seriesStar;               // спрайт для серий
    
    public void Awake()
    {
        //itemImage = itemDescriptionObject.GetComponentInChildren<Image>();
        itemName = itemDescriptionObject.GetComponentsInChildren<Text>()[1];            // почему этот в массиве первый, а следующий нулевой? Потому что тот выше в иерархии
        itemDescription = itemDescriptionObject.GetComponentsInChildren<Text>()[0];
        itemCanvas = itemDescriptionObject.GetComponent<Canvas>();
    }
    
    // Уйдет в сервер? Нет, там своя аналогичная ф-ия
    public int AddItem(Item itemToAdd)                     // поместить пункт в слот инвентория
    {
        // Go through all the item slots...
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == itemToAdd) return -2;           // такой предмет уже есть - не добавляем
            if (items[i] == null)                           // ... if the item slot is empty...
            {
                // ... set it to the picked up item and set the image component to display the item's sprite.
                items[i] = itemToAdd;                       // помещаем сам item в массив item-ов
                itemImages[i].sprite = itemToAdd.Sprite;    // картиночку - в массив картинок
                itemImages[i].enabled = true;               // показываем картинку, false - чтобы при пустом слоте было пусто, а не белый фон
                return i;
            }
        }
        return -1;  // не удалось добавить по причине отсутствия свободных слотов
    }

    
    public void RemoveItem (Item itemToRemove)              // удалить item из слота инвентория
    {
        // Go through all the item slots...
        for (int i = 0; i < items.Length; i++)
        {
            // ... if the item slot has the item to be removed...
            if (items[i] == itemToRemove)
            {
                // ... set the item slot to null and set the image component to display nothing.
                items[i] = null;                            
                itemImages[i].sprite = null;                
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
        itemName.text = items[index]./*name*/Name.Localize();
        itemDescription.text = items[index].Description.Localize();
        itemImage.sprite = items[index].Sprite;
        //itemDescriptionObject.SetActive(true); - так будет перебатчен родительский холст
        itemCanvas.enabled = true; // так производительнее
    }

    // закрыть описание объекта в слоте инвентаря
    public void CloseItemDescription()
    {
        //itemDescriptionObject.SetActive(false);
        itemCanvas.enabled = false;
    }
    
    public void ShowSeriesDescription (int index)
    {
        switch (index)
        {
            case 1:
                itemDescription.text = "strong_strikes_series_desc".Localize();
                itemName.text = "strong_strikes_series".Localize();
                break;
            case 2:
                itemDescription.text = "series_of_blocks_desc".Localize();
                itemName.text = "series_of_blocks".Localize();
                break;
            case 3:
                itemDescription.text = "series_of_strikes_desc".Localize();
                itemName.text = "series_of_strikes".Localize();
                break;
        }
        itemImage.sprite = seriesStar;
        itemCanvas.enabled = true; // так производительнее
    }
}
