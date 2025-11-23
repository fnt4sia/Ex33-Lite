using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Transition : MonoBehaviour
{
    public static Transition Instance;

    public Animator animator;
    public float transitionTime;
    public Image image;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void CloseScene(string sceneName)
    {
        StartCoroutine(CloseAndLoad(sceneName));
    }

    private IEnumerator CloseAndLoad(string sceneName)
    {
        Color c = image.color; 
        c.a = 1f;              
        image.color = c;

        animator.Play("FadeOut");

        yield return new WaitForSeconds(transitionTime + 0.5f);

        SceneManager.LoadScene(sceneName);

        animator.Play("FadeIn");
    }
}
