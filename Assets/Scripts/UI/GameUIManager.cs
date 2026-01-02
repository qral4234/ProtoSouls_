using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager instance;

    [Header("UI Panelleri")]
    public GameObject losePanel; // Kaybetme Ekranı
    public GameObject winPanel;  // Kazanma Ekranı
    public GameObject pauseMenuPanel; // Pause Menüsü
    public AudioSource backgroundMusic; // Arka plan müziği
    private bool isGamePaused = false;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // Başlangıçta tüm panelleri gizle
        if (losePanel != null) losePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        
        Time.timeScale = 1f; // Zamanın aktığından emin ol
    }

    private void Update()
    {
        // ESC tuşuna basınca Pause menüsünü aç/kapat
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Oyun zaten bittiyse pause açılamaz
            if ((losePanel != null && losePanel.activeSelf) || (winPanel != null && winPanel.activeSelf))
                return;

            TogglePauseGame();
        }
    }

    /// <summary>
    /// Oyunu dondurur veya devam ettirir.
    /// </summary>
    public void TogglePauseGame()
    {
        isGamePaused = !isGamePaused;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(isGamePaused);

        if (isGamePaused)
        {
            Time.timeScale = 0f; // Zamanı DURDUR
            UnlockCursor(); // Mouse'u serbest bırak
            if(backgroundMusic != null) backgroundMusic.volume *= 0.5f; // Müziği kıs
        }
        else
        {
            Time.timeScale = 1f; // Zamanı DEVAM ETTİR
            LockCursor(); // Mouse'u kilitle
            if(backgroundMusic != null) backgroundMusic.volume *= 2f; // Müziği aç
        }
    }

    // --- TETİKLEYİCİLER (Oyun sonu) ---

    // Kaybetme ekranını gecikmeli (2sn) açar
    // Kaybetme ekranını gecikmeli açar (Realtime kullanır)
    public void TriggerLoseGame()
    {
        StartCoroutine(ShowLosePanelRoutine());
    }

    // Kazanma ekranını gecikmeli açar (Realtime kullanır)
    public void TriggerWinGame()
    {
        StartCoroutine(ShowWinPanelRoutine());
    }

    private System.Collections.IEnumerator ShowLosePanelRoutine()
    {
        yield return new WaitForSecondsRealtime(2f);
        ShowLosePanel();
    }

    private System.Collections.IEnumerator ShowWinPanelRoutine()
    {
        // Ölüm anında Slow Motion varsa bile 3 saniye gerçek zaman bekle
        yield return new WaitForSecondsRealtime(3f);
        ShowWinPanel();
    }

    private void ShowLosePanel()
    {
        if (losePanel != null)
        {
            Time.timeScale = 1f; // Zamanın durmadığından emin ol (HitStop vs. varsa düzelt)
            losePanel.SetActive(true);
            if(backgroundMusic != null) backgroundMusic.Stop(); // Müziği kapat
            UnlockCursor();
        }
    }

    private void ShowWinPanel()
    {
        if (winPanel != null)
        {
            Time.timeScale = 1f; // Zamanın durmadığından emin ol
            winPanel.SetActive(true);
            if(backgroundMusic != null) backgroundMusic.Stop(); 
            UnlockCursor();
        }
    }

    // Mouse imlecini görünür yapar ve serbest bırakır (Menüler için)
    private void UnlockCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // Mouse imlecini gizler ve ortaya kilitler (Oyun içi)
    private void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // --- BUTON FONKSİYONLARI ---

    // Oyuna dön butonu
    public void ResumeGame()
    {
        TogglePauseGame();
    }

    // Tekrar dene butonu (Sahneyi yeniden yükler)
    public void TryAgain()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Menüye dön butonu
    public void ReturnToMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("MenuScene");
    }

    // --- SES AYARLARI (BUTONLAR İÇİN) ---

    // Müzik Aç/Kapat Butonu
    public void ToggleMusic()
    {
        if (backgroundMusic != null)
        {
            backgroundMusic.mute = !backgroundMusic.mute;
            
            if(backgroundMusic.mute)
            {
                // Müzik kapandı
            }
            else
            {
                // Müzik açıldı, ses çok kısıksa biraz aç
                if(backgroundMusic.volume <= 0.05f) backgroundMusic.volume = 0.5f;
            }
        }
    }

    // Genel Ses (SFX) Aç/Kapat Butonu
    public void ToggleSound()
    {
        bool isSoundOn = AudioListener.volume > 0.1f;
        
        if (isSoundOn)
        {
            AudioListener.volume = 0f; // Tüm sesleri kapat (Master Volume)
        }
        else
        {
            AudioListener.volume = 1f; // Tüm sesleri aç
        }
    }
}
