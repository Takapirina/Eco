using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuView : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private GameObject movesPanel;

    [Header("Root Rows (4)")]
    [SerializeField] private TMP_Text[] rootRows;

    [Header("Moves Rows (4)")]
    [SerializeField] private TMP_Text[] movesRows;

    private readonly string[] rootLabels = { "Attacca", "Difendi", "Strumenti", "Fuggi" };

    private void Awake()
    {
        // default: root visibile, moves nascosto
        if (rootPanel) rootPanel.SetActive(true);
        if (movesPanel) movesPanel.SetActive(false);
    }

    public void ShowRoot(int index)
    {
        if (rootPanel) rootPanel.SetActive(true);
        if (movesPanel) movesPanel.SetActive(false);

        DrawList(rootRows, rootLabels, index);
    }

    public void ShowMoves(List<string> moveNames, int index)
    {
        if (rootPanel) rootPanel.SetActive(true);
        if (movesPanel) movesPanel.SetActive(true);

        DrawList(movesRows, moveNames, index);
    }

    // overload per array di stringhe
    private void DrawList(TMP_Text[] rows, string[] items, int selected)
    {
        if (rows == null) return;

        for (int i = 0; i < rows.Length; i++)
        {
            if (rows[i] == null) continue;

            if (items == null || i >= items.Length)
            {
                rows[i].text = "";
                continue;
            }

            rows[i].text = (i == selected ? "> " : "  ") + items[i];
        }
    }

    // overload per lista (mosse)
    private void DrawList(TMP_Text[] rows, List<string> items, int selected)
    {
        if (rows == null) return;

        for (int i = 0; i < rows.Length; i++)
        {
            if (rows[i] == null) continue;

            if (items == null || i >= items.Count)
            {
                rows[i].text = "";
                continue;
            }

            rows[i].text = (i == selected ? "> " : "  ") + items[i];
        }
    }
}