using UnityEngine;

[CreateAssetMenu(menuName = "Items/Weapon Item")]
public class WeaponItem : Item
{
    [Header("Silah Bilgileri")]
    public GameObject modelPrefab; // Silahın 3D modeli
    public bool isUnarmed; // Silahsız mı? (Yumruk vb.)

    [Header("Tek El Saldırı Animasyonları")]
    public string OH_Light_Attack_1;
    public string OH_Light_Attack_2;
    public string OH_Light_Attack_3;

    [Header("Ses Efektleri")]
    public AudioClip hitSound1; // Vuruş sesi 1
    public AudioClip hitSound2; // Vuruş sesi 2
    public AudioClip hitSound3; // Vuruş sesi 3
}