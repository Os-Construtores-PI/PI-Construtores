using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Game/Game Loader")]
	public class GameLoader : Singleton<GameLoader>
	{
		/// <summary>
		/// Called when any loading process has started.
		/// </summary>
		public UnityEvent OnLoadStart;

		/// <summary>
		/// Called when any loading process has finished.
		/// </summary>
		public UnityEvent OnLoadFinish;

		public UIContainer loadingScreen;

		[Header("Minimum Time")]
		public float startDelay = 1f;
		public float finishDelay = 1f;

		/// <summary>
		/// Returns true if there's any loading in process.
		/// </summary>
		public bool isLoading { get; protected set; }

		/// <summary>
		/// Returns the loading percentage.
		/// </summary>
		public float loadingProgress { get; protected set; }

		/// <summary>
		/// Returns the name of the current scene.
		/// </summary>
		public string currentScene => SceneManager.GetActiveScene().name;

		/// <summary>
		/// Reloads the current scene.
		/// </summary>
		public virtual void Reload()
		{
			StartCoroutine(LoadRoutine(currentScene));
		}

		/// <summary>
		/// Loads a given scene based on its name.
		/// </summary>
		/// <param name="scene">The name of the scene you want to load.</param>
		public virtual void Load(string scene)
		{
			if (!isLoading && (currentScene != scene))
			{
				StartCoroutine(LoadRoutine(scene));
			}
		}

		protected virtual IEnumerator LoadRoutine(string scene)
		{
			OnLoadStart?.Invoke();
			isLoading = true;
			loadingScreen.SetActive(true);
			loadingScreen.Show();

			yield return new WaitForSeconds(startDelay);

			var operation = SceneManager.LoadSceneAsync(scene);
			loadingProgress = 0;

			while (!operation.isDone)
			{
				loadingProgress = operation.progress;
				yield return null;
			}

			loadingProgress = 1;

			yield return new WaitForSeconds(finishDelay);

			isLoading = false;
			loadingScreen.Hide();
			OnLoadFinish?.Invoke();
		}
	}
}
