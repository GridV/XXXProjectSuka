using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameBootRoot : MonoBehaviour
{
    private static GameBootRoot instance;

    [Header("Boot references")]
    [SerializeField] private AIResponseExecutor aiResponseExecutor;
    [SerializeField] private AISessionDirector aiSessionDirector;

    private SceneRuntimeBinder activeSceneBinder;
    private int activeSceneHandle = -1;
    private bool sessionStarted;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
        SceneManager.sceneUnloaded += HandleSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneUnloaded -= HandleSceneUnloaded;
    }

    private void Start()
    {
        ResolveBootReferences();

        // Ищем binder во всех сценах, которые уже загружены.
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);

            if (!scene.isLoaded)
                continue;

            if (TryBindScene(scene))
                break;
        }

        StartSessionWhenReady();
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Работает для любых будущих environment scenes.
        if (TryBindScene(scene))
            StartSessionWhenReady();
    }

    private void HandleSceneUnloaded(Scene scene)
    {
        if (activeSceneHandle != scene.handle)
            return;

        if (aiResponseExecutor != null && activeSceneBinder != null)
        {
            aiResponseExecutor.UnbindSceneRuntime(activeSceneBinder);
        }

        Debug.Log(
            $"[GameBootRoot] Scene runtime unbound for '{scene.name}'.");

        activeSceneBinder = null;
        activeSceneHandle = -1;
    }

    private bool TryBindScene(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded)
            return false;

        SceneRuntimeBinder binder = FindSceneRuntimeBinder(scene);

        // BootScene обычно не содержит binder, поэтому молча пропускаем.
        if (binder == null)
            return false;

        ResolveBootReferences();

        if (aiResponseExecutor == null)
        {
            Debug.LogError(
                "[GameBootRoot] AIResponseExecutor not found; " +
                "cannot bind scene runtime.");
            return false;
        }

        if (binder.AIAnimationExecutor == null)
        {
            Debug.LogError(
                $"[GameBootRoot] SceneRuntimeBinder in '{scene.name}' " +
                "has no AIAnimationExecutor assigned.");
            return false;
        }

        // Если уже привязана другая environment scene, отвязываем её.
        if (activeSceneBinder != null &&
            activeSceneBinder != binder)
        {
            aiResponseExecutor.UnbindSceneRuntime(activeSceneBinder);
        }

        activeSceneBinder = binder;
        activeSceneHandle = scene.handle;

        Debug.Log(
            $"[GameBootRoot] Scene binder found in '{scene.name}'.");

        aiResponseExecutor.BindSceneRuntime(binder);

        Debug.Log(
            $"[GameBootRoot] Scene runtime bound for '{scene.name}'.");

        return true;
    }

    private void StartSessionWhenReady()
    {
        if (sessionStarted)
            return;

        if (activeSceneBinder == null)
        {
            Debug.LogWarning(
                "[GameBootRoot] AI session not started: " +
                "no environment SceneRuntimeBinder is bound.");
            return;
        }

        ResolveBootReferences();

        if (aiSessionDirector == null)
        {
            Debug.LogError(
                "[GameBootRoot] AISessionDirector not found.");
            return;
        }

        sessionStarted = true;

        Debug.Log(
            "[GameBootRoot] Runtime ready. Starting AI session.");

        aiSessionDirector.StartDefaultSession();
    }

    private void ResolveBootReferences()
    {
        if (aiResponseExecutor == null)
            aiResponseExecutor =
                FindFirstObjectByType<AIResponseExecutor>();

        if (aiSessionDirector == null)
            aiSessionDirector =
                FindFirstObjectByType<AISessionDirector>();
    }

    private static SceneRuntimeBinder FindSceneRuntimeBinder(
        Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            SceneRuntimeBinder binder =
                root.GetComponentInChildren<SceneRuntimeBinder>(true);

            if (binder != null)
                return binder;
        }

        return null;
    }
}