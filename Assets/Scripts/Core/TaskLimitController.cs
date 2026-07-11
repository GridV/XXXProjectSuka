using System;

public class TaskLimitController : ITaskLimitController
{
    public event Action<TaskLimitReachedReason> OnLimitReached;

    private readonly TaskLimitType limitType;
    private readonly float timeLimit;
    private readonly int rhythmLimit;

    private float elapsed;
    private int rhythmSteps;
    private bool isActive;
    private bool triggered;
    private bool isPaused;

    public TaskLimitController(TaskData data)
    {
        limitType = data.limitType;
        timeLimit = data.timeLimitSeconds;
        rhythmLimit = data.rhythmStepLimit;
    }

    public void Pause()
    {
        isPaused = true;
    }

    public void Resume()
    {
        isPaused = false;
    }
    public void Start()
    {
        elapsed = 0f;
        rhythmSteps = 0;
        triggered = false;
        isActive = true;
    }

    public void Stop()
    {
        isActive = false;
    }

    public void Tick(float deltaTime)
    {
        if (!isActive || triggered || isPaused)
            return;

        if (limitType != TaskLimitType.Time)
            return;

        elapsed += deltaTime;

        if (elapsed >= timeLimit)
        {
            triggered = true;
            OnLimitReached?.Invoke(TaskLimitReachedReason.Time);
        }
    }

    public void OnRhythmStep()
    {
        if (!isActive || triggered || isPaused)
            return;

        if (limitType != TaskLimitType.RhythmCount)
            return;

        rhythmSteps++;

        if (rhythmSteps >= rhythmLimit)
        {
            triggered = true;
            OnLimitReached?.Invoke(TaskLimitReachedReason.RhythmSteps);
        }
    }
}
