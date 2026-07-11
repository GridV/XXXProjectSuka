using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TaskRunner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StateController stateController;

    [Header("Environment")]
    [SerializeField] private string defaultEnvironmentSceneName = "Env_Default";
    private string loadedEnvironmentSceneName;
    private bool isEnvironmentLoading;

    private TaskData currentTask;
    private bool isRunning;

    private const string SemaphoreEnvSceneName = "Env_Semaphore";
    private const string SemaphoreIdleStatePath = "Base Layer.Semaphore.sitting"; // adjust if your path differs

    private void Start()
    {
        StartCoroutine(WaitForSession());
    }

    private IEnumerator WaitForSession()
    {
        while (TaskManager.Instance == null)
            yield return null;

        while (TaskManager.Instance.sessionQueue == null)
            yield return null;

        while (TaskManager.Instance.sessionQueue.Count == 0)
            yield return null;

        TaskManager.Instance.RegisterTaskRunner(this);

        // Canonical: normalize editor state -> keep only default env
        yield return EnsureOnlyTargetEnvLoaded(defaultEnvironmentSceneName);

        StartNextTask();
    }

    public void StartNextTask()
    {
        if (isRunning) return;
        StartCoroutine(StartNextTaskRoutine());
    }

    private IEnumerator StartNextTaskRoutine()
    {
        if (isRunning)
            yield break;

        isRunning = true;

        currentTask = TaskManager.Instance.GetNextTask();
        if (currentTask == null)
        {
            isRunning = false;
            yield break;
        }

        string targetEnvScene = ResolveTargetEnvScene(currentTask);

        // Canonical: always normalize -> keep only target env
        yield return EnsureOnlyTargetEnvLoaded(targetEnvScene);

        FillUpGlobalContext();
        ApplyEnvDefaultIdleIfNeeded();

        var ctx = GameContext.Instance.TaskContext;

        if (ctx.controller != null)
        {
            Destroy(ctx.controller.gameObject);
            ctx.controller = null;
        }

        var controller = CreateControllerForTask(currentTask);
        if (controller == null)
        {
            isRunning = false;
            yield break;
        }

        ctx.controller = controller;
        controller.Init(currentTask);

        if (stateController != null)
            stateController.EnterDialogue();
        else
            Debug.LogError("[TaskRunner] StateController reference is missing.");

        ctx.controller.StartTask();

        isRunning = false;
    }

    private string ResolveTargetEnvScene(TaskData task)
    {
        if (task != null &&
            task.taskType == TaskType.SpecialScene &&
            !string.IsNullOrWhiteSpace(task.sceneName))
        {
            return task.sceneName;
        }

        return defaultEnvironmentSceneName;
    }

    // -------------------------
    // Scene loading (canonical)
    // -------------------------

    private IEnumerator EnsureOnlyTargetEnvLoaded(string targetSceneName)
    {
        if (string.IsNullOrWhiteSpace(targetSceneName))
            yield break;

        // 1) Unload all Env_ scenes except target
        yield return UnloadAllEnvScenesExcept(targetSceneName);

        // 2) Load target if needed
        yield return LoadEnvironmentSceneIfNeeded(targetSceneName);

        loadedEnvironmentSceneName = targetSceneName;
    }

    private IEnumerator LoadEnvironmentSceneIfNeeded(string targetSceneName)
    {
        if (isEnvironmentLoading)
            yield break;

        isEnvironmentLoading = true;

        var target = SceneManager.GetSceneByName(targetSceneName);
        if (!target.IsValid() || !target.isLoaded)
        {
            var op = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);
            while (op != null && !op.isDone)
                yield return null;
        }

        // One frame for scene objects to initialize
        yield return null;

        isEnvironmentLoading = false;
    }

    private IEnumerator UnloadAllEnvScenesExcept(string keepSceneName)
    {
        for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
        {
            var s = SceneManager.GetSceneAt(i);
            if (!s.isLoaded)
                continue;

            if (!s.name.StartsWith("Env_"))
                continue;

            if (s.name == keepSceneName)
                continue;

            var op = SceneManager.UnloadSceneAsync(s);
            while (op != null && !op.isDone)
                yield return null;
        }

        // One frame for cleanup
        yield return null;
    }

    // -------------------------
    // Context wiring
    // -------------------------

    public void FillUpGlobalContext()
    {
        var ctx = GameContext.Instance.TaskContext;

        ctx.currentTask = currentTask;

        // Scene services (must come from the loaded env scene)
        ctx.animator = FindInScene<Animator>(loadedEnvironmentSceneName);
        ctx.characterAnimation = FindInScene<CharacterAnimationService>(loadedEnvironmentSceneName);
        ctx.cameraMoveService = FindInScene<CameraMoveService>(loadedEnvironmentSceneName);
        ctx.rigService = Object.FindFirstObjectByType<CharacterRigService>();
        ctx.facial = Object.FindFirstObjectByType<FacialExpressionController>();
        ctx.danceCameraService = FindInScene<CinemachineDanceSwitcher>(loadedEnvironmentSceneName);

        // Persistent services (BootScene)
        if (ctx.sessionStats == null) ctx.sessionStats = FindObjectOfType<SessionStatsService>();
        if (ctx.dialogueManager == null) ctx.dialogueManager = FindObjectOfType<DialogueManager>();
        if (ctx.dialogueUI == null) ctx.dialogueUI = FindObjectOfType<DialogueUIController>();
        if (ctx.rhythmManager == null) ctx.rhythmManager = FindObjectOfType<RhythmController>();

        if (ctx.animator == null)
            Debug.LogError($"[TaskRunner] Animator not found in env scene '{loadedEnvironmentSceneName}'.");
        if (ctx.rigService == null)
            Debug.LogWarning("[TaskRunner] CharacterRigService not found in loaded environment scene.");
        else
            ctx.rigService.SafeRebuild();
    }

    private T FindInScene<T>(string sceneName) where T : Object
    {
        if (string.IsNullOrEmpty(sceneName))
            return null;

        var scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.IsValid() || !scene.isLoaded)
            return null;

        var roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            var found = roots[i].GetComponentInChildren<T>(true);
            if (found != null)
                return found;
        }

        return null;
    }

    // -------------------------
    // Env-specific defaults
    // -------------------------

    private void ApplyEnvDefaultIdleIfNeeded()
    {
        if (loadedEnvironmentSceneName != SemaphoreEnvSceneName)
            return;

        var animator = GameContext.Instance.TaskContext != null ? GameContext.Instance.TaskContext.animator : null;
        if (animator == null)
            return;

        // Force Semaphore sitting idle on Base Layer when Env_Semaphore is loaded
        animator.Play(SemaphoreIdleStatePath, 0, 0f);
        animator.Update(0f);
    }

    // -------------------------
    // Controller creation
    // -------------------------

    private BaseTaskController CreateControllerForTask(TaskData task)
    {
        if (task == null)
            return null;

        if (!TaskControllerMap.TryGetControllerType(task.id, out var controllerType))
        {
            Debug.LogError($"[TaskRunner] No TaskController mapped for task id: {task.id}");
            return null;
        }

        if (!typeof(BaseTaskController).IsAssignableFrom(controllerType))
        {
            Debug.LogError($"[TaskRunner] Controller {controllerType.Name} does not inherit BaseTaskController");
            return null;
        }

        var go = new GameObject(controllerType.Name);
        return (BaseTaskController)go.AddComponent(controllerType);
    }
}
