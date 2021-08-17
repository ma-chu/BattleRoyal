using EF.Sounds;
using UnityEngine;

public class EnemyManager : HeroManager
{
    const float zeroZposition = 1.35f;      // позиция героя на ристалище  
    const float zeroYrotation = -180f;      // вращение героя на ристалище  
    const float stockXposition = -2.2f;     // начальное вращение героя 
    const float startRotation = 90f;        // начальная позиция героя (позиция склада)

    [SerializeField]
    private Item enemiesItem1;                                      // что выдавать врагу на последний раунд
    // изменения форм и материалов оружия врага
    [SerializeField]
    private Mesh shieldMesh1;
    [SerializeField]
    private Material shieldMaterial1;
    [SerializeField]
    private Mesh shieldMesh2;
    [SerializeField]
    private Material shieldMaterial2;
    [SerializeField]
    private Mesh twoHandedSwordMesh2;
    [SerializeField]
    private Material twoHandedSwordMaterial2;
    [SerializeField]
    private Mesh shieldMesh3;
    [SerializeField]
    private Material shieldMaterial3;
    [SerializeField]
    private Mesh twoHandedSwordMesh3;
    [SerializeField]
    private Material twoHandedSwordMaterial3;

    protected override void Awake()
    {
        // определимся со ссылками на слоты инвентория
        for (int i = 0; i < Inventory.numItemSlots; i++)
        {
            itemSlots = GameObject.FindGameObjectsWithTag("itemSlot_enemy");
        }

        base.Awake();                                              

        heroType = Heroes.Enemy;
    }

    protected override void OnEnable()                                 // что мы делаем, когда герой снова жив (back on again, следующий раунд)
    {
        // Установить начальное положение героя, задать исходное на ристалище 
        m_HeroAnimation.SetStartPositions(zeroZposition, zeroYrotation, stockXposition, startRotation);
  
        if (GameManager.gameType == GameType.Single) inventory.RemoveItem(enemiesItem1);    // убираем инвентарь врага с прошлого раунда (если был, то максимум один)
        
        // 1. Изменения в зависимости от количества выигранных игроком раундов: цвет и форма оружия врага, инвентарь врага
        switch (HeroManager.player_countRoundsWon)                  
        {
            case 1:
                shieldMeshFilter.mesh = shieldMesh1;                            // меняем форму щита
                shieldMeshRenderer.material = shieldMaterial1;                  // материал щита
                shieldMeshRenderer.material.SetColor("_Color", Color.red);      // цвет щита и мечей
                swordMeshRenderer.material.SetColor("_Color", Color.red);
                sword2MeshRenderer.material.SetColor("_Color", Color.red);
                //twoHandedSwordMeshRenderer.material.SetColor("_Color", Color.red);
                /* foreach (MeshRenderer mR in /m_Enemy/this.GetComponentsInChildren<MeshRenderer>()) mR.material.SetColor("_Color", Color.red);
                 * коротко, но не работает для не Enabled объектов*/
                // свечение мечей - красным
                swordMeshRenderer.material.EnableKeyword("_EMISSION");
                swordMeshRenderer.material.SetColor("_EmissionColor", new Color(0.6f, 0.2f, 0.2f));
                sword2MeshRenderer.material.EnableKeyword("_EMISSION");
                sword2MeshRenderer.material.SetColor("_EmissionColor", new Color(0.6f, 0.2f, 0.2f));
                twoHandedSwordMeshRenderer.material.EnableKeyword("_EMISSION");
                twoHandedSwordMeshRenderer.material.SetColor("_EmissionColor", new Color(0.6f, 0.2f, 0.2f));
                break;
            case 2:
                // меняем форму щита и двуры
                /* Св-во sharedMesh компонента MeshFilter ссылается на существующий инстанс меша,
                 *  в отличие от GetComponent<MeshFilter>().mesh, который создает инстанс-дубликат*/
                if (shieldMeshFilter.sharedMesh == shieldMesh1) // если щит еще не перевернут
                    heroShield.transform.Rotate(Vector3.forward, 180);  // перевернуть щит один раз на модели 2 и 3
                shieldMeshFilter.mesh = shieldMesh2;
                twoHandedSwordMeshFilter.mesh = twoHandedSwordMesh2;
                shieldMeshRenderer.material = shieldMaterial2;
                twoHandedSwordMeshRenderer.material = twoHandedSwordMaterial2;
                shieldMeshRenderer.material.SetColor("_Color", Color.green);
                swordMeshRenderer.material.SetColor("_Color", Color.green);
                sword2MeshRenderer.material.SetColor("_Color", Color.green);
                twoHandedSwordMeshRenderer.material.SetColor("_Color", Color.green);
                // свечение мечей - зеленым
                swordMeshRenderer.material.EnableKeyword("_EMISSION");
                swordMeshRenderer.material.SetColor("_EmissionColor", new Color(0.2f, 0.6f, 0.2f));
                sword2MeshRenderer.material.EnableKeyword("_EMISSION");
                sword2MeshRenderer.material.SetColor("_EmissionColor", new Color(0.2f, 0.6f, 0.2f));
                twoHandedSwordMeshRenderer.material.EnableKeyword("_EMISSION");
                twoHandedSwordMeshRenderer.material.SetColor("_EmissionColor", new Color(0.2f, 0.6f, 0.2f));
                break;
            case 3:
                // еще раз меняем форму щита и двуры
                shieldMeshFilter.mesh = shieldMesh3;
                twoHandedSwordMeshFilter.mesh = twoHandedSwordMesh3;
                shieldMeshRenderer.material = shieldMaterial3;
                twoHandedSwordMeshRenderer.material = twoHandedSwordMaterial3;
                shieldMeshRenderer.material.SetColor("_Color", Color.yellow);
                swordMeshRenderer.material.SetColor("_Color", Color.yellow);
                sword2MeshRenderer.material.SetColor("_Color", Color.yellow);
                //m_Enemy.twoHandedSwordMeshRenderer.material.SetColor("_Color", Color.yellow);
                // свечение мечей - золотым
                swordMeshRenderer.material.EnableKeyword("_EMISSION");
                swordMeshRenderer.material.SetColor("_EmissionColor", new Color(0.6f, 0.6f, 0.2f));
                sword2MeshRenderer.material.EnableKeyword("_EMISSION");
                sword2MeshRenderer.material.SetColor("_EmissionColor", new Color(0.6f, 0.6f, 0.2f));
                twoHandedSwordMeshRenderer.material.EnableKeyword("_EMISSION");
                twoHandedSwordMeshRenderer.material.SetColor("_EmissionColor", new Color(0.6f, 0.6f, 0.2f));
                // даем врагу кольцо
                if (GameManager.gameType == GameType.Single) inventory.AddItem(enemiesItem1);
                break;
        }

        base.OnEnable();                           

        //2. Усложнить игру базовым уроном в зависимости от кол-ва выигранных раундов
        if (GameManager.gameType == GameType.Single) m_Tweakers.AddLevelTweakers(HeroManager.player_countRoundsWon);
    }

}
