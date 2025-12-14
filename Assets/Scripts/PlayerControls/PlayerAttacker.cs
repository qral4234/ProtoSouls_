using UnityEngine;

public class PlayerAttacker : MonoBehaviour
{
    AnimatorHandler animatorHandler;
    DamageCollider damageCollider;
    public string lastAttack;

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
        PlayerRageManager rageManager = GetComponent<PlayerRageManager>();
        if (rageManager != null) rageManager.DrainRage(rageManager.attackDrainAmount);

        animatorHandler.PlayTargetAnimation(weapon.OH_Light_Attack_1, true);
        lastAttack = weapon.OH_Light_Attack_1;
    }

    public void HandleCombo(WeaponItem weapon)
    {
        if (animatorHandler.canDoCombo)
        {
            animatorHandler.canDoCombo = false;
            
            PlayerRageManager rageManager = GetComponent<PlayerRageManager>();
            if (rageManager != null) rageManager.DrainRage(rageManager.attackDrainAmount);
            if (lastAttack == weapon.OH_Light_Attack_1)
            {
                animatorHandler.PlayTargetAnimation(weapon.OH_Light_Attack_2, true);
                lastAttack = weapon.OH_Light_Attack_2;
            }
            else if (lastAttack == weapon.OH_Light_Attack_2)
            {
                animatorHandler.PlayTargetAnimation(weapon.OH_Light_Attack_3, true);
                lastAttack = weapon.OH_Light_Attack_3;
            }
        }
    }
}
