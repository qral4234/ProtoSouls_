using UnityEngine;

public class PlayerAttacker : MonoBehaviour
{
    AnimatorHandler animatorHandler;
    DamageCollider damageCollider;

    public void Awake()
    {
        animatorHandler = GetComponentInChildren<AnimatorHandler>();
        damageCollider = GetComponentInChildren<DamageCollider>();
    }

    public void OpenDamageCollider()
    {
        if (damageCollider != null)
        {
            damageCollider.EnableDamageCollider();
        }
    }

    public void CloseDamageCollider()
    {
        if (damageCollider != null)
        {
            damageCollider.DisableDamageCollider();
        }
    }

    public void HandleLightAttack(WeaponItem weapon)
    {
        animatorHandler.PlayTargetAnimation(weapon.OH_Light_Attack_1, true);
    }

    public void HandleCombo(WeaponItem weapon)
    {
        if (animatorHandler.canDoCombo)
        {
            animatorHandler.canDoCombo = false;
            if (animatorHandler.anim.GetCurrentAnimatorStateInfo(0).IsName(weapon.OH_Light_Attack_1))
            {
                animatorHandler.PlayTargetAnimation(weapon.OH_Light_Attack_2, true);
            }
        }
    }
}
