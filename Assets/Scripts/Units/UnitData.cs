using UnityEngine;
using UnityEngine.UI;

public class UnitData : MonoBehaviour
{
    [Header("기본 정보")]
    [SerializeField] private string unitName = "Pawn";
    [SerializeField] private bool isPlayerUnit = true;

    [Header("능력치")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private float moveSpeed = 100f;
    [SerializeField] private float attackRange = 80f;
    [SerializeField] private float attackInterval = 1f;

    [Header("체력 UI")]
    [SerializeField] private Image healthFill;

    private int currentHealth;

    public string UnitName => unitName;
    public bool IsPlayerUnit => isPlayerUnit;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public int AttackDamage => attackDamage;

    public float MoveSpeed => moveSpeed;
    public float AttackRange => attackRange;
    public float AttackInterval => attackInterval;

    public bool IsDead => currentHealth <= 0;

    private void Awake()
    {
        // 기물이 생성되면 현재 체력을 최대 체력으로 시작한다.
        currentHealth = maxHealth;

        // 처음 생성됐을 때 체력바를 100% 상태로 표시한다.
        UpdateHealthUI();
    }

    public void TakeDamage(int damage)
    {
        // 데미지가 0 이하이거나 이미 죽은 기물이면 처리하지 않는다.
        if (damage <= 0 || IsDead)
            return;

        // 현재 체력에서 받은 데미지를 차감한다.
        currentHealth -= damage;

        // 현재 체력이 0보다 작거나 최대 체력보다 커지지 않도록 제한한다.
        currentHealth = Mathf.Clamp(
            currentHealth,
            0,
            maxHealth
        );

        // 변경된 체력에 맞게 체력바를 다시 표시한다.
        UpdateHealthUI();

        // 체력이 0이 되면 기물 오브젝트를 삭제한다.
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void UpdateHealthUI()
    {
        // Inspector에서 체력 Fill을 연결하지 않았다면 실행하지 않는다.
        if (healthFill == null)
            return;

        // 현재 체력을 0~1 사이의 비율로 변환한다.
        healthFill.fillAmount =
            (float)currentHealth / maxHealth;
    }
}