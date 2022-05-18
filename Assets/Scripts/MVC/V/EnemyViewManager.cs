using System;
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

public class EnemyViewManager : HeroViewManager
{
    [SerializeField] private WeaponChanges[] Weapons;               // изменения мешей и материалов оружия врага - в HEROManager!!!

    private bool _rotated;
    
    protected override void Awake()
    {
        base.Awake();                                              

        heroType = Heroes.Enemy; //- в конструкторе в идеале, но пока и здесь норм
    }
    
    public void ChangeWeaponsView(int winsZeroBased)
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
