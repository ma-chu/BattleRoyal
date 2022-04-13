using System;
using EF.Tools;
using UnityEngine;

[Serializable]
public struct WeaponChanges
{
    public Mesh shieldMesh;
    public Material shieldMat;
    public Mesh longMesh;
    public Material longMat;
    public Color color;
}


public class EnemyManager : HeroManager
{
    const float zeroZposition = 1.35f;      // позиция героя на ристалище  
    const float zeroYrotation = -180f;      // вращение героя на ристалище  
    const float stockXposition = -2.2f;     // начальное вращение героя 
    const float startRotation = 90f;        // начальная позиция героя (позиция склада)

    [SerializeField] private Item enemiesItem1;                     // что выдавать врагу на последний раунд
    [SerializeField] private WeaponChanges[] Weapons;               // изменения мешей и материалов оружия врага

    private bool _rotated;
    
    protected override void Awake()
    {
        // определимся со ссылками на слоты инвентория
        var eventTriggers = inventory.GetComponentsInChildren<UnityEngine.EventSystems.EventTrigger>();
        for (int i = 0; i < Inventory.numItemSlots; i++)
        {
            //itemSlots = GameObject.FindGameObjectsWithTag("itemSlot_enemy");
            itemSlots[i] = eventTriggers[i].gameObject;
        }

        base.Awake();                                              

        heroType = Heroes.Enemy;
    }

    protected override void OnEnable()                                 // when the enemy is back on again, следующий раунд)
    {
        // Установить начальное положение героя, задать исходное на ристалище 
        m_HeroAnimation.SetStartPositions(zeroZposition, zeroYrotation, stockXposition, startRotation);
  
        if (GameManager.gameType == GameType.Single) inventory.RemoveItem(enemiesItem1);    // убираем инвентарь врага с прошлого раунда (если был, то максимум один)
        
        // 1. Изменения в зависимости от количества выигранных игроком раундов: цвет и форма оружия врага, инвентарь врага
        if (player_countRoundsWon!=0) ChangeWeaponsView(player_countRoundsWon-1);
        if (player_countRoundsWon == 3 && GameManager.gameType == GameType.Single) inventory.AddItem(enemiesItem1);  // даем врагу кольцо
        base.OnEnable();                           

        //2. Усложнить игру базовым уроном в зависимости от кол-ва выигранных раундов
        if (GameManager.gameType == GameType.Single) m_Tweakers.AddLevelTweakers(HeroManager.player_countRoundsWon);
    }

    private void ChangeWeaponsView(int winsZeroBased)
    {
        // меш и материал щита и лонга
        shieldMeshFilter.mesh = Weapons[winsZeroBased].shieldMesh;                                         
        shieldMeshRenderer.material = Weapons[winsZeroBased].shieldMat;
        if (!_rotated && winsZeroBased == 1)
        {
            heroShield.transform.Rotate(Vector3.forward, 180);        // перевернуть щит один раз на 3 раунде для моделей 2 и 3
            _rotated = true;
        }
        twoHandedSwordMeshFilter.mesh = Weapons[winsZeroBased].longMesh;                                         
        twoHandedSwordMeshRenderer.material = Weapons[winsZeroBased].longMat;
        
        // цвет меча, щита и лонга
        //shieldMeshRenderer.material.SetColor("Color", Weapons[round].color);  // так было до URP
        shieldMeshRenderer.material.color = Weapons[winsZeroBased].color;
        swordMeshRenderer.material.color = Weapons[winsZeroBased].color;
        sword2MeshRenderer.material.color = Weapons[winsZeroBased].color;
        twoHandedSwordMeshRenderer.material.color = Weapons[winsZeroBased].color;
        /* foreach (MeshRenderer mR in /m_Enemy/this.GetComponentsInChildren<MeshRenderer>()) mR.material.SetColor("_Color", Color.red);
         * коротко, но не работает для не Enabled объектов*/
        // свечение мечей
        swordMeshRenderer.material.EnableKeyword("_EMISSION");
        swordMeshRenderer.material.SetColor("_EmissionColor", Weapons[winsZeroBased].color);
        sword2MeshRenderer.material.EnableKeyword("_EMISSION");
        sword2MeshRenderer.material.SetColor("_EmissionColor", Weapons[winsZeroBased].color);
        twoHandedSwordMeshRenderer.material.EnableKeyword("_EMISSION");
        twoHandedSwordMeshRenderer.material.SetColor("_EmissionColor", Weapons[winsZeroBased].color);
        twoHandedSwordMeshRenderer.material.SetTexture("_EmissionMap", twoHandedSwordMeshRenderer.material.mainTexture);
    }

}
