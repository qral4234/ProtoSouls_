using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    // --- BİLEŞEN REFERANSLARI ---
    InputHandler inputHandler;       // Klavye/Mouse girdilerini okuyan script
    AnimatorHandler animatorHandler; // Animasyonları yöneten script
    PlayerLocomotion playerLocomotion; // Hareketi yöneten script
    PlayerStats playerStats;         // Can, Stamina gibi verileri tutan script
    PlayerAttacker playerAttacker;   // Saldırı mantığını yöneten script

    [Header("Oyuncu Durumları")]
    [Tooltip("Karakter şu an bir animasyon etkileşiminde mi? (Saldırı, Yuvarlanma, Hasar Alma vb.)")]
    public bool isInteracting;
    [Tooltip("Karakter blok modunda mı? (Sağ tık basılı mı?)")]
    public bool isBlocking;
    [Tooltip("Karakter öldü mü?")]
    public bool isDead;

    [Header("Ekipman")]
    [Tooltip("Şu an karakterin elindeki silahın verisi.")]
    public WeaponItem currentWeapon; 

    // Unity Awake: Script ilk yüklendiğinde çalışır. Referansları burada topluyoruz.
    void Awake()
    {
        inputHandler = GetComponent<InputHandler>();
        playerLocomotion = GetComponent<PlayerLocomotion>();
        animatorHandler = GetComponent<AnimatorHandler>();
        playerStats = GetComponent<PlayerStats>();
        playerAttacker = GetComponent<PlayerAttacker>();
    }

    // Unity Update: Her karede (frame) çalışır. Oyun mantığı burada döner.
    void Update()
    {
        // 1. Ölüm kontrolü: Eğer ölüsek hiçbir şey yapma.
        if (isDead)
            return;

        // Kare süresi (Delta Time) hesaplaması
        float delta = Time.deltaTime;

        // 2. Animasyon Durumu: Şu an özel bir animasyon (Roll, Attack) oynuyor mu?
        isInteracting = animatorHandler.anim.GetBool(AnimatorHandler.isInteractingHash);

        // 3. Girdi Okuma: InputHandler'ı çalıştır ve tuşları dinle
        inputHandler.TickInput(delta);

        // 4. Bloklama Mantığı
        // Eğer blok tuşuna basılıyorsa VE karakter başka bir işle (yuvarlanma/saldırı) meşgul değilse blok yap.
        if (inputHandler.blockingInput && !isInteracting) 
        {
            isBlocking = true;
        }
        else
        {
            isBlocking = false;
        }
        // Animator'a blok durumunu bildir (Kalkanı kaldırması için)
        animatorHandler.anim.SetBool(AnimatorHandler.isBlockingHash, isBlocking);

        // 5. Stamina Yenilenme Hızı
        // Hareket halindeyken (koşmuyorsa) stamina daha yavaş dolsun.
        if (inputHandler.moveAmount > 0 && !inputHandler.sprintFlag)
        {
            playerStats.SetRegenMultiplier(2.0f); // Örnek: Yürürken stamina 2 kat hızlı dolsun (Tasarım tercihi)
        }
        else
        {
            playerStats.SetRegenMultiplier(1.0f); // Dururken normal hız
        }

        // 6. Koşma (Sprint) Kontrolü
        if (inputHandler.sprintFlag)
        {
            // Eğer bir animasyonun ortasındaysak (örn: saldırı), koşamayız.
            if (isInteracting)
                return;

            // Stamina varsa koş, yoksa koşmayı bırak
            if (playerStats.currentStamina > 0)
            {
                playerStats.TakeStaminaDamage(10 * delta); // Saniyede 10 stamina harca
            }
            else
            {
                inputHandler.sprintFlag = false;
            }
        }

        // 7. Yuvarlanma (Roll) Kontrolü
        if (inputHandler.rollFlag)
        {
            // Animasyon ortasındaysak dönemeyiz
            if (isInteracting)
                return;

            // Stamina yetiyorsa yuvarlan
            if (playerStats.currentStamina > 0)
            {
                playerStats.TakeStaminaDamage(15);
            }
            else
            {
                // Yetmiyorsa komutu iptal et
                inputHandler.rollFlag = false;
            }
        }

        // Yuvarlanma ve Koşma fiziklerini uygula
        playerLocomotion.HandleRollingAndSprinting(delta);

        // 8. İyileşme (Heal) Kontrolü (Q Tuşu)
        if (inputHandler.heal_Input)
        {
            inputHandler.heal_Input = false; // Tek seferlik tetiklensin
            playerStats.HealPlayer();
        }

        // 9. Saldırı Kontrolü (Sol Tık)
        if (inputHandler.rb_Input)
        {
            // Stamina varsa saldırabilir
            if (playerStats.currentStamina > 0)
            {
                // Kombo yapılabilir bir andaysak (önceki saldırının bitişi), kombo zincirini devam ettir
                if (animatorHandler.canDoCombo)
                {
                    playerStats.TakeStaminaDamage(10);
                    playerAttacker.HandleCombo(currentWeapon);
                }
                // Boştaysak normal ilk saldırıyı yap
                else if (!isInteracting)
                {
                    if (currentWeapon != null)
                    {
                        playerStats.TakeStaminaDamage(10);
                        playerAttacker.HandleLightAttack(currentWeapon);
                    }
                }
            }

            inputHandler.rb_Input = false; // Girdiyi temizle
        }

        // 10. Animasyon Güncelleme
        // Hareket değerlerini Animator'a gönder (Yürüme/Koşma animasyonları için)
        float moveAmount = inputHandler.moveAmount;
        if (playerLocomotion.isSprinting)
        {
            moveAmount = 2; // 2 değeri Animator'da "Sprint" blend tree'sine denk gelir
        }

        bool isLockedOn = CameraHandler.singleton.currentLockOnTarget != null;
        animatorHandler.UpdateAnimatorValues(moveAmount, inputHandler.horizontal, inputHandler.vertical, isLockedOn);
    }

    // Unity FixedUpdate: Fizik hesaplamaları için sabit aralıklarla çalışır.
    void FixedUpdate()
    {
        float delta = Time.fixedDeltaTime;

        if (playerLocomotion != null)
        {
            // Karakterin fiziksel hareketlerini (Move, Fall, Rotate) işle
            playerLocomotion.HandleAllMovement(delta);
        }
    }

    // Unity LateUpdate: Her şey bittikten sonra (Kamera takibi için ideal) çalışır.
    void LateUpdate()
    {
        float delta = Time.deltaTime;

        if (CameraHandler.singleton != null)
        {
            // Kamera işlemlerini sırasıyla yap
            CameraHandler.singleton.HandleLockOn();           // Hedef kilitlenme
            CameraHandler.singleton.FollowTarget(delta);      // Takip et
            CameraHandler.singleton.HandleCameraRotation(delta, inputHandler.mouseX, inputHandler.mouseY); // Dön
            CameraHandler.singleton.HandleCameraCollisions(delta); // Duvara çarpma
        }
    }
}