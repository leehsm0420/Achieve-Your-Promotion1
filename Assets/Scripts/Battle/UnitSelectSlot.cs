using UnityEngine;
using UnityEngine.EventSystems;

public class UnitSelectSlot :
    MonoBehaviour,
    IPointerClickHandler
{
    [Header("이 선택칸에서 배치할 기물")]
    [SerializeField] private GameObject unitPrefab;

    public void OnPointerClick(
        PointerEventData eventData)
    {
        if (unitPrefab == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: Unit Prefab이 연결되지 않았습니다."
            );

            return;
        }

        if (UnitPlacementManager.Instance == null)
        {
            Debug.LogError(
                "씬에 UnitPlacementManager가 없습니다."
            );

            return;
        }

        // 선택한 기물 프리팹을 배치 관리자에게 전달한다.
        UnitPlacementManager.Instance
            .SelectUnit(unitPrefab);
    }
}