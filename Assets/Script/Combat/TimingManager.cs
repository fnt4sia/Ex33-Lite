using NUnit.Framework.Internal;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum TimingResult { Perfect, SmallMiss, MediumMiss, Failed }

public class TimingManager : MonoBehaviour
{
    public static TimingManager Instance;

    [SerializeField] private KeyCode timingKey = KeyCode.Space;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject indicatorGameObject;

    [Header("Base Bands (in seconds, unscaled)")]
    public float basePerfect = 0.125f;
    public float baseSmall = 0.25f;
    public float baseMedium = 0.35f;

    public TimingResult LastResult { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    public IEnumerator RunMinigame(float perfectBand, float smallBand, float mediumBand)
    {
        indicatorGameObject.SetActive(true);

        LastResult = TimingResult.Failed;

        float duration = 1f + mediumBand;
        float t = 0f;
        float? pressTime = null;

        animator.Play("TimingIndicator");

        while (t < duration)
        {
            t += Time.deltaTime;

            if (Input.GetKeyDown(timingKey))
            {
                pressTime = t;
                break;
            }

            yield return null;
        }

        if (pressTime == null)
        {
            LastResult = TimingResult.Failed;
        }
        else
        {
            float delta = Mathf.Abs(1f - pressTime.Value);

            if (delta <= perfectBand)
                LastResult = TimingResult.Perfect;

            else if (delta <= smallBand)
                LastResult = TimingResult.SmallMiss;

            else if (delta <= mediumBand)
                LastResult = TimingResult.MediumMiss;

            else
                LastResult = TimingResult.Failed;
        }


        switch (LastResult)
        {
            case TimingResult.Perfect:
                animator.Play("PerfectIndicator");
                break;

            case TimingResult.SmallMiss:
                animator.Play("SmallMistakeIndicator");
                break;

            case TimingResult.MediumMiss:
                animator.Play("MediumMistakeIndicator");
                break;

            default:
                animator.Play("FailedIndicator");
                break;
        }

        yield return new WaitForSeconds(0.2f);

        indicatorGameObject.SetActive(false);

        yield return new WaitForSeconds(0.2f);
    }
}