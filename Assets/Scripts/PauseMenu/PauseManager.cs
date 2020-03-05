using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Audio;        // пространство имен для аудио-миксеров и их снимков
#if UNITY_EDITOR                // скрипт запущен из редактора (или как приложение?)
using UnityEditor;
#endif

public class PauseManager : MonoBehaviour {
	
	public AudioMixerSnapshot paused;                         // ссылки на снимки состояний миксера
	public AudioMixerSnapshot unpaused;
	
	Canvas canvas;
	
	void Start()
	{
		canvas = GetComponent<Canvas>();
        canvas.enabled = false;
    }
	
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))               //?? равно ли нажатию "назад" на телефоне?
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
			paused.TransitionTo(.01f);                     // достичь уровнем звуков состояния этого снимка за 0.01 сек
		}
		
		else
			
		{
			unpaused.TransitionTo(.01f);
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
