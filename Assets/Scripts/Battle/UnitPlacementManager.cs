using System;
using UnityEngine;

/// <summary>
/// 전장 한 줄에 포함된 셀 8개의 정보를 보관한다.
/// 이 클래스는 프로젝트 안에서 한 번만 선언한다.
/// </summary>
[Serializable]
public class BoardRow
{
    [Tooltip("왼쪽에서 오른쪽 순서로 셀을 연결합니다.")]
    public RectTransform[] cells;
}

public class UnitPlacementManager : MonoBehaviour
{
    public static UnitPlacementManager Instance
    {
        get;
        private set;
    }

    [Header("베이스")]
    [Tooltip("플레이어 기물이 마지막 셀에 도착했을 때 공격할 적 베이스 체력")]
    [SerializeField] private BaseHealth enemyBaseHealth;

    [Header("UI")]
    [SerializeField] private GameObject rowSelectPanel;

    [Header("기물 생성")]
    [SerializeField] private RectTransform battleUnitRoot;

    [Header("전장 셀 경로")]
    [SerializeField] private BoardRow[] boardRows;

    // 현재 플레이어가 선택한 기물 프리팹을 임시로 저장한다.
    private GameObject selectedUnitPrefab;

    private void Awake()
    {
        Instance = this;

        // 게임 시작 시 라인 선택 패널은 보이지 않게 한다.
        if (rowSelectPanel != null)
        {
            rowSelectPanel.SetActive(false);
        }
    }

    public void SelectUnit(GameObject unitPrefab)
    {
        // 클릭한 선택칸의 기물 프리팹을 저장한다.
        selectedUnitPrefab = unitPrefab;

        // 기물을 배치할 줄을 선택하도록 패널을 연다.
        if (rowSelectPanel != null)
        {
            rowSelectPanel.SetActive(true);
        }
    }

    public void SelectRow(int rowIndex)
    {
        // 기물 선택 없이 줄 버튼부터 눌렀다면 생성하지 않는다.
        if (selectedUnitPrefab == null)
            return;

        // 배열 범위를 벗어난 줄 번호라면 생성하지 않는다.
        if (rowIndex < 0 || rowIndex >= boardRows.Length)
        {
            Debug.LogError("잘못된 줄 번호입니다.");
            return;
        }

        RectTransform[] selectedRowCells =
            boardRows[rowIndex].cells;

        // 선택한 줄에 셀이 연결되지 않았다면 생성하지 않는다.
        if (selectedRowCells == null ||
            selectedRowCells.Length == 0)
        {
            Debug.LogError(
                $"{rowIndex + 1}번 줄에 셀이 연결되지 않았습니다."
            );
            return;
        }

        // 선택한 프리팹을 BattleUnitRoot 아래에 생성한다.
        GameObject unit = Instantiate(
            selectedUnitPrefab,
            battleUnitRoot
        );

        RectTransform unitRect =
            unit.GetComponent<RectTransform>();

        if (unitRect != null)
        {
            unitRect.localScale = Vector3.one;

            // 플레이어 기물은 선택한 줄의 첫 번째 셀에서 시작한다.
            unitRect.position =
                selectedRowCells[0].position;
        }

        UnitAI unitAI =
            unit.GetComponent<UnitAI>();

        if (unitAI != null)
        {
            // 생성된 기물에게 소속, 줄 번호, 셀 경로,
            // 공격할 적 베이스를 전달한다.
            unitAI.Initialize(
                UnitAI.Team.Player,
                rowIndex,
                selectedRowCells,
                enemyBaseHealth
            );
        }
        else
        {
            Debug.LogError(
                $"{unit.name} 프리팹에 UnitAI가 없습니다."
            );
        }

        // 배치가 끝났으므로 선택 정보를 초기화한다.
        selectedUnitPrefab = null;

        // 라인 선택 패널을 닫는다.
        if (rowSelectPanel != null)
        {
            rowSelectPanel.SetActive(false);
        }
    }

    public void CancelPlacement()
    {
        // 취소 버튼을 누르면 현재 선택한 기물을 취소한다.
        selectedUnitPrefab = null;

        if (rowSelectPanel != null)
        {
            rowSelectPanel.SetActive(false);
        }
    }
}