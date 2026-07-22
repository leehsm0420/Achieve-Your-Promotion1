using UnityEngine;
using UnityEngine.EventSystems;

public class UnitSelectSlot :
    MonoBehaviour,
    IPointerClickHandler
{
    [SerializeField]
    private GameObject unitPrefab;

    public void OnPointerClick(
        PointerEventData eventData)
    {
        UnitPlacementManager.Instance
            .SelectUnit(unitPrefab);
    }
}
