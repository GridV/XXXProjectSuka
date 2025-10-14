using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TaskRunner : MonoBehaviour
{
    [Header("References")]
    public Animator characterAnimator; // Assign your DAZ character's Animator here

    private TaskData currentTask;
    private bool isRunning = false;

    private void Start()
    {
        // Optionally start the first task automatically
        StartNextTask();
    }

    /// <summary>
    /// Gets the next task from TaskManager and starts it.
    /// </summary>
    public void StartNextTask()
    {
        if (isRunning) return;

        currentTask = TaskManager.Instance.GetNextTask();
        if (currentTask == null)
        {
            Debug.Log("[TaskRunner] No more tasks in queue.");
            return;
        }

        Debug.Log($"[TaskRunner] Starting task: {currentTask.title}");

        if (currentTask.taskType == TaskType.SpecialScene)
        {
            // Load another scene if the task requires it
            Time.timeScale = 1f; // Make sure the game is not paused
            SceneManager.LoadScene(currentTask.sceneName);
        }
        else
        {
            // Run the animation sequence in this scene
            StartCoroutine(PlayAnimationSequence(currentTask));
        }
    }

    /// <summary>
    /// Plays all animations defined in the task's animationSequence.
    /// </summary>
    private IEnumerator PlayAnimationSequence(TaskData task)
    {
        isRunning = true;

        if (task.animationSequence.Length == 0)
        {
            Debug.LogWarning("[TaskRunner] Task has no animations defined.");
        }
        else
        {
            foreach (var clip in task.animationSequence)
            {
                if (characterAnimator == null)
                {
                    Debug.LogWarning("[TaskRunner] No animator assigned.");
                    yield break;
                }

                Debug.Log($"[TaskRunner] Playing animation: {clip.stateName}");
                characterAnimator.Play(clip.stateName);

                // Wait for the animation duration
                yield return new WaitForSeconds(
                    characterAnimator.GetCurrentAnimatorStateInfo(0).length + clip.delayAfter
                );
            }
        }

        FinishTask(task);
        isRunning = false;
    }

    /// <summary>
    /// Called when a task is finished: gives engagement and adds a new one.
    /// </summary>
    private void FinishTask(TaskData task)
    {
        if (task.engagementReward != 0)
            TaskManager.Instance.ChangeEngagement(task.engagementReward);

        // Add next task to the session queue
        TaskManager.Instance.AddNextTask();

        Debug.Log($"[TaskRunner] Task finished: {task.title}. Engagement now {TaskManager.Instance.engagement}");
    }
}
