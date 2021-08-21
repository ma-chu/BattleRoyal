using System;
using EF.Localization;
using EF.Sounds;
using UnityEngine;

	public static class GameSave
	{
		public const string _SaveKey = "SAVE";

		public static SaveSnapshot LastLoadedSnapshot { get; private set; }
		
		public static bool SaveKeyExists()
		{
			return PlayerPrefs.HasKey(_SaveKey) && PlayerPrefs.GetString(_SaveKey).Length > 0;
		}
		
		
		public static SaveSnapshot Load()
		{
			if (!SaveKeyExists()) return null;

			var json = PlayerPrefs.GetString(_SaveKey);
			LastLoadedSnapshot = JsonUtility.FromJson<SaveSnapshot>(json);
			
			Debug.Log(String.Format("Load() done. Lang = {0}. CUPS = {1}. SFXlvl = {2}. MusLvl = {3}.",
				LastLoadedSnapshot.language, LastLoadedSnapshot.tournamentsWon, LastLoadedSnapshot.SFXLvl, LastLoadedSnapshot.musicLvl));
			
			return LastLoadedSnapshot;
		}
		

		public static void Save()
		{
			var snapshot = LastLoadedSnapshot ?? new SaveSnapshot
			{
				//Time = DateTime.Now,
				language = Localization.CurrentLanguage,
				tournamentsWon = 0
			};
			snapshot.language = Localization.CurrentLanguage;
			
			SoundsManager.Instance.masterMixer.GetFloat("sfxVolume", out float sfx);
			snapshot.SFXLvl = sfx;
			SoundsManager.Instance.masterMixer.GetFloat("musicVolume", out float mus);
			snapshot.musicLvl = mus;
				
			Save(snapshot);

			Debug.Log(String.Format("Save() done. Lang = {0}. CUPS = {1}. SFXlvl = {2}. MusLvl = {3}.",
				snapshot.language, snapshot.tournamentsWon, snapshot.SFXLvl, snapshot.musicLvl));
		}
		
		
		public static void Save(SaveSnapshot snapshot)
		{
			var json = JsonUtility.ToJson(snapshot);

			PlayerPrefs.SetString(_SaveKey, json);
			PlayerPrefs.Save();
		}
		
		
		public static void ClearSave()
		{
			PlayerPrefs.DeleteKey(_SaveKey);
		}

	}