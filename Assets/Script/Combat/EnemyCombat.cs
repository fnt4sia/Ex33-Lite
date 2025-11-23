using System.Collections;
using UnityEngine;

public enum EnemyPattern
{
    Pattern1,
    Pattern2,
    Pattern3 // Pattern3 = Pattern2 then Pattern1
}

public class EnemyCombat : MonoBehaviour
{
    [Header("Stats")]
    public int maxHP = 100;
    public int currentHP = 100;
    public float baseAttack = 8f;

    [Header("Parry / Attack")]
    public float baseEnemyParry = 0.10f;

    [Header("Enemy Parry Tracking")]
    public bool turnParrySuccess = true;

    [SerializeField] private Animator animator;
    [SerializeField] private Animator effectAnimator;

    [Header("Pattern selection settings")]
    [Range(0f, 1f)] public float chancePattern1 = 0.45f;
    [Range(0f, 1f)] public float chancePattern2 = 0.45f;
    [Range(0f, 1f)] public float chancePattern3 = 0.10f;

    [Header("UI")]
    [SerializeField] private GameObject warningText;
    [SerializeField] private FloatingTextSpawner floatingTextSpawner;

    [Header("Other")]
    public Stance currentStance = Stance.None;

    private void Awake()
    {
        currentHP = maxHP;
    }

    // =========================
    // Pattern selection
    // =========================
    public EnemyPattern DecidePattern()
    {
        float r = Random.value;

        if (r < chancePattern1) return EnemyPattern.Pattern1;
        if (r < chancePattern1 + chancePattern2) return EnemyPattern.Pattern2;
        return EnemyPattern.Pattern3;
    }

    // =========================
    // Playing telegraph / attack animations
    // =========================
    public void PlayTelegraph(int patternId, int hitIndex)
    {
        if (patternId == 1)
        {
            if (hitIndex == 1) animator.Play("EnemyTelegraph1_1");
            else animator.Play("EnemyTelegraph1_2");
        }
        else
        {
            if (hitIndex == 1) animator.Play("EnemyTelegraph2_1");
            else animator.Play("EnemyTelegraph2_2");
        }
    }

    public void PlayAttack(int patternId, int hitIndex)
    {
        if (patternId == 1)
        {
            if (hitIndex == 1) animator.Play("EnemyAttack1_1");
            else animator.Play("EnemyAttack1_2");
        }
        else
        {
            if (hitIndex == 1) animator.Play("EnemyAttack2_1");
            else animator.Play("EnemyAttack2_2");
        }
    }

    public void ShowHideWarningText()
    {
        warningText.SetActive(!warningText.activeSelf);
    }

    public void SpawnFloatingText(string text)
    {
        floatingTextSpawner.SpawnText(text);
    }

    // =========================
    // Stance animations
    // =========================
    public IEnumerator PlayStanceChangeAnim(Stance newStance)
    {
        string stateName = null;

        switch (newStance)
        {
            case Stance.Stone:
                stateName = "StoneStance";
                break;
            case Stance.Wind:
                stateName = "WindStance";
                break;
            case Stance.Flame:
                stateName = "FlameStance";
                break;
        }

        animator.Play(stateName);

        yield return null;

        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).IsName(stateName));

        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
    }

    public void PlayIdleForCurrentStance()
    {
        switch (currentStance)
        {
            case Stance.Stone: animator.Play("StoneIdle"); break;
            case Stance.Wind: animator.Play("WindIdle"); break;
            case Stance.Flame: animator.Play("FlameIdle"); break;
            default: animator.Play("IdleNoStance"); break;
        }
    }

    public void PlayParried()
    {
        animator.Play("Parry", 0, 0f);
        SpawnFloatingText("Parried");

        AudioManager.Instance.PlaySFX("Parry");
        CameraController.Instance.Shake(0.1f, 0.2f);
    }

    public void PlayHit()
    {
        animator.Play("Hit", 0, 0f);
        SpawnFloatingText("Hit!");

        AudioManager.Instance.PlaySFX("Hit");
        CameraController.Instance.Shake(0.1f, 0.2f);
    }

    public void PlayCountered()
    {
        animator.Play("Counter");
        SpawnFloatingText("Counter!");

        AudioManager.Instance.PlaySFX("Counter");
        CameraController.Instance.Shake(0.1f, 0.2f);
    }

    public void PlayPierced()
    {
        animator.Play("Hit", 0, 0f);
        SpawnFloatingText("Pierced!");

        AudioManager.Instance.PlaySFX("Pierced");
        CameraController.Instance.Shake(0.1f, 0.2f);
    }

    public IEnumerator PlaySpecialEffect(Stance playerStance)
    {
        effectAnimator.gameObject.SetActive(true);
        switch (playerStance)
        {
            case Stance.Stone:
                effectAnimator.Play("StoneEffect");
                break;
            case Stance.Wind:
                effectAnimator.Play("WindEffect");
                break;
            case Stance.Flame:
                effectAnimator.Play("FlameEffect");
                break;
        }

        yield return new WaitForSeconds(1f);
        effectAnimator.gameObject.SetActive(false);
    }

    public void PlayDeath()
    {
        animator.Play("Death");
    }

    // =========================
    // Damage
    // =========================
    public int CalculateEnemyAttackDamage()
    {
        float mult = currentStance switch
        {
            Stance.Flame => 1.3f,
            Stance.Stone => 0.7f,
            Stance.Wind => 1.0f,
            _ => 1f
        };

        return Mathf.RoundToInt(baseAttack * mult);
    }

    public void TakeDamage(int dmg)
    {
        currentHP = Mathf.Max(currentHP - dmg, 0);
    }

    // =========================
    // Stance change helper
    // =========================
    public bool TryChangeStance()
    {
        float r = Random.value;
        if (r <= 0.05f)
        {
            Stance newStance;
            do
            {
                newStance = (Stance)Random.Range(1, 4);
            }
            while (newStance == currentStance);

            currentStance = newStance;
            return true;
        }

        return false;
    }
}