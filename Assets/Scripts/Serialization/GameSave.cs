using EF.Localization;
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

			Debug.Log("Load() done. Lang = "+ LastLoadedSnapshot.language + ". CUPS = " + LastLoadedSnapshot.tournamentsWon
			          + ". SFXlvl = " + LastLoadedSnapshot.SFXLvl + ". MusLvl = " + LastLoadedSnapshot.musicLvl);
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
			
			MixLevels.Instance.masterMixer.GetFloat("sfxVolume", out float sfx);
			snapshot.SFXLvl = sfx;
			MixLevels.Instance.masterMixer.GetFloat("musicVolume", out float mus);
			snapshot.musicLvl = mus;
				
			Save(snapshot);
			Debug.Log("Save() done. Lang = " + snapshot.language + ". CUPS = " + snapshot.tournamentsWon
			          + ". SFXlvl = " + snapshot.SFXLvl + ". MusLvl = " + snapshot.musicLvl);
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