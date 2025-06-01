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

        HeroType = Heroes.Enemy; //- в конструкторе в идеале, но пока и здесь норм
    }
    
    public void ChangeWeaponsView(int winsZeroBased)
    {
        // меш и материал щита и лонга
        _shieldMeshFilter.mesh = Weapons[winsZeroBased].shieldMesh;                                         
        _shieldMeshRenderer.material = Weapons[winsZeroBased].shieldMat;
        if (!_rotated && winsZeroBased == 1)
        {
            heroShield.transform.Rotate(Vector3.forward, 180);        // перевернуть щит один раз на 3 раунде для моделей 2 и 3
            _rotated = true;
        }
        _twoHandedSwordMeshFilter.mesh = Weapons[winsZeroBased].longMesh;                                         
        _twoHandedSwordMeshRenderer.material = Weapons[winsZeroBased].longMat;
        
        // цвет меча, щита и лонга
        //_shieldMeshRenderer.material.SetColor("Color", Weapons[round].color);  // так было до URP
        _shieldMeshRenderer.material.color = Weapons[winsZeroBased].color;
        _swordMeshRenderer.material.color = Weapons[winsZeroBased].color;
        _sword2MeshRenderer.material.color = Weapons[winsZeroBased].color;
        _twoHandedSwordMeshRenderer.material.color = Weapons[winsZeroBased].color;
        /* foreach (MeshRenderer mR in /m_Enemy/this.GetComponentsInChildren<MeshRenderer>()) mR.material.SetColor("_Color", Color.red);
         * коротко, но не работает для не Enabled объектов*/
        // свечение мечей
        _swordMeshRenderer.material.EnableKeyword("_EMISSION");
        _swordMeshRenderer.material.SetColor("_EmissionColor", Weapons[winsZeroBased].color);
        _sword2MeshRenderer.material.EnableKeyword("_EMISSION");
        _sword2MeshRenderer.material.SetColor("_EmissionColor", Weapons[winsZeroBased].color);
        _twoHandedSwordMeshRenderer.material.EnableKeyword("_EMISSION");
        _twoHandedSwordMeshRenderer.material.SetColor("_EmissionColor", Weapons[winsZeroBased].color);
        _twoHandedSwordMeshRenderer.material.SetTexture("_EmissionMap", _twoHandedSwordMeshRenderer.material.mainTexture);
    }
}
