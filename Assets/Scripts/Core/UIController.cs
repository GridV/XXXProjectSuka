using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("Dialogue UI")]
    public TMP_Text dialogueText;

    void Start()
    {
        // При включении UI начинаем с пустого текста и скрытой кнопки
        ClearText();
    }

    // --- Основные методы ---

    public void ShowText(string text)
    {
        dialogueText.text = text;
        dialogueText.gameObject.SetActive(true);
    }

    public void ClearText()
    {
        dialogueText.text = "";
        dialogueText.gameObject.SetActive(false);
    }

    // --- Будущие расширения (пока пустые, но пригодятся) ---

    public void ShowHint(string text)
    {
        // Подсказки/ритм-индикаторы 
        // оставляем место для расширения
    }

    public void ClearHint()
    {
        // убираем будущие подсказки
    }
}
