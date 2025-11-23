using System.Collections;
using UnityEngine;

public enum PlayerAction
{
    None,
    Attack,
    Special,
    ChangeStance
}

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    [SerializeField] private PlayerCombat player;
    [SerializeField] private EnemyCombat enemy;
    [SerializeField] private int currentLevel;

    private bool waitingForPlayerChoice = false;
    private PlayerAction chosenAction = PlayerAction.None;
    private PlayerAttackType chosenAttackType = PlayerAttackType.Basic;

    private const float telegraphDelay = 0.75f;

    private static WaitForSeconds W(float t) => new WaitForSeconds(t);

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(MainCombatLoop());
    }

    IEnumerator MainCombatLoop()
    {
        yield return new WaitUntil(() => CameraController.Instance.isStarted);
        yield return W(1f);

        UIManager.Instance.StartUI();
        UIManager.Instance.UpdatePlayerHP(player.CurrentHp, player.maxHp);
        UIManager.Instance.UpdateEnemyHP(enemy.currentHP, enemy.maxHP);
        UIManager.Instance.UpdatePlayerFocus(player.Focus);

        player.PlayAnimationIdle();
        enemy.PlayIdleForCurrentStance();

        while (player.CurrentHp > 0 && enemy.currentHP > 0)
        {
            yield return StartCoroutine(PlayerTurn());
            if (enemy.currentHP <= 0) break;

            yield return StartCoroutine(EnemyTurn());
            if (player.CurrentHp <= 0) break;
        }
    }

    // -------------------- PLAYER TURN --------------------
    IEnumerator PlayerTurn()
    {
        player.PlayAnimationIdle();

        chosenAction = PlayerAction.None;
        waitingForPlayerChoice = true;

        UIManager.Instance.RefreshPlayerActionButtons(player.CurrentStance, player.Focus);
        UIManager.Instance.ShowPlayerActions(true);

        while (waitingForPlayerChoice)
            yield return null;

        UIManager.Instance.ShowPlayerActions(false);

        switch (chosenAction)
        {
            case PlayerAction.ChangeStance:
                UIManager.Instance.UpdatePlayerFocus(player.Focus);
                yield return W(1f);
                break;

            case PlayerAction.Special:
                chosenAttackType = GetSpecialByStance(player.CurrentStance);
                yield return StartCoroutine(ExecutePlayerAttack(chosenAttackType));
                break;

            case PlayerAction.Attack:
                yield return StartCoroutine(ExecutePlayerAttack(chosenAttackType));
                break;
        }

        player.PlayAnimationIdle();
        yield return W(1f);
    }

    public void OnChooseStone()
    {
        player.ChangeStance(Stance.Stone);
        chosenAction = PlayerAction.ChangeStance;
        waitingForPlayerChoice = false;
    }

    public void OnChooseWind()
    {
        player.ChangeStance(Stance.Wind);
        chosenAction = PlayerAction.ChangeStance;
        waitingForPlayerChoice = false;
    }

    public void OnChooseFlame()
    {
        player.ChangeStance(Stance.Flame);
        chosenAction = PlayerAction.ChangeStance;
        waitingForPlayerChoice = false;
    }

    public void OnPlayerBasicButton()
    {
        chosenAttackType = PlayerAttackType.Basic;
        chosenAction = PlayerAction.Attack;
        waitingForPlayerChoice = false;
    }

    public void OnPlayerEnhanceButton()
    {
        chosenAttackType = PlayerAttackType.Enhance;
        chosenAction = PlayerAction.Attack;
        waitingForPlayerChoice = false;

        player.Focus -= 3;
    }

    public void OnPlayerSpecialButton()
    {
        chosenAction = PlayerAction.Special;
        waitingForPlayerChoice = false;

        player.Focus -= 6;
    }

    // -------------------- PLAYER ATTACK --------------------
    private IEnumerator ExecutePlayerAttack(PlayerAttackType type)
    {
        enemy.turnParrySuccess = true;

        int hitCount = GetHitCountForAttack(type);

        for (int hit = 1; hit <= hitCount; hit++)
        {
            player.PlayTelegraph(type, hit);

            GetDefaultBands(out float perfect, out float small, out float medium);
            yield return StartCoroutine(TimingManager.Instance.RunMinigame(perfect, small, medium));

            TimingResult result = TimingManager.Instance.LastResult;

            float parryChance = result switch
            {
                TimingResult.Perfect => enemy.baseEnemyParry,
                TimingResult.SmallMiss => enemy.baseEnemyParry + 0.25f,
                TimingResult.MediumMiss => enemy.baseEnemyParry + 0.50f,
                _ => 1f
            };

            parryChance = Mathf.Clamp01(parryChance);

            bool enemyParried = Random.value < parryChance;

            player.PlayAttack(type, hit);

            if (enemyParried)
            {
                if (player.CurrentStance == Stance.Flame)
                {
                    int chip = Mathf.RoundToInt(player.baseAttack * 0.3f);
                    enemy.TakeDamage(chip);
                    UIManager.Instance.UpdateEnemyHP(enemy.currentHP, enemy.maxHP);
                    enemy.PlayPierced();
                }
                else
                {
                    enemy.PlayParried();
                }

                enemy.turnParrySuccess = false;
            } else
            {
                int dmg = player.CalculateAttackDamage();
                enemy.TakeDamage(dmg);

                if(chosenAction == PlayerAction.Attack)
                {
                    player.AddFocus();
                    UIManager.Instance.UpdatePlayerFocus(player.Focus);
                }

                UIManager.Instance.UpdateEnemyHP(enemy.currentHP, enemy.maxHP);
                enemy.PlayHit();
            }

            yield return W(1f);

            enemy.PlayIdleForCurrentStance();
            player.PlayAnimationIdle();
        }

        if (enemy.turnParrySuccess && enemy.currentStance == Stance.Wind)
        {
            if (player.CurrentStance != Stance.Stone)
            {
                int counter = Mathf.RoundToInt(enemy.baseAttack * 1.2f);
                player.TakeDamage(counter);
                UIManager.Instance.UpdatePlayerHP(player.CurrentHp, player.maxHp);
                player.PlayCountered();
            }
        }

        if (chosenAction == PlayerAction.Special) StartCoroutine(enemy.PlaySpecialEffect(player.CurrentStance));

        if (enemy.currentHP <= 0) StartCoroutine(EndCombat(true));

        yield return W(1f);
    }

    private int GetHitCountForAttack(PlayerAttackType type)
    {
        return type switch
        {
            PlayerAttackType.Basic => 1,
            PlayerAttackType.Enhance => 2,
            PlayerAttackType.StoneSpecial => 2,
            PlayerAttackType.WindSpecial => 1,
            PlayerAttackType.FlameSpecial => 4,
            _ => 1
        };
    }

    private PlayerAttackType GetSpecialByStance(Stance s)
    {
        return s switch
        {
            Stance.Stone => PlayerAttackType.StoneSpecial,
            Stance.Wind => PlayerAttackType.WindSpecial,
            Stance.Flame => PlayerAttackType.FlameSpecial,
            _ => PlayerAttackType.Basic
        };
    }

    // -------------------- ENEMY TURN --------------------
    IEnumerator EnemyTurn()
    {
        if (enemy.TryChangeStance())
        {
            yield return StartCoroutine(enemy.PlayStanceChangeAnim(enemy.currentStance));
            yield return W(0.25f);
            enemy.PlayIdleForCurrentStance();
            yield break;
        }

        player.turnParrySuccess = true;

        EnemyPattern pattern = enemy.DecidePattern();

        if (pattern == EnemyPattern.Pattern1)
            yield return StartCoroutine(RunEnemyPattern(1, 2));
        else if (pattern == EnemyPattern.Pattern2)
            yield return StartCoroutine(RunEnemyPattern(2, 2));
        else
        {
            yield return StartCoroutine(RunEnemyPattern(2, 2));
            yield return W(1f);
            yield return StartCoroutine(RunEnemyPattern(1, 2));
        }

        enemy.PlayIdleForCurrentStance();

        if (player.turnParrySuccess && player.CurrentStance == Stance.Wind)
        {
            if (enemy.currentStance != Stance.Stone)
            {
                int counter = player.CalculateWindCounter((int)enemy.baseAttack);
                enemy.TakeDamage(counter);
                UIManager.Instance.UpdateEnemyHP(enemy.currentHP, enemy.maxHP);
                enemy.PlayCountered();
            }
        }

        if (player.IsParrying)
            player.ResetParry();

        if (player.CurrentHp <= 0) StartCoroutine(EndCombat(false));

        yield return W(1f);
    }

    // -------------------- ENEMY MULTI-HIT PATTERN --------------------
    private IEnumerator RunEnemyPattern(int patternId, int hitCount)
    {
        for (int hit = 1; hit <= hitCount; hit++)
        {
            enemy.ShowHideWarningText();
            enemy.PlayTelegraph(patternId, hit);

            float elapsed = 0f;

            while (elapsed < telegraphDelay)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    player.TryStartParry();
                }

                elapsed += Time.deltaTime;
                yield return null;
            }
            enemy.ShowHideWarningText();

            enemy.PlayAttack(patternId, hit);

            if (player.IsParrying)
            {
                if (enemy.currentStance == Stance.Flame)
                {
                    int steal = Mathf.RoundToInt(enemy.baseAttack * 0.3f);
                    player.TakeDamage(steal);
                    UIManager.Instance.UpdateEnemyHP(enemy.currentHP, enemy.maxHP);
                    player.PlayPierced();
                } else
                {
                    player.PlaySuccessfulParry();
                }

                player.AddFocus();
                UIManager.Instance.UpdatePlayerFocus(player.Focus);
            }
            else
            {
                int dmg = enemy.CalculateEnemyAttackDamage();
                player.TakeDamage(dmg);
                UIManager.Instance.UpdatePlayerHP(player.CurrentHp, player.maxHp);
                player.PlayHit();
                player.turnParrySuccess = false;
            }

            yield return W(1f);

            player.ResetParry();
            enemy.PlayIdleForCurrentStance();
            player.PlayAnimationIdle();
        }
    }

    // -------------------- HELPERS --------------------
    private void GetDefaultBands(out float perfect, out float small, out float medium)
    {
        perfect = TimingManager.Instance.basePerfect;
        small = TimingManager.Instance.baseSmall;
        medium = TimingManager.Instance.baseMedium;
    }

    private IEnumerator EndCombat(bool isPlayerWin)
    {
        if (isPlayerWin) enemy.PlayDeath();
        else player.PlayDeath();

        yield return W(3f);

        if (isPlayerWin) Transition.Instance.CloseScene("Level_" + (currentLevel + 1));
        else Transition.Instance.CloseScene("Level_" + currentLevel);
    }
}