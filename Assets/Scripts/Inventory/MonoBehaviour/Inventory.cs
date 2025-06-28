using EF.Localization;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public const int NumItemSlots = 3;
    
    [SerializeField] private Image[] itemImages = new Image[NumItemSlots];    
    [SerializeField] private Item[] items = new Item[NumItemSlots];           
    [Header("Description canvas")] 
    [SerializeField] private Image itemDescriptionImage;
    [SerializeField] private Text itemDescriptionName;
    [SerializeField] private Text itemDescriptionText;
    [SerializeField] private Canvas itemDescriptionCanvas;
    
    [SerializeField] private Sprite seriesStar;

    public Item[] Items => items;

    public int AddItem(Item itemToAdd)
    {
        for (var i = 0; i < items.Length; i++)
        {
            if (items[i] == itemToAdd) 
                return -2;                                  // такой предмет уже есть - не добавляем
            
            if (items[i] == null)                          
            {
                items[i] = itemToAdd;
                itemImages[i].sprite = itemToAdd.Sprite;
                itemImages[i].enabled = true;               // показываем картинку, false - чтобы при пустом слоте было пусто, а не белый фон
                return i;
            }
        }
        
        return -1;                                          // не удалось добавить по причине отсутствия свободных слотов
    }
    
    public void RemoveItem (Item itemToRemove)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == itemToRemove)
            {
                items[i] = null;                            
                itemImages[i].sprite = null;                
                itemImages[i].enabled = false;
                return;
            }
        }
    }

    [UsedImplicitly]
    public void ShowItemDescription (int index)
    {
        if (items[index] == null)
            return;
        
        if (index == -1) 
            return;                 // защита от показывания свойств недобавленного (ввиду избытка) объекта - костыль
        
        itemDescriptionName.text = items[index].Name.Localize();
        itemDescriptionText.text = items[index].Description.Localize();
        itemDescriptionImage.sprite = items[index].Sprite;
        itemDescriptionCanvas.enabled = true; // так производительнее чем itemDescriptionCanvas.gameObject.SetActive(true), не будет перебатчен родительский холст
    }
    
    public void CloseItemDescription()
    {
        itemDescriptionCanvas.enabled = false;
    }
    
    //ToDo: Убрать звезды серий из этого скрипта, создать менеджер описания предметов?
    [UsedImplicitly]
    public void ShowSeriesDescription (int index)
    {
        switch (index)
        {
            case 1:
                itemDescriptionText.text = "strong_strikes_series_desc".Localize();
                itemDescriptionName.text = "strong_strikes_series".Localize();
                break;
            case 2:
                itemDescriptionText.text = "series_of_blocks_desc".Localize();
                itemDescriptionName.text = "series_of_blocks".Localize();
                break;
            case 3:
                itemDescriptionText.text = "series_of_strikes_desc".Localize();
                itemDescriptionName.text = "series_of_strikes".Localize();
                break;
        }
        itemDescriptionImage.sprite = seriesStar;
        itemDescriptionCanvas.enabled = true;
    }
}
