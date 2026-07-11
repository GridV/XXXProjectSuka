using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Менеджер заданий — хранит все задания и текущую сессию.
/// </summary>
public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }

    private BaseTaskController activeController;

    [Header("Session queue")]
    public Queue<TaskData> sessionQueue = new Queue<TaskData>();

    [Header("UI")]
    [SerializeField] private NeonEngagementBar engagementBar;


    [SerializeField] private DialogueUIController dialogueUI;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private RhythmController rhythmManager;
    public DialogueUIController DialogueUI => dialogueUI;
    public DialogueManager DialogueManager => dialogueManager;
    public RhythmController RhythmManager => rhythmManager;

    protected TaskContext ctx;
    private string activeTaskId;
    private string activeDialogueFile;
    private List<TaskData> allTasks = new List<TaskData>();
    private TaskRunner taskRunner;
    private TaskPhase currentPhase;

    public void RegisterTaskRunner(TaskRunner runner)
    {
        taskRunner = runner;
    }

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        TaskData[] loaded = Resources.LoadAll<TaskData>("Tasks");
        allTasks = new List<TaskData>(loaded);
        GenerateInitialSession();
    }

    private void Start()
    {
        ctx = GameContext.Instance.TaskContext;
        if(ctx.sessionStats == null)
        {
            ctx.sessionStats = FindObjectOfType<SessionStatsService>();
        }      
        Debug.Log("[TaskManager] Start() called");

        // Обновляем UI вовлечённости (если назначен)
        if (engagementBar != null)
        {    
            engagementBar.UpdateBar(ctx.sessionStats.engagement);
            Debug.Log("[TaskManager] Engagement bar initialized.");
        }
        else
        {
            Debug.LogWarning("[TaskManager] engagementBar is NULL!");
        }

        // Запускаем выполнение первого задания, если есть
        var taskRunner = FindObjectOfType<TaskRunner>();
    }

    /// <summary> Создаём начальную сессию из 2–3 заданий </summary>
    private void GenerateInitialSession()
    {
        sessionQueue.Clear();

        // Always start the session with InitialTask
        var init = allTasks.Find(t => t != null && t.id == "InitialTask");
        if (init == null)
        {
            Debug.LogWarning("[TaskManager] InitialTask not found in allTasks!");
        }
        else
        {
            sessionQueue.Enqueue(init);
            Debug.Log("[TaskManager] InitialTask added as first task in session.");
        }

        List<TaskData> available = GetAvailableTasks();

        // If no tasks are available, add a fallback
        if (available.Count == 0)
        {
            Debug.LogWarning("[TaskManager] No available tasks found. Adding fallback task.");

            var fallback = allTasks.Find(t => t != null && t.id == "Task_01");
            if (fallback != null)
            {
                sessionQueue.Enqueue(fallback);
                Debug.Log("[TaskManager] Fallback Task_01 added to session.");
            }
        }
        else
        {
            int count = Mathf.Min(3, available.Count);
            for (int i = 0; i < count; i++)
            {
                var picked = PickRandomTask(available);
                if (picked == null) continue;

                // Avoid accidentally enqueueing InitialTask again
                if (picked.id == "InitialTask") continue;

                sessionQueue.Enqueue(picked);
            }
        }

        Debug.Log($"[TaskManager] Session queue initialized with {sessionQueue.Count} tasks.");
    }



    /// <summary> Добавить следующее задание в очередь </summary>
    public void AddNextTask()
    {
        var available = GetAvailableTasks();
        if (available.Count > 0)
        {
            sessionQueue.Enqueue(PickRandomTask(available));
        }
    }



    public void StartNextTask()
    {
        if (taskRunner == null)
        {
            Debug.LogWarning("[TaskManager] TaskRunner not registered.");
            return;
        }

        taskRunner.StartNextTask();
    }

    /// <summary> Получить следующее задание и удалить его из очереди </summary>
    public TaskData GetNextTask()
    {
        if (sessionQueue.Count == 0)
            AddNextTask();

        return sessionQueue.Count > 0 ? sessionQueue.Dequeue() : null;
    }

    /// <summary> Вернуть список заданий, которые можно сейчас использовать </summary>
    List<TaskData> GetAvailableTasks()
    {
        /* for tesing*/
        List<TaskData> result = new List<TaskData>();

        foreach (var t in allTasks)
        {
            if (t.id == "FinalTask")
            {
                result.Add(t);
                break; // only one task for test
            }
        }

        return result;
        /*Actual
         
        List<TaskData> result = new List<TaskData>();
        foreach (var t in allTasks)
        {
            if (t.id == "InitialTask") continue;
            if (t.IsAvailable(engagement) && t.GetWeight(currentPhase) > 0f)
                result.Add(t);
        }
        return result;*/
    }

    /// <summary> Выбрать случайное задание с учётом весов для текущей фазы </summary>
    TaskData PickRandomTask(List<TaskData> list)
    {
        float total = 0f;
        foreach (var t in list)
            total += t.GetWeight(currentPhase);

        float rnd = Random.Range(0, total);
        foreach (var t in list)
        {
            rnd -= t.GetWeight(currentPhase);
            if (rnd <= 0f)
                return t;
        }
        return list[0]; // fallback
    }

    /// <summary> Смена фазы вручную или по логике </summary>
    public void SetPhase(TaskPhase phase)
    {
        currentPhase = phase;
        Debug.Log($"[TaskManager] Phase set to {phase}");
    }


    /// <summary>
    /// Когда сцена загружается — проверяем, не передал ли TaskRunner данные.
    /// </summary>
    private void OnEnable()
    {
        // Если PlayerPrefs содержит задание, подхватываем его
        activeTaskId = PlayerPrefs.GetString("ActiveTaskId", "");
        activeDialogueFile = PlayerPrefs.GetString("ActiveDialogue", "");
    }

    /// <summary>
    /// Когда ритм или задание завершено.
    /// </summary>
    public void OnTaskCompleted()
    {
        int reward = PlayerPrefs.GetInt("EngagementReward", 0);
        if (reward != 0)
            ChangeEngagement(reward);

        Debug.Log($"[TaskManager] Task completed. Reward: {reward}");
    }


    /// <summary> Изменить вовлечение </summary>
    public void ChangeEngagement(float delta)
    {             
        ctx.sessionStats.engagement = Mathf.Clamp(ctx.sessionStats.engagement + delta, 0, 100);
        Debug.Log($"[TaskManager] Engagement = {ctx.sessionStats.engagement}");

        if (engagementBar != null)
        {
            Debug.Log("[TaskManager] Calling UpdateBar()");
            engagementBar.UpdateBar(ctx.sessionStats.engagement);
        }
        else
        {
            Debug.LogWarning("[TaskManager] engagementBar is NULL!");
        }
    }
 
}
