using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseHealth : MonoBehaviour
{
    [Header("체력")]
    [SerializeField] private int maxHealth = 1000;
    [SerializeField] private int currentHealth;

    [Header("UI")]
    [SerializeField] private Image healthFill;
    [SerializeField] private TMP_Text healthText;

    private void Awake()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || currentHealth <= 0)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Debug.Log($"{gameObject.name} 파괴됨");
        }
    }

    private void UpdateHealthUI()
    {
        if (healthFill != null)
        {
            healthFill.fillAmount =
                (float)currentHealth / maxHealth;
        }

        if (healthText != null)
        {
            healthText.text =
                $"{currentHealth} / {maxHealth}";
        }
    }

    [ContextMenu("테스트 데미지 100")]
    private void TestDamage()
    {
        TakeDamage(100);
    }

    [ContextMenu("체력 초기화")]
    private void ResetHealth()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }
}