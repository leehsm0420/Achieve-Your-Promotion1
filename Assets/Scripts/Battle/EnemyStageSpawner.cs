using System;
using System.Collections;
using UnityEngine;

public class EnemyStageSpawner : MonoBehaviour
{
    [Serializable]
    public class EnemySpawnOrder
    {
        [Header("소환할 적")]
        public GameObject enemyPrefab;

        [Header("소환 라인")]
        [Tooltip("1번 줄부터 5번 줄까지 설정합니다.")]
        [Range(1, 5)]
        public int rowNumber = 1;

        [Header("소환 수량")]
        [Min(1)]
        public int spawnCount = 1;

        [Header("시간")]
        [Tooltip("게임 시작 후 첫 번째 적이 생성될 때까지 기다리는 시간")]
        [Min(0f)]
        public float firstSpawnDelay = 1f;

        [Tooltip("같은 소환 명령에서 적과 적 사이의 생성 간격")]
        [Min(0.1f)]
        public float spawnInterval = 2f;
    }

    [Header("스테이지 정보")]
    [SerializeField] private int stageNumber = 1;

    [Header("참조")]
    [SerializeField] private RectTransform battleUnitRoot;

    [Tooltip("적 기물이 마지막 셀에 도착했을 때 공격할 플레이어 베이스")]
    [SerializeField] private BaseHealth playerBaseHealth;

    [Header("전장 셀 경로")]
    [SerializeField] private BoardRow[] boardRows;

    [Header("적 소환 명령")]
    [SerializeField] private EnemySpawnOrder[] spawnOrders;

    [Header("시작 설정")]
    [SerializeField] private bool autoStart = true;

    private void Start()
    {
        if (autoStart)
        {
            StartStage();
        }
    }

    [ContextMenu("스테이지 시작")]
    public void StartStage()
    {
        if (spawnOrders == null ||
            spawnOrders.Length == 0)
        {
            Debug.LogWarning(
                $"Stage {stageNumber}: 적 소환 설정이 없습니다."
            );

            return;
        }

        // Inspector에 작성한 각 소환 명령을 실행한다.
        foreach (EnemySpawnOrder order in spawnOrders)
        {
            StartCoroutine(
                SpawnOrderRoutine(order)
            );
        }

        Debug.Log($"Stage {stageNumber} 시작");
    }

    private IEnumerator SpawnOrderRoutine(
        EnemySpawnOrder order)
    {
        if (order.enemyPrefab == null)
        {
            Debug.LogWarning(
                "Enemy Prefab이 연결되지 않았습니다."
            );

            yield break;
        }

        // Inspector에서는 1~5를 사용하지만
        // 배열은 0~4를 사용하므로 1을 뺀다.
        int rowIndex = order.rowNumber - 1;

        if (rowIndex < 0 ||
            rowIndex >= boardRows.Length)
        {
            Debug.LogError(
                $"{order.rowNumber}번 줄이 존재하지 않습니다."
            );

            yield break;
        }

        RectTransform[] rowCells =
            boardRows[rowIndex].cells;

        if (rowCells == null ||
            rowCells.Length == 0)
        {
            Debug.LogError(
                $"{order.rowNumber}번 줄의 셀이 연결되지 않았습니다."
            );

            yield break;
        }

        // 첫 적을 소환하기 전까지 기다린다.
        yield return new WaitForSeconds(
            order.firstSpawnDelay
        );

        for (int i = 0;
             i < order.spawnCount;
             i++)
        {
            SpawnEnemy(
                order.enemyPrefab,
                rowIndex,
                rowCells
            );

            // 마지막 기물 이후에는 추가 대기가 필요 없다.
            if (i < order.spawnCount - 1)
            {
                yield return new WaitForSeconds(
                    order.spawnInterval
                );
            }
        }
    }

    private void SpawnEnemy(
        GameObject enemyPrefab,
        int rowIndex,
        RectTransform[] rowCells)
    {
        GameObject enemy = Instantiate(
            enemyPrefab,
            battleUnitRoot
        );

        RectTransform enemyRect =
            enemy.GetComponent<RectTransform>();

        if (enemyRect != null)
        {
            enemyRect.localScale = Vector3.one;

            // 적은 해당 줄의 가장 오른쪽 셀에서 생성된다.
            enemyRect.position =
                rowCells[rowCells.Length - 1].position;
        }

        UnitAI enemyAI =
            enemy.GetComponent<UnitAI>();

        if (enemyAI != null)
        {
            enemyAI.Initialize(
                UnitAI.Team.Enemy,
                rowIndex,
                rowCells,
                playerBaseHealth
            );
        }
        else
        {
            Debug.LogError(
                $"{enemy.name} 프리팹에 UnitAI가 없습니다."
            );
        }
    }
}