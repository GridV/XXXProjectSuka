using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Менеджер заданий — хранит все задания и текущую сессию.
/// </summary>
public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }

    [Header("Game state")]
    [Range(0, 100)]
    public int engagement = 0;            
    public TaskPhase currentPhase = TaskPhase.Early;

    [Header("Session queue")]
    public Queue<TaskData> sessionQueue = new Queue<TaskData>();

    [Header("UI")]
    [SerializeField] private NeonEngagementBar engagementBar;

    private List<TaskData> allTasks = new List<TaskData>();

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

        LoadAllTasks();
        GenerateInitialSession();
    }

    private void Start()
    {
        Debug.Log("[TaskManager] Start() called");
        Debug.Log("[TaskManager] engagementBar = " + (engagementBar != null ? "ASSIGNED" : "NULL"));

        if (engagementBar != null)
            engagementBar.UpdateBar(engagement);
    }

    /// <summary> Загружаем все ScriptableObject из Resources/Tasks </summary>
    void LoadAllTasks()
    {
        TaskData[] loaded = Resources.LoadAll<TaskData>("Tasks");
        allTasks = new List<TaskData>(loaded);
        Debug.Log($"[TaskManager] Loaded {allTasks.Count} tasks.");
    }

    /// <summary> Создаём начальную сессию из 2–3 заданий </summary>
    void GenerateInitialSession()
    {
        List<TaskData> available = GetAvailableTasks();
        int count = Mathf.Min(3, available.Count);

        for (int i = 0; i < count; i++)
        {
            sessionQueue.Enqueue(PickRandomTask(available));
        }
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
        List<TaskData> result = new List<TaskData>();
        foreach (var t in allTasks)
        {
            if (t.IsAvailable(engagement) && t.GetWeight(currentPhase) > 0f)
                result.Add(t);
        }
        return result;
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



    /// <summary> Изменить вовлечение </summary>
    public void ChangeEngagement(int delta)
    {             
        engagement = Mathf.Clamp(engagement + delta, 0, 100);
        Debug.Log($"[TaskManager] Engagement = {engagement}");

        if (engagementBar != null)
        {
            Debug.Log("[TaskManager] Calling UpdateBar()");
            engagementBar.UpdateBar(engagement);
        }
        else
        {
            Debug.LogWarning("[TaskManager] engagementBar is NULL!");
        }
    }
}
