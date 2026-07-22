using UnityEngine;

public class UnitPlacementManager : MonoBehaviour
{
    public static UnitPlacementManager Instance
    { get; private set; }

    [Header("UI")]
    [SerializeField]
    private GameObject rowSelectPanel;

    [Header("»ý¼º")]
    [SerializeField]
    private RectTransform battleUnitRoot;

    [SerializeField]
    private RectTransform[] rowSpawnPoints;

    private GameObject selectedUnitPrefab;

    private void Awake()
    {
        Instance = this;

        if (rowSelectPanel != null)
            rowSelectPanel.SetActive(false);
    }

    public void SelectUnit(GameObject unitPrefab)
    {
        selectedUnitPrefab = unitPrefab;

        if (rowSelectPanel != null)
            rowSelectPanel.SetActive(true);
    }

    public void SelectRow(int rowIndex)
    {
        if (selectedUnitPrefab == null)
            return;

        if (rowIndex < 0 ||
            rowIndex >= rowSpawnPoints.Length)
            return;

        GameObject unit = Instantiate(
            selectedUnitPrefab,
            battleUnitRoot);

        RectTransform unitRect =
            unit.GetComponent<RectTransform>();

        if (unitRect != null)
        {
            unitRect.position =
                rowSpawnPoints[rowIndex].position;

            unitRect.localScale = Vector3.one;
        }

        selectedUnitPrefab = null;
        rowSelectPanel.SetActive(false);
    }

    public void CancelPlacement()
    {
        selectedUnitPrefab = null;
        rowSelectPanel.SetActive(false);
    }
}