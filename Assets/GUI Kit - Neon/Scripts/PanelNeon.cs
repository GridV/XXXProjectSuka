using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LayerLab
{
    public class PanelNeon : MonoBehaviour
    {
        [SerializeField] private GameObject[] otherPanels;

        public void OnEnable()
        {
            if (otherPanels == null) return;
            for (int i = 0; i < otherPanels.Length; i++) otherPanels[i].SetActive(true);
        }

        public void OnDisable()
        {
            if (otherPanels == null) return;
            for (int i = 0; i < otherPanels.Length; i++) otherPanels[i].SetActive(false);
        }
    }
}
