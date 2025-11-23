using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [SerializeField] private PlayerUIManager playerUI;
    [SerializeField] private EnemyUIManager enemyUI;

    void Awake()
    {
        Instance = this;
    }

    public void StartUI()
    {
        playerUI.StartUI();
        enemyUI.StartUI();
    }

    //==============================
    //  REFERENCE PLAYER HELPER
    //==============================

    public void UpdatePlayerHP(int current, int max)
    {
        playerUI.UpdateHP(current, max);
    }

    public void UpdatePlayerFocus(int focus)
    {
        playerUI.UpdateFocus(focus);
    }

    public void ShowPlayerActions(bool show)
    {
        playerUI.ShowActions(show);
    }

    public void RefreshPlayerActionButtons(Stance current, int focus)
    {
        playerUI.SetStanceInteractable(current);
        playerUI.SetSpecialInteractable(focus >= 6 && current != Stance.None);
        playerUI.SetEnhanceInteractable(focus >= 3);
    }

    //==============================
    //   REFERENCE ENEMY HELPER
    //==============================
    public void UpdateEnemyHP(int current, int max)
    {
        enemyUI.UpdateHP(current, max);
    }
}
