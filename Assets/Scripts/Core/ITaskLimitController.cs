using System;

public enum TaskLimitReachedReason
{
    Time,
    RhythmSteps
}

public interface ITaskLimitController
{
    event Action<TaskLimitReachedReason> OnLimitReached;

    void Start();
    void Stop();

    void Tick(float deltaTime);
    void OnRhythmStep();
}
