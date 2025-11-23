using System.Collections;
using UnityEngine;

public enum PlayerAttackType
{
    Basic,
    Enhance,
    StoneSpecial,
    WindSpecial,
    FlameSpecial
}

public class PlayerCombat : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private FloatingTextSpawner floatingTextSpawner;

    [Header("Stats")]
    public float baseAttack;
    public int maxHp;

    public int CurrentHp { get; private set; }
    public int Focus { get; set; } = 6;
    public Stance CurrentStance { get; private set; }

    public bool turnParrySuccess = true;    

    public bool IsParrying { get; private set; } = false;
    private bool canParry = true;

    private const float baseParryDuration = 0.2f;
    private const float parryCooldown = 0.3f;

    private void Awake()
    {
        CurrentHp = maxHp;
        CurrentStance = Stance.Flame;
    }

    private float GetParryDurationMultiplier()
    {
        return CurrentStance switch
        {
            Stance.Stone => 1.25f,
            Stance.Wind => 0.75f,
            Stance.Flame => 1f,
            _ => 1f
        };
    }

    public bool TryStartParry()
    {
        if (!canParry) return false;

        animator.Play("StartParry", 0, 0f);

        StartCoroutine(ParryRoutine());
        return true;
    }

    private IEnumerator ParryRoutine()
    {
        canParry = false;
        IsParrying = true;

        float duration = baseParryDuration * GetParryDurationMultiplier();
        yield return new WaitForSeconds(duration);

        IsParrying = false;
        PlayAnimationIdle();

        yield return new WaitForSeconds(parryCooldown);
        canParry = true;
    }

    public void ResetParry()
    {
        IsParrying = false;
        PlayAnimationIdle();
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

        AudioManager.Instance.PlaySFX("Counter");
        CameraController.Instance.Shake(0.1f, 0.2f);
    }

    public void PlaySuccessfulParry()
    {
        animator.Play("Parry", 0, 0f);
        SpawnFloatingText("Parried!");


        AudioManager.Instance.PlaySFX("Parry");
        CameraController.Instance.Shake(0.1f, 0.2f);
    }

    public void PlayDeath()
    {
        animator.Play("Death");
    }

 

    public void SpawnFloatingText(string text)
    {
        floatingTextSpawner.SpawnText(text);
    }

    // ==========================
    // ATTACK ANIMATIONS
    // ==========================
    public void PlayTelegraph(PlayerAttackType type, int hit)
    {
        switch (type)
        {
            case PlayerAttackType.Basic:
                animator.Play("BasicAttackTelegraph_1");
                break;

            case PlayerAttackType.Enhance:
                animator.Play(hit == 1 ? "EnhanceAttackTelegraph_1" : "EnhanceAttackTelegraph_2");
                break;

            case PlayerAttackType.StoneSpecial:
                animator.Play(hit == 1 ? "StoneSpecialTelegraph_1" : "StoneSpecialTelegraph_2");
                break;

            case PlayerAttackType.WindSpecial:
                animator.Play("WindSpecialTelegraph_1");
                break;

            case PlayerAttackType.FlameSpecial:
                animator.Play(hit switch
                {
                    1 => "FlameSpecialTelegraph_1",
                    2 => "FlameSpecialTelegraph_2",
                    3 => "FlameSpecialTelegraph_3",
                    4 => "FlameSpecialTelegraph_4",
                    _ => "FlameSpecialTelegraph_1"
                });
                break;
        }
    }

    public void PlayAttack(PlayerAttackType type, int hit)
    {
        switch (type)
        {
            case PlayerAttackType.Basic:
                animator.Play("BasicAttack_1");
                break;

            case PlayerAttackType.Enhance:
                animator.Play(hit == 1 ? "EnhanceAttack_1" : "EnhanceAttack_2");
                break;

            case PlayerAttackType.StoneSpecial:
                animator.Play(hit == 1 ? "StoneSpecial_1" : "StoneSpecial_2");
                break;

            case PlayerAttackType.WindSpecial:
                animator.Play("WindSpecial_1");
                break;

            case PlayerAttackType.FlameSpecial:
                animator.Play(hit switch
                {
                    1 => "FlameSpecial_1",
                    2 => "FlameSpecial_2",
                    3 => "FlameSpecial_3",
                    4 => "FlameSpecial_4",
                    _ => "FlameSpecial_1"
                });
                break;
        }
    }

    // ==========================
    // STANCE / VISUALS
    // ==========================
    public void ChangeStance(Stance newStance)
    {
        CurrentStance = newStance;
        Focus = 0;

        switch (newStance)
        {
            case Stance.Flame:
                animator.Play("FlameStance");
                break;

            case Stance.Wind:
                animator.Play("WindStance");
                break;

            case Stance.Stone:
                animator.Play("StoneStance");
                break;

            default:
                spriteRenderer.color = Color.white;
                break;
        }
    }

    public void PlayAnimationIdle()
    {
        if (IsParrying) return;

        switch (CurrentStance)
        {
            case Stance.Flame: animator.Play("FlameIdle"); break;
            case Stance.Wind: animator.Play("WindIdle"); break;
            case Stance.Stone: animator.Play("StoneIdle"); break;
            default: animator.Play("IdleNoStance"); break;
        }
    }

    // ==========================
    // DAMAGE CALCULATION
    // ==========================
    public float GetStanceDamageMultiplier()
    {
        return CurrentStance switch
        {
            Stance.Stone => 0.7f,
            Stance.Wind => 1.0f,
            Stance.Flame => 1.3f,
            _ => 1f
        };
    }

    public int CalculateAttackDamage()
    {
        float dmg = baseAttack * GetStanceDamageMultiplier();
        return Mathf.RoundToInt(dmg);
    }

    public int CalculateWindCounter(int enemyBaseAttack)
    {
        if (CurrentStance != Stance.Wind) return 0;

        return Mathf.RoundToInt(enemyBaseAttack * 0.5f);
    }

    // ==========================
    // FOCUS SYSTEM
    // ==========================
    public void AddFocus()
    {
        Focus = Mathf.Min(Focus + 1, 6);
    }

    public void ResetFocus()
    {
        Focus = 0;
    }

    // ==========================
    // HEALTH
    // ==========================
    public void TakeDamage(int damage)
    {
        CurrentHp = Mathf.Max(CurrentHp - damage, 0);
    }
}