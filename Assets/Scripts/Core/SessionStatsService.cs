using UnityEngine;

public class SessionStatsService : MonoBehaviour
{
    public static SessionStatsService Instance { get; private set; }

    public float SessionDurationSeconds { get; private set; }
    public int PausePressedCount { get; private set; }

    [Header("Game state")]
    [Range(0, 100)]
    public float engagement = 0;
    public TaskPhase currentPhase = TaskPhase.Early;

    private bool isSessionRunning;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (!isSessionRunning)
            return;

        SessionDurationSeconds += Time.deltaTime;
    }

    // ===== Session control =====

    public void SessionStart()
    {
        SessionDurationSeconds = 0f;
        PausePressedCount = 0;
        isSessionRunning = true;
    }

    public void SessionPause()
    {
        isSessionRunning = false;
    }

    public void SessionResume()
    {
        isSessionRunning = true;
    }

    public void SessionEnd()
    {
        isSessionRunning = false;
    }

    // ===== Counters =====

    public void RegisterPausePressed()
    {
        PausePressedCount++;
    }
}
