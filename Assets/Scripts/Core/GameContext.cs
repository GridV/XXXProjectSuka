using UnityEngine;

 
public class GameContext : MonoBehaviour
{
    public static GameContext Instance { get; private set; }

    public TaskContext TaskContext = new TaskContext();
    public GameDecisionService Decisions { get; private set; }

    public void InitializeDecisionsIfNeeded()
    {
        if (Decisions != null)
            return;

        int seed = System.Environment.TickCount;
        Decisions = new GameDecisionService(new SystemRng(seed));
        Debug.Log($"[GameContext] Decisions initialized. Seed={seed}");
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeDecisionsIfNeeded();
        TaskContext.decisions = Decisions;
    }

    public void ResetTaskContext()
    {
        TaskContext = new TaskContext();
        TaskContext.decisions = Decisions;
        Debug.Log("[GameContext] Task context reset.");
    }
}

 
[System.Serializable]
public class TaskContext
{
    public TaskData currentTask;
    public DialogueActionAdapter dialogueActionAdapter;
    public BaseTaskController controller;
    public DialogueUIController dialogueUI;
    public DialogueManager dialogueManager;
    public RhythmController rhythmManager;
    public Animator animator;
    public SessionStatsService sessionStats;
    public TaskManager taskManager;
    //helpers
    public CharacterAnimationService characterAnimation;
    public CameraMoveService cameraMoveService;
    public CinemachineDanceSwitcher danceCameraService;
    // Services
    public GameDecisionService decisions;
    public CharacterRigService rigService;
    public FacialExpressionController facial;
}
