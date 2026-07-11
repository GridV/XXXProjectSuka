using UnityEngine;
using UnityEngine.UI;

public sealed class FinalTaskClothingPanel : MonoBehaviour
{
    [Header("Toggles")]
    [SerializeField] private Toggle tshirtToggle;
    [SerializeField] private Toggle shortsToggle;
    [SerializeField] private Toggle braToggle;
    [SerializeField] private Toggle underwearToggle;

    private ClothingService clothing;
    private bool suppressEvents;
    private bool isHooked;

    private const string IdTshirt = "tshirt";
    private const string IdShorts = "shorts";
    private const string IdBra = "bra";
    private const string IdUnderwear = "underwear";

    public void Bind(ClothingService service)
    {
        clothing = service;

        if (clothing == null)
        {
            Debug.LogWarning("[FinalTaskClothingPanel] Bind failed: ClothingService is null.");
            return;
        }

        HookEventsOnce();
        SyncFromService();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        SyncFromService();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void HookEventsOnce()
    {
        if (isHooked)
            return;

        isHooked = true;

        if (tshirtToggle != null) tshirtToggle.onValueChanged.AddListener(OnTshirtChanged);
        if (shortsToggle != null) shortsToggle.onValueChanged.AddListener(OnShortsChanged);
        if (braToggle != null) braToggle.onValueChanged.AddListener(OnBraChanged);
        if (underwearToggle != null) underwearToggle.onValueChanged.AddListener(OnUnderwearChanged);
    }

    private void SyncFromService()
    {
        if (clothing == null)
            return;

        suppressEvents = true;

        if (tshirtToggle != null) tshirtToggle.isOn = clothing.IsItemVisible(IdTshirt);
        if (shortsToggle != null) shortsToggle.isOn = clothing.IsItemVisible(IdShorts);
        if (braToggle != null) braToggle.isOn = clothing.IsItemVisible(IdBra);
        if (underwearToggle != null) underwearToggle.isOn = clothing.IsItemVisible(IdUnderwear);

        suppressEvents = false;
    }

    private void OnTshirtChanged(bool isOn)
    {
        if (suppressEvents || clothing == null) return;
        clothing.SetItemVisible(IdTshirt, isOn);
    }

    private void OnShortsChanged(bool isOn)
    {
        if (suppressEvents || clothing == null) return;
        clothing.SetItemVisible(IdShorts, isOn);
    }

    private void OnBraChanged(bool isOn)
    {
        if (suppressEvents || clothing == null) return;
        clothing.SetItemVisible(IdBra, isOn);
    }

    private void OnUnderwearChanged(bool isOn)
    {
        if (suppressEvents || clothing == null) return;
        clothing.SetItemVisible(IdUnderwear, isOn);
    }
}
