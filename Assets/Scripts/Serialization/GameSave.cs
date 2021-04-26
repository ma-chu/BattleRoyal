using System;
using System.IO;
using EF.Localization;
using UnityEditor;
using UnityEngine;

	public static class GameSave
	{
		public const string _KEY = "SAVE";

		//private const string FIRST_SESSION = "HAS_FIRST_SESSION";

		//public static bool IsFirstSession => !PlayerPrefs.HasKey(FIRST_SESSION);
		
		//public const float SAVE_DELAY = 5f;
		
		//public static float SaveDelay { get; private set; } = SAVE_DELAY;
		
		public static SaveSnapshot LastLoadedSnapshot { get; private set; }
		
		public static bool Exists()
		{
			return PlayerPrefs.HasKey(_KEY) && PlayerPrefs.GetString(_KEY).Length > 0;
		}
		
		
		public static SaveSnapshot Load()
		{
			if (!Exists()) return null;

			var json = PlayerPrefs.GetString(_KEY);
			LastLoadedSnapshot = JsonUtility.FromJson<SaveSnapshot>(json);

			Debug.Log("Load() done "+LastLoadedSnapshot.language + " " + LastLoadedSnapshot.tournamentsWon);
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
			Save(snapshot);
			Debug.Log("Save() done " + snapshot.language + " " + snapshot.tournamentsWon);
		}
		
		
		public static void Save(SaveSnapshot snapshot)
		{
			var json = JsonUtility.ToJson(snapshot);

			PlayerPrefs.SetString(_KEY, json);
			PlayerPrefs.Save();
		}
		
		
		public static void ClearSave() 
		{
			PlayerPrefs.DeleteKey(_KEY);
        
			//if (!PlayerPrefs.HasKey(FIRST_SESSION)) PlayerPrefs.SetString(FIRST_SESSION, "YES");
		}
		
		/*public static void Update(float deltaTime)
		{
			if (SaveDelay.EqualTo(0f)) return;

			SaveDelay -= deltaTime;

			if (SaveDelay > 0f) return;

			SaveDelay = SAVE_DELAY;
			Save();
		}*/
		

	}