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
    [SerializeField] private float smgFireRate = 0.05f;
    [SerializeField] private int shotgunPelletCount = 5;
    [SerializeField] private float spreadAngle = 30f;
    [SerializeField] private float smgSpreadAngle = 10f;
    [SerializeField] private float smgRecoil = 0.5f;
    
    private Vector2 moveInput;
    private Vector2 mousePosition;
    private Rigidbody2D rb;
    private InputSystem_Actions inputActions;
    private Camera mainCamera;
    private bool canDash = true;
    private bool canFire = true;
    private float currentRecoil = 0f;
    private bool isFireButtonHeld = false;
    private Coroutine autoFireCoroutine;
    
    private enum WeaponMode { Normal, Shotgun, SMG }
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
        inputActions.Player.Attack.started += OnFireStarted;
        inputActions.Player.Attack.canceled += OnFireCanceled;
        inputActions.Player.Dash.performed += OnDash;
        inputActions.Player.WeaponSwitch.performed += OnWeaponSwitch;
    }

    private void OnDisable()
    {
        inputActions.Disable();
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;
        inputActions.Player.Look.performed -= OnLook;
        inputActions.Player.Attack.started -= OnFireStarted;
        inputActions.Player.Attack.canceled -= OnFireCanceled;
        inputActions.Player.Dash.performed -= OnDash;
        inputActions.Player.WeaponSwitch.performed -= OnWeaponSwitch;
        StopAutoFire();
        CancelInvoke(nameof(ReduceRecoil));
    }

    private void OnFireStarted(InputAction.CallbackContext context)
    {
        isFireButtonHeld = true;
        autoFireCoroutine = StartCoroutine(AutoFire());
    }

    private void OnFireCanceled(InputAction.CallbackContext context)
    {
        isFireButtonHeld = false;
        StopAutoFire();
    }

    private System.Collections.IEnumerator AutoFire()
    {
        while (isFireButtonHeld)
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
                    case WeaponMode.SMG:
                        FireSMG();
                        break;
                }
            }
            yield return new WaitForSeconds(0.01f);
        }
    }

    private void StopAutoFire()
    {
        if (autoFireCoroutine != null)
        {
            StopCoroutine(autoFireCoroutine);
            autoFireCoroutine = null;
        }
    }

    private void OnWeaponSwitch(InputAction.CallbackContext context)
    {
        currentWeaponMode = currentWeaponMode switch
        {
            WeaponMode.Normal => WeaponMode.Shotgun,
            WeaponMode.Shotgun => WeaponMode.SMG,
            WeaponMode.SMG => WeaponMode.Normal,
            _ => WeaponMode.Normal
        };
        
        currentRecoil = 0f;
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

    private void FireNormal()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        StartCoroutine(FireCooldown(normalFireRate));
    }

    private void FireShotgun()
    {
        float angleStep = spreadAngle / (shotgunPelletCount - 1);
        float startAngle = -spreadAngle / 2;

        for (int i = 0; i < shotgunPelletCount; i++)
        {
            float currentBulletAngle = startAngle + (angleStep * i);
            Quaternion rotation = Quaternion.Euler(0, 0, currentBulletAngle) * firePoint.rotation;
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, rotation);
        }

        StartCoroutine(FireCooldown(shotgunFireRate));
    }

    private void FireSMG()
    {
        currentRecoil = Mathf.Min(currentRecoil + smgRecoil, smgSpreadAngle);
        float randomSpread = Random.Range(-currentRecoil, currentRecoil);
        Quaternion rotation = Quaternion.Euler(0, 0, randomSpread) * firePoint.rotation;
        
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, rotation);
        
        StartCoroutine(FireCooldown(smgFireRate));
        
        if (!IsInvoking(nameof(ReduceRecoil)))
        {
            InvokeRepeating(nameof(ReduceRecoil), 0.1f, 0.1f);
        }
    }

    private void ReduceRecoil()
    {
        if (currentRecoil > 0)
        {
            currentRecoil = Mathf.Max(0, currentRecoil - 0.5f);
        }
        else
        {
            CancelInvoke(nameof(ReduceRecoil));
        }
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
        canDash = false;
        
        Vector2 dashDirection = (mousePosition - (Vector2)transform.position).normalized;
        
        if (dashTrail != null)
            dashTrail.emitting = true;
            
        Vector2 dashPosition = rb.position + (dashDirection * dashDistance);
        rb.MovePosition(dashPosition);
        
        yield return new WaitForSeconds(0.1f);
        
        if (dashTrail != null)
            dashTrail.emitting = false;
            
        yield return new WaitForSeconds(dashCooldown);
        
        canDash = true;
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