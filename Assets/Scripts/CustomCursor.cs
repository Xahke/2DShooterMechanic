using UnityEngine;
using System.Collections;

public class CustomCursor : MonoBehaviour
{
    private Camera mainCamera;
    private Vector3 mousePosition;
    private SpriteRenderer spriteRenderer;
    
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private Color criticalFlashColor = Color.white;
    [SerializeField] private Color hoverColor = Color.red;
    [SerializeField] private float screenPadding = 0.5f;
    private Color originalColor;
    private bool isFlashing = false;
    private float minX, maxX, minY, maxY;

    private void Awake()
    {
        mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    private void Start()
    {
        UnityEngine.Cursor.visible = false;
        CalculateScreenBounds();
        GameEvents.OnEnemyHit += FlashCursor;
        GameEvents.OnCriticalHit += CriticalFlashCursor;
    }

    private void OnDestroy()
    {
        GameEvents.OnEnemyHit -= FlashCursor;
        GameEvents.OnCriticalHit -= CriticalFlashCursor;
    }

    private void CalculateScreenBounds()
    {
        Vector2 screenBounds = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        Vector2 screenOrigin = mainCamera.ScreenToWorldPoint(Vector3.zero);

        minX = screenOrigin.x + screenPadding;
        maxX = screenBounds.x - screenPadding;
        minY = screenOrigin.y + screenPadding;
        maxY = screenBounds.y - screenPadding;
    }

    private void Update()
    {
        mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        
        float clampedX = Mathf.Clamp(mousePosition.x, minX, maxX);
        float clampedY = Mathf.Clamp(mousePosition.y, minY, maxY);
        
        transform.position = new Vector3(clampedX, clampedY, 0);

        RaycastHit2D hit = Physics2D.Raycast(new Vector2(clampedX, clampedY), Vector2.zero);
        if (hit.collider != null && hit.collider.CompareTag("Enemy"))
        {
            if (!isFlashing)
            {
                spriteRenderer.color = hoverColor;
            }
        }
        else if (!isFlashing)
        {
            spriteRenderer.color = originalColor;
        }
    }

    private void FlashCursor()
    {
        if (!isFlashing)
        {
            StartCoroutine(FlashRoutine());
        }
    }

    private void CriticalFlashCursor()
    {
        if (!isFlashing)
        {
            StartCoroutine(CriticalFlashRoutine());
        }
    }

    private IEnumerator FlashRoutine()
    {
        isFlashing = true;
        spriteRenderer.color = flashColor;
        
        yield return new WaitForSeconds(flashDuration);
        
        spriteRenderer.color = originalColor;
        isFlashing = false;
    }

    private IEnumerator CriticalFlashRoutine()
    {
        isFlashing = true;
        
        spriteRenderer.color = criticalFlashColor;
        yield return new WaitForSeconds(flashDuration / 2);
        
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        
        spriteRenderer.color = originalColor;
        isFlashing = false;
    }
}