public static class GameEvents
{
    public delegate void EnemyHitAction();
    public static event EnemyHitAction OnEnemyHit;
    
    public delegate void CriticalHitAction();  // Yeni event
    public static event CriticalHitAction OnCriticalHit;  // Critical hit için

    public static void TriggerEnemyHit()
    {
        OnEnemyHit?.Invoke();
    }

    public static void TriggerCriticalHit()  // Yeni event trigger
    {
        OnCriticalHit?.Invoke();
    }
}