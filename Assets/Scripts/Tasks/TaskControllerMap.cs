using System;
using System.Collections.Generic;

public static class TaskControllerMap
{
    // Maps taskId -> concrete TaskController type
    private static readonly Dictionary<string, Type> map = new()
    {
        { "InitialTask", typeof(InitialTaskController) },
        { "Task_01", typeof(Task01Controller) },
        { "TaskRepeat", typeof(TaskRepeatCountController) },
        { "SemaphoreTask", typeof(SemaphoreTaskController) },
        { "EdgeLadderTask", typeof(EdgeLadderTaskController) },
        { "FinalTask", typeof(FinalTaskController) }
    };

    public static bool TryGetControllerType(string taskId, out Type controllerType)
    {
        return map.TryGetValue(taskId, out controllerType);
    }
}
