using UnityEngine;
using UnityEngine.Audio;        // пространство имен для аудио-миксеров и их снимков
using EF.Tools;
using UnityEngine.UI;
#if UNITY_EDITOR                // скрипт запущен из редактора (не как приложение)
using UnityEditor;
#endif

public class PauseManager : MonoBehaviour {
	
	[SerializeField] private AudioMixerSnapshot startPaused;                         // ссылки на снимки состояний миксера
	[SerializeField] private AudioMixerSnapshot startUnpaused;
	
	Canvas canvas;

	private AudioMixerSnapshot _paused;
	private AudioMixerSnapshot _unpaused;

	[SerializeField] private Slider sFXSlider;
	[SerializeField] private Slider musicSlider;

	void Start()
	{
		_paused = startPaused;
		_unpaused = startUnpaused;
		
		canvas = GetComponent<Canvas>();
        canvas.enabled = false;
        
        var snapshot = GameSave.LastLoadedSnapshot ?? GameSave.Load();
        if (!snapshot.IsNull())
        {
	        _unpaused.audioMixer.SetFloat("sfxVolume", snapshot.SFXLvl);
	        _unpaused.audioMixer.SetFloat("moveVolume", snapshot.SFXLvl - 12f);
	        _unpaused.audioMixer.SetFloat("musicVolume", snapshot.musicLvl);

	        _paused.audioMixer.SetFloat("sfxVolume", snapshot.SFXLvl - 40f);
	        _paused.audioMixer.SetFloat("moveVolume", snapshot.SFXLvl - 28f);
	        _paused.audioMixer.SetFloat("musicVolume", snapshot.musicLvl - 20f);

	        sFXSlider.value = snapshot.SFXLvl;
	        musicSlider.value = snapshot.musicLvl;
        }
	}
	
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))                // равно нажатию "назад" на телефоне
		{
			Pause();                                         // эта же функция вызывается по нажатию кнопки resume доп. меню при помощи UnityEvents
		}
	}
	
	public void Pause()
	{
        canvas.enabled = !canvas.enabled;
        Time.timeScale = Time.timeScale == 0 ? 1 : 0;       // заморозить/разморозить движение объектов в игре - тут не очень актуально
		Lowpass ();                                         // уменьшить звуки до уровня снимка "Paused"
		
	}
	
	void Lowpass()
	{
		if (Time.timeScale == 0)
		{
			/*startPaused*/_paused.TransitionTo(.01f);                     // достичь уровнем звуков состояния этого снимка за 0.01 сек
		}
		
		else
			
		{
			/*startUnpaused*/_unpaused.TransitionTo(.01f);
		}
	}
	
	public void Quit()                                      // функция вызывается по нажатию кнопки quit доп. меню при помощи UnityEvents
    {
		#if UNITY_EDITOR 
		EditorApplication.isPlaying = false;
		#else 
		Application.Quit();
		#endif
	}
}
