using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    [Header("Health UI References")]
    [SerializeField] private RectTransform hpBase;
    [SerializeField] private RectTransform hpFill;   
    [SerializeField] private GameObject hpParent;

    [Header("Focus UI References")]
    [SerializeField] private GameObject focusParent;
    [SerializeField] private Image[] focusBoxes; 
    [SerializeField] private Sprite focusFilled;
    [SerializeField] private Sprite focusEmpty;

    [Header("Action UI")]
    [SerializeField] private GameObject actionPanel;

    [Header("Action Buttons")]
    [SerializeField] private Button enhanceButton;
    [SerializeField] private Button specialButton;
    [SerializeField] private Button stanceStoneButton;
    [SerializeField] private Button stanceWindButton;
    [SerializeField] private Button stanceFlameButton;

    public void StartUI()
    {
        focusParent.SetActive(true);
        hpParent.SetActive(true);
    }

    public void UpdateHP(int current, int max)
    {
        float percent = Mathf.Clamp01((float)current / max);
        float baseWidth = hpBase.rect.width;  
        hpFill.sizeDelta = new Vector2(baseWidth * percent, hpFill.sizeDelta.y);
    }

    public void UpdateFocus(int focus)
    {
        for (int i = 0; i < focusBoxes.Length; i++)
        {
            focusBoxes[i].sprite = (i < focus ? focusFilled : focusEmpty);
        }
    }

    public void ShowActions(bool show)
    {
        actionPanel.SetActive(show);
    }
    public void SetSpecialInteractable(bool value)
    {
        specialButton.interactable = value;
    }

    public void SetEnhanceInteractable(bool value)
    {
        enhanceButton.interactable = value;
    }

    public void SetStanceInteractable(Stance current)
    {
        stanceStoneButton.interactable = (current != Stance.Stone);
        stanceWindButton.interactable = (current != Stance.Wind);
        stanceFlameButton.interactable = (current != Stance.Flame);
    }
}
