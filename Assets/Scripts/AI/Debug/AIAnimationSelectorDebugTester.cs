using UnityEngine;

// Debug tester for AIAnimationSelector.
public class AIAnimationSelectorDebugTester : MonoBehaviour
{
    void Start()
    {
        TestSelector(new[] { "slow_controlled", "loop" }, "Test 1");
        TestSelector(new[] { "fast_intense" }, "Test 2");
        TestSelector(new[] { "come_close" }, "Test 3");
        TestSelector(new[] { "invalid_tag" }, "Test 4");
    }

    private void TestSelector(string[] requestedTags, string testName)
    {
        var selected = AIAnimationSelector.SelectBest(requestedTags);
        if (selected == null)
        {
            Debug.Log($"{testName}: No animation selected for tags [{string.Join(", ", requestedTags)}].");
        }
        else
        {
            Debug.Log($"{testName}: Selected animation id '{selected.Id}' for tags [{string.Join(", ", requestedTags)}].");
        }
    }
}
