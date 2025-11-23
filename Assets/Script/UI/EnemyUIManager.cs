using UnityEngine;
using UnityEngine.UI;

public class EnemyUIManager : MonoBehaviour
{
    [Header("Health UI References")]
    [SerializeField] private RectTransform hpBase;
    [SerializeField] private RectTransform hpFill;
    [SerializeField] private GameObject hpParent;

    public void StartUI()
    {
        hpParent.SetActive(true);
    }

    public void UpdateHP(int current, int max)
    {
        float percent = Mathf.Clamp01((float)current / max);
        float baseWidth = hpBase.rect.width;
        hpFill.sizeDelta = new Vector2(baseWidth * percent, hpFill.sizeDelta.y);
    }
}
