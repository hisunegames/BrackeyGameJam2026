using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Mainmenu : MonoBehaviour
{
    [SerializeField] GameObject mainMenuPanel;
    [SerializeField] Button StartButton;
    [SerializeField] Button QuitButton;
    private CanvasGroup mainmenuCanvasgroup;
    private Coroutine StartGameCoroutine;

    void Start()
    {
        mainmenuCanvasgroup = mainMenuPanel.GetComponent<CanvasGroup>();
        Time.timeScale = 0f;
        if (StartGameCoroutine != null)
        {
            StopCoroutine(StartGameCoroutine);
            StartGameCoroutine = null;
        }
    }

    public void EnterGame()
    {
        StartGameCoroutine = StartCoroutine(FadeOutMenu());
    }

    IEnumerator FadeOutMenu()
    {
        yield return null;
        float duration = 2f;
        float elapsed = 0f;
        float startalpha = mainmenuCanvasgroup.alpha;
        StartButton.interactable = false;
        QuitButton.interactable = false;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            mainmenuCanvasgroup.alpha = Mathf.Lerp(startalpha , 0f , elapsed/duration);
            yield return null;
        }
        Time.timeScale = 1f;
        mainmenuCanvasgroup.alpha = 0f;
        mainMenuPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
