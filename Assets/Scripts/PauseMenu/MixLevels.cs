using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class MixLevels : MonoBehaviour {

	private static MixLevels _instance;
	public static MixLevels Instance => _instance;
	
	public AudioMixer masterMixer;                          // ссылка на главный миксер

	private void Awake()
	{
		_instance = this;
	}
	
	public void SetSfxLvl(float sfxLvl)
	{
		masterMixer.SetFloat("sfxVolume", sfxLvl);
        masterMixer.SetFloat("moveVolume", sfxLvl-12f);     // отдельный слайдер и переменную для громкости перемещений делать не стал - просто -12Дб относительно sfx
    }

    public void SetMusicLvl (float musicLvl)
	{
		masterMixer.SetFloat ("musicVolume", musicLvl);    // устанавливаем занчение musicLvl в  exposed parametr "musicVolume"
    }
}
