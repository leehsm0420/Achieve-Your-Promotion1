using UnityEngine;

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

    private int currentHealth;

    public string UnitName => unitName;
    public bool IsPlayerUnit => isPlayerUnit;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public int AttackDamage => attackDamage;
    public float MoveSpeed => moveSpeed;
    public float AttackRange => attackRange;
    public float AttackInterval => attackInterval;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || currentHealth <= 0)
            return;

        currentHealth =
            Mathf.Max(0, currentHealth - damage);

        if (currentHealth == 0)
        {
            Destroy(gameObject);
        }
    }
}