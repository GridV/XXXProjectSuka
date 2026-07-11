using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class OptionGroupAligner : MonoBehaviour
{
    private VerticalLayoutGroup layoutGroup;

    private void Awake()
    {
        layoutGroup = GetComponent<VerticalLayoutGroup>();
    }

    private void Update()
    {
        int activeChildren = 0;
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf)
                activeChildren++;
        }

        if (layoutGroup == null) return;

        // Если одна кнопка — выравниваем по центру, иначе как обычно
        layoutGroup.childAlignment = activeChildren == 1
            ? TextAnchor.MiddleCenter
            : (transform.name.Contains("Left")
                ? TextAnchor.MiddleRight
                : TextAnchor.MiddleLeft);
    }
}
