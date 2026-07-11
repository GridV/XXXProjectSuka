using System;
using System.Collections.Generic;
using UnityEngine;

public enum ClothingSlot
{
    Upper,
    Lower
}

[Serializable]
public class ClothingItem
{
    public string id;              // "tshirt", "bra", "shorts", "underwear"
    public ClothingSlot slot;
    public GameObject clothingObject;
}

[Serializable]
public sealed class ClothingService
{
    [Header("Clothing Objects (disable to remove)")]
    [SerializeField] private ClothingItem tShirtItem;
    [SerializeField] private ClothingItem braItem;
    [SerializeField] private ClothingItem shortsItem;
    [SerializeField] private ClothingItem underwearItem;

    private readonly HashSet<string> removed = new();

    public bool IsUpperExposed => removed.Contains("tshirt");
    public bool IsLowerExposed => removed.Contains("shorts");

    public void ResetSessionState()
    {
        removed.Clear();
    }

    public void RemoveOneOnPlateauSuccess()
    {
        var item = GetNextItemToRemove();
        if (item == null)
            return;

        if (item.clothingObject != null)
            item.clothingObject.SetActive(false);

        removed.Add(item.id);

        Debug.Log($"[EdgeLadder] Clothing removed: {item.id}");
    }
    public void Bind(GameObject tshirt, GameObject shorts, GameObject bra, GameObject underwear)
    {
        // Fixed order: tshirt -> shorts -> bra -> underwear
        tShirtItem = new ClothingItem { id = "tshirt", slot = ClothingSlot.Upper, clothingObject = tshirt };
        shortsItem = new ClothingItem { id = "shorts", slot = ClothingSlot.Lower, clothingObject = shorts };
        braItem = new ClothingItem { id = "bra", slot = ClothingSlot.Upper, clothingObject = bra };
        underwearItem = new ClothingItem { id = "underwear", slot = ClothingSlot.Lower, clothingObject = underwear };
    }
    private ClothingItem GetNextItemToRemove()
    {
        if (!removed.Contains("tshirt") && IsValid(tShirtItem)) return tShirtItem;
        if (!removed.Contains("shorts") && IsValid(shortsItem)) return shortsItem;
        if (!removed.Contains("bra") && IsValid(braItem)) return braItem;
        if (!removed.Contains("underwear") && IsValid(underwearItem)) return underwearItem;

        return null;
    }
    public void ForceRemoveUpper()
    {
        if (tShirtItem?.clothingObject != null)
            tShirtItem.clothingObject.SetActive(false);

        if (!string.IsNullOrEmpty(tShirtItem?.id))
            removed.Add(tShirtItem.id);

        if (shortsItem?.clothingObject != null)
            shortsItem.clothingObject.SetActive(false);

        if (!string.IsNullOrEmpty(shortsItem?.id))
            removed.Add(shortsItem.id);
    }
    private static bool IsValid(ClothingItem item)
    {
        return item != null && !string.IsNullOrEmpty(item.id);
    }
    public void SetItemVisible(GameObject clothingObject, bool visible)
    {
        if (clothingObject == null)
            return;

        clothingObject.SetActive(visible);

        var item = FindItemByGameObject(clothingObject);
        if (item == null)
            return;

        if (visible)
            removed.Remove(item.id);
        else
            removed.Add(item.id);
    }

    private ClothingItem FindItemByGameObject(GameObject obj)
    {
        if (tShirtItem?.clothingObject == obj) return tShirtItem;
        if (shortsItem?.clothingObject == obj) return shortsItem;
        if (braItem?.clothingObject == obj) return braItem;
        if (underwearItem?.clothingObject == obj) return underwearItem;
        return null;
    }
    public void SetItemVisible(string itemId, bool visible)
    {
        if (string.IsNullOrEmpty(itemId))
            return;

        var item = FindItemById(itemId);
        if (item?.clothingObject == null)
            return;

        item.clothingObject.SetActive(visible);

        if (visible)
            removed.Remove(itemId);
        else
            removed.Add(itemId);
    }


    public bool IsItemVisible(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
            return true;

        return !removed.Contains(itemId);
    }

    // Implement according to your internal structure
    private ClothingItem FindItemById(string itemId)
    {
        if (tShirtItem != null && tShirtItem.id == itemId) return tShirtItem;
        if (shortsItem != null && shortsItem.id == itemId) return shortsItem;
        if (braItem != null && braItem.id == itemId) return braItem;
        if (underwearItem != null && underwearItem.id == itemId) return underwearItem;
        return null;
    }

}
