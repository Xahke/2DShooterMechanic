using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
[SerializeField] private float moveSpeed = 5f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    
    [Header("Dash Settings")]
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private TrailRenderer dashTrail;
    
    [Header("Weapon Settings")]
    [SerializeField] private float normalFireRate = 0.2f;
    [SerializeField] private float shotgunFireRate = 0.8f;
    [SerializeField] private int shotgunPelletCount = 5;        // Tek atışta kaç mermi
    [SerializeField] private float spreadAngle = 30f;           // Yayılma açısı
    
    private Vector2 moveInput;
    private Vector2 mousePosition;
    private Rigidbody2D rb;
    private InputSystem_Actions inputActions;
    private Camera mainCamera;
    private bool canDash = true;
    private bool canFire = true;
    
    private enum WeaponMode { Normal, Shotgun }
    private WeaponMode currentWeaponMode = WeaponMode.Normal;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Look.performed += OnLook;
        inputActions.Player.Attack.performed += OnFire;
        inputActions.Player.Dash.performed += OnDash;
        inputActions.Player.WeaponSwitch.performed += OnWeaponSwitch; // X tuşu için
    }

    private void OnDisable()
    {
        inputActions.Disable();
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;
        inputActions.Player.Look.performed -= OnLook;
        inputActions.Player.Attack.performed -= OnFire;
        inputActions.Player.Dash.performed -= OnDash;
        inputActions.Player.WeaponSwitch.performed -= OnWeaponSwitch;
    }

    private void OnWeaponSwitch(InputAction.CallbackContext context)
    {
        // Silah modunu değiştir
        currentWeaponMode = currentWeaponMode == WeaponMode.Normal ? 
                           WeaponMode.Shotgun : WeaponMode.Normal;
        
        // Mod değişikliğini görsel olarak belirt (opsiyonel)
        Debug.Log($"Weapon Mode: {currentWeaponMode}");
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        mousePosition = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }

     private void OnFire(InputAction.CallbackContext context)
    {
        if (canFire)
        {
            switch (currentWeaponMode)
            {
                case WeaponMode.Normal:
                    FireNormal();
                    break;
                case WeaponMode.Shotgun:
                    FireShotgun();
                    break;
            }
        }
    }
    private void FireNormal()
    {
        // Normal tek mermi ateşleme
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        StartCoroutine(FireCooldown(normalFireRate));
    }

    private void FireShotgun()
    {
        // Shotgun tarzı çoklu mermi ateşleme
        float angleStep = spreadAngle / (shotgunPelletCount - 1);
        float startAngle = -spreadAngle / 2;

        for (int i = 0; i < shotgunPelletCount; i++)
        {
            // Her mermi için açı hesapla
            float currentBulletAngle = startAngle + (angleStep * i);
            Quaternion rotation = Quaternion.Euler(0, 0, currentBulletAngle) * firePoint.rotation;
            
            // Mermiyi oluştur
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, rotation);
        }

        StartCoroutine(FireCooldown(shotgunFireRate));
    }
     private System.Collections.IEnumerator FireCooldown(float cooldown)
    {
        canFire = false;
        yield return new WaitForSeconds(cooldown);
        canFire = true;
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        if (canDash)
        {
            StartCoroutine(PerformDash());
        }
    }

    private System.Collections.IEnumerator PerformDash()
    {
        canDash = false; // Dash'i devre dışı bırak
        
        // Dash öncesi hızı kaydet
        Vector2 originalVelocity = rb.linearVelocity;
        
        // Dash yönünü hesapla (karakterin baktığı yön)
        Vector2 dashDirection = (mousePosition - (Vector2)transform.position).normalized;
        
        // Trail efektini aktifleştir (eğer varsa)
        if (dashTrail != null)
            dashTrail.emitting = true;
            
        // Dash pozisyonunu hesapla ve uygula
        Vector2 dashPosition = rb.position + (dashDirection * dashDistance);
        rb.MovePosition(dashPosition);
        
        // Kısa bir bekleme
        yield return new WaitForSeconds(0.1f);
        
        // Trail efektini kapat
        if (dashTrail != null)
            dashTrail.emitting = false;
            
        // Cooldown süresi kadar bekle
        yield return new WaitForSeconds(dashCooldown);
        
        canDash = true; // Dash'i tekrar aktif et
    }

    private void Update()
    {
        Vector2 direction = mousePosition - (Vector2)transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }
}