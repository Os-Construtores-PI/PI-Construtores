using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/UI/UI Main Menu")]
	public class UIMainMenu : Singleton<UIMainMenu>
	{
		public enum Screen
		{
			Title,
			FileSelect,
			LevelSelect,
		}

		[System.Serializable]
		public class ScreenData
		{
			[Tooltip("The UI Container to show. Leave empty to load a scene instead.")]
			public UIContainer screen;

			[Tooltip(
				"The scene to load when the screen is shown and its first time condition is met. "
					+ "Leave empty to load the screen or regular scene instead."
			)]
			public string firstTimeScene;

			[Tooltip(
				"The scene to load when the screen is shown. Leave empty to load the screen instead."
			)]
			public string scene;

			/// <summary>
			/// Shows the screen Game Object or loads a scene if the screen is null.
			/// </summary>
			/// <param name="firstTimeCondition">If true, the first time scene will be loaded.</param>
			public virtual void Show(bool firstTimeCondition = false)
			{
				if (firstTimeCondition && !string.IsNullOrEmpty(firstTimeScene))
					GameLoader.instance.Load(firstTimeScene);
				else if (screen != null)
					ShowScreen();
				else if (!string.IsNullOrEmpty(scene))
					GameLoader.instance.Load(scene);
			}

			/// <summary>
			/// Hides the screen Game Object and calls the callback when done.
			/// </summary>
			/// <param name="callback">The callback to call when the screen is hidden.</param>
			/// <param name="disable">If true, the screen Game Object will be disabled.</param>
			public virtual void Hide(UnityAction callback, bool disable = true)
			{
				if (screen != null)
				{
					screen.Hide(() =>
					{
						callback?.Invoke();
						if (disable)
							Disable();
					});
				}
			}

			/// <summary>
			/// Disables the screen Game Object.
			/// </summary>
			public virtual void Disable()
			{
				if (screen != null)
					screen.SetActive(false);
			}

			/// <summary>
			/// Enables the screen Game Object and call UI Container Show method.
			/// </summary>
			protected virtual void ShowScreen()
			{
				screen.SetActive(true);
				screen.Show();
			}
		}

		public ScreenData titleScreen;
		public ScreenData fileSelectScreen;
		public ScreenData levelSelectScreen;

		protected Screen m_currentScreen = Screen.Title;
		protected Dictionary<Screen, ScreenData> m_screens;

		protected virtual void Start()
		{
			InitializeCallbacks();
			InitializeScreens();
			InitializeCurrentScreen();
		}

		protected virtual void InitializeCallbacks()
		{
			Game.instance.onLoadState.AddListener(OnLoadState);
		}

		protected virtual void InitializeScreens()
		{
			m_screens = new()
			{
				{ Screen.Title, titleScreen },
				{ Screen.FileSelect, fileSelectScreen },
				{ Screen.LevelSelect, levelSelectScreen },
			};

			foreach (var data in m_screens.Values)
				data.Disable();
		}

		protected virtual void InitializeCurrentScreen()
		{
			if (Game.instance.dataLoaded && levelSelectScreen.screen)
				m_currentScreen = Screen.LevelSelect;
			else
				m_currentScreen = Screen.Title;

			m_screens[m_currentScreen].Show();
		}

		protected virtual void OnLoadState(int _) => ChangeTo(Screen.LevelSelect);

		/// <summary>
		/// Changes the current screen to the previous one.
		/// </summary>
		public virtual void ChangeToPrevious()
		{
			var index = (int)m_currentScreen - 1;
			var totalScreens = System.Enum.GetValues(typeof(Screen)).Length;
			index = Mathf.Clamp(index, 0, totalScreens - 1);
			ChangeTo((Screen)index);
		}

		/// <summary>
		/// Changes the current screen to the specified one.
		/// </summary>
		/// <param name="screen">The screen to change to.</param>
		public virtual void ChangeTo(Screen screen)
		{
			if (m_currentScreen == screen)
				return;

			m_screens[m_currentScreen]
				.Hide(() =>
				{
					var isFirstTime = IsFirstTime(screen);
					m_currentScreen = screen;
					m_screens[m_currentScreen].Show(isFirstTime);
				});
		}

		/// <summary>
		/// Returns true if the screen is shown for the first time.
		/// </summary>
		/// <param name="screen">The screen to check.</param>
		public virtual bool IsFirstTime(Screen screen)
		{
			return screen switch
			{
				Screen.Title => false,
				Screen.FileSelect => GameSaver.instance.HasAnyData(),
				Screen.LevelSelect => Game.instance.HasAnyBeatenLevel(),
				_ => false,
			};
		}
	}
}
