using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAI : MonoBehaviour
{
    public enum Team
    {
        Player,
        Enemy
    }

    // 현재 씬에 존재하는 모든 기물을 저장한다.
    // 적 탐색과 앞 칸 점유 확인에 사용한다.
    private static readonly List<UnitAI> ActiveUnits = new();

    [Header("이동")]
    [Tooltip("현재 셀에서 다음 셀까지 이동하는 데 걸리는 시간")]
    [SerializeField] private float moveDuration = 0.35f;

    [Tooltip("한 칸 이동한 후 다음 행동까지 기다리는 시간")]
    [SerializeField] private float waitBetweenMoves = 0.2f;

    [Header("전투")]
    [Tooltip("몇 칸 이내의 적을 공격할지 설정합니다. 폰은 우선 1칸입니다.")]
    [SerializeField] private int attackRangeCells = 1;

    private RectTransform rectTransform;
    private UnitData unitData;

    private RectTransform[] pathCells;
    private BaseHealth targetBase;

    private Team team;
    private int rowIndex;
    private int currentCellIndex;

    private bool initialized;
    private bool isMoving;
    private bool canAct = true;

    private UnitAI currentTarget;
    private Coroutine actionRoutine;

    public Team UnitTeam => team;
    public int RowIndex => rowIndex;
    public int CurrentCellIndex => currentCellIndex;
    public bool IsInitialized => initialized;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        unitData = GetComponent<UnitData>();
    }

    private void OnEnable()
    {
        // 현재 기물을 전체 기물 목록에 등록한다.
        if (!ActiveUnits.Contains(this))
        {
            ActiveUnits.Add(this);
        }
    }

    private void OnDisable()
    {
        // 기물이 삭제되거나 비활성화되면 목록에서 제거한다.
        ActiveUnits.Remove(this);
    }

    public void Initialize(
        Team newTeam,
        int newRowIndex,
        RectTransform[] newPathCells,
        BaseHealth newTargetBase)
    {
        team = newTeam;
        rowIndex = newRowIndex;
        pathCells = newPathCells;
        targetBase = newTargetBase;

        if (pathCells == null || pathCells.Length == 0)
        {
            Debug.LogError(
                $"{gameObject.name}: 이동할 셀이 없습니다."
            );

            return;
        }

        // 플레이어는 줄의 왼쪽 끝에서 시작한다.
        // 적은 줄의 오른쪽 끝에서 시작한다.
        currentCellIndex =
            team == Team.Player
                ? 0
                : pathCells.Length - 1;

        rectTransform.position =
            pathCells[currentCellIndex].position;

        initialized = true;

        if (actionRoutine != null)
        {
            StopCoroutine(actionRoutine);
        }

        actionRoutine =
            StartCoroutine(ActionRoutine());
    }

    private IEnumerator ActionRoutine()
    {
        while (initialized)
        {
            if (!canAct || isMoving)
            {
                yield return null;
                continue;
            }

            // 1. 같은 줄에서 공격 가능한 적이 있는지 확인한다.
            currentTarget = FindAttackTarget();

            // 2. 적이 있으면 이동하지 않고 공격한다.
            if (currentTarget != null)
            {
                AttackCurrentTarget();

                float interval =
                    unitData != null
                        ? unitData.AttackInterval
                        : 1f;

                yield return new WaitForSeconds(interval);
                continue;
            }

            // 3. 상대 끝까지 도착했다면 상대 베이스를 공격한다.
            if (HasReachedEnemyBase())
            {
                AttackBase();

                float interval =
                    unitData != null
                        ? unitData.AttackInterval
                        : 1f;

                yield return new WaitForSeconds(interval);
                continue;
            }

            // 4. 다음 칸에 다른 기물이 있으면 이동하지 않고 기다린다.
            if (IsNextCellOccupied())
            {
                yield return new WaitForSeconds(0.1f);
                continue;
            }

            // 5. 적도 없고 앞 칸도 비어 있다면 한 칸 이동한다.
            yield return MoveOneCell();

            yield return new WaitForSeconds(
                waitBetweenMoves
            );
        }
    }

    private UnitAI FindAttackTarget()
    {
        UnitAI closestTarget = null;
        int closestDistance = int.MaxValue;

        foreach (UnitAI other in ActiveUnits)
        {
            if (other == null || other == this)
                continue;

            if (!other.initialized)
                continue;

            // 같은 팀은 공격하지 않는다.
            if (other.team == team)
                continue;

            // 다른 줄의 적은 공격하지 않는다.
            if (other.rowIndex != rowIndex)
                continue;

            int distance = Mathf.Abs(
                other.currentCellIndex -
                currentCellIndex
            );

            // 공격 범위 안에서 가장 가까운 적을 선택한다.
            if (distance <= attackRangeCells &&
                distance < closestDistance)
            {
                closestTarget = other;
                closestDistance = distance;
            }
        }

        return closestTarget;
    }

    private bool IsNextCellOccupied()
    {
        int direction =
            team == Team.Player ? 1 : -1;

        int nextCellIndex =
            currentCellIndex + direction;

        // 다음 셀이 경로 밖이라면 이동하지 않는다.
        if (nextCellIndex < 0 ||
            nextCellIndex >= pathCells.Length)
        {
            return true;
        }

        foreach (UnitAI other in ActiveUnits)
        {
            if (other == null || other == this)
                continue;

            if (!other.initialized)
                continue;

            // 다른 줄의 기물은 현재 줄 이동을 막지 않는다.
            if (other.rowIndex != rowIndex)
                continue;

            // 다음 칸에 기물이 있으면 이동하지 않는다.
            if (other.currentCellIndex == nextCellIndex)
            {
                return true;
            }
        }

        return false;
    }

    private void AttackCurrentTarget()
    {
        if (currentTarget == null)
            return;

        UnitData targetData =
            currentTarget.GetComponent<UnitData>();

        if (targetData == null || targetData.IsDead)
        {
            currentTarget = null;
            return;
        }

        int damage =
            unitData != null
                ? unitData.AttackDamage
                : 10;

        // 상대 UnitData의 TakeDamage를 호출한다.
        targetData.TakeDamage(damage);

        Debug.Log(
            $"{gameObject.name}이(가) " +
            $"{currentTarget.name}에게 " +
            $"{damage} 데미지를 주었습니다."
        );

        // 공격으로 상대가 죽었다면 현재 타깃을 해제한다.
        if (targetData.IsDead)
        {
            currentTarget = null;
        }
    }

    private void AttackBase()
    {
        if (targetBase == null)
            return;

        int damage =
            unitData != null
                ? unitData.AttackDamage
                : 10;

        targetBase.TakeDamage(damage);

        Debug.Log(
            $"{gameObject.name}이(가) " +
            $"상대 베이스에 {damage} 데미지를 주었습니다."
        );
    }

    private bool HasReachedEnemyBase()
    {
        if (pathCells == null ||
            pathCells.Length == 0)
        {
            return false;
        }

        if (team == Team.Player)
        {
            return currentCellIndex >=
                   pathCells.Length - 1;
        }

        return currentCellIndex <= 0;
    }

    private IEnumerator MoveOneCell()
    {
        int direction =
            team == Team.Player ? 1 : -1;

        int nextCellIndex =
            currentCellIndex + direction;

        if (nextCellIndex < 0 ||
            nextCellIndex >= pathCells.Length)
        {
            yield break;
        }

        isMoving = true;

        Vector3 startPosition =
            rectTransform.position;

        Vector3 targetPosition =
            pathCells[nextCellIndex].position;

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / moveDuration
            );

            rectTransform.position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    t
                );

            yield return null;
        }

        // 보간 계산에서 생길 수 있는 작은 오차를 없앤다.
        rectTransform.position = targetPosition;

        // 현재 기물이 몇 번째 셀에 있는지 갱신한다.
        currentCellIndex = nextCellIndex;

        isMoving = false;
    }

    public void StopActing()
    {
        canAct = false;
    }

    public void ResumeActing()
    {
        canAct = true;
    }
}
