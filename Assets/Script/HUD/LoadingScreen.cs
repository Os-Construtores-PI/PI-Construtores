using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
  [Header("UI")]
  [SerializeField] private GameObject _loadingRoot;
  [SerializeField] private Image _progressBar;
  [SerializeField] private TextMeshProUGUI _loadingText;
  [SerializeField] private RectTransform _spinner;

  private Coroutine _dotsRoutine;
    

  public void LoadScene(string sceneName)
  {
    _loadingRoot.SetActive(true);

    StartCoroutine(LoadSceneAsync(sceneName));  
  }

  private IEnumerator LoadSceneAsync(string sceneName)
  {
    _dotsRoutine = StartCoroutine(AnimateDots());

    AsyncOperation operation =
      SceneManager.LoadSceneAsync(sceneName);

    operation.allowSceneActivation = false;

    float loadingTime = 0f;
    float minMinute = 3f;

    while (operation.progress < 0.9f || loadingTime < minMinute)
    {
      loadingTime += Time.deltaTime;

      float progress =
        Mathf.Clamp01(loadingTime / minMinute);

      _progressBar.fillAmount =
        Mathf.Lerp(
          _progressBar.fillAmount,
          progress,
          4f * Time.deltaTime);

      _spinner.Rotate(0,0, -90 * Time.deltaTime);

      yield return null;
    }

    _progressBar.fillAmount = 1f;

    yield return new WaitForSeconds(0.5f);

    if(_dotsRoutine != null)
    {
      StopCoroutine(_dotsRoutine);
      _dotsRoutine = null;
    }

    operation.allowSceneActivation = true;
  }

  private IEnumerator AnimateDots()
  {
    while (true)
    {
      _loadingText.text = "CARREGANDO.";
      yield return new WaitForSeconds(.3f);

      _loadingText.text = "CARREGANDO..";
      yield return new WaitForSeconds(.7f);

      _loadingText.text = "CARREGANDO...";
      yield return new WaitForSeconds(1f);
    }
  }
}
