using UnityEngine;

public sealed class FacialExpressionController : MonoBehaviour
{
    public void SetExpression(string id)
    {
        // Stub. Will be implemented later.
        Debug.Log($"[FacialExpressionController] SetExpression called: {id}");
    }

    public void ResetExpression()
    {
        Debug.Log("[FacialExpressionController] ResetExpression called");
    }
}
