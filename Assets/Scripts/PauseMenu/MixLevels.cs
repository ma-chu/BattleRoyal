using UnityEngine;
using EF.Sounds;

public class MixLevels : MonoBehaviour {

	private static MixLevels _instance;
	public static MixLevels Instance => _instance;
	
	///*public*/private AudioMixer masterMixer;                          // ссылка на главный миксер

	private void Awake()
	{
		_instance = this;
	}
	
	public void SetSfxLvl(float sfxLvl)
	{
		SoundsManager.Instance.masterMixer.SetFloat("sfxVolume", sfxLvl);
		SoundsManager.Instance.masterMixer.SetFloat("moveVolume", sfxLvl-12f);     // отдельный слайдер и переменную для громкости перемещений делать не стал - просто -12Дб относительно sfx
    }

    public void SetMusicLvl (float musicLvl)
	{
		SoundsManager.Instance.masterMixer.SetFloat ("musicVolume", musicLvl);    // устанавливаем занчение musicLvl в  exposed parametr "musicVolume"
    }
}
