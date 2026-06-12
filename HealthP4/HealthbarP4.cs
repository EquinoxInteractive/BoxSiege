// HealthbarP4.cs — UI Healthbar untuk Player 4
// Letakkan di folder: HealthP4/

using UnityEngine;
using UnityEngine.UI;

public class HealthbarP4 : MonoBehaviour
{
    [SerializeField] private HealthP4 player4Health;
    [SerializeField] private Image totalhealthBar;
    [SerializeField] private Image currenthealthBar;

    private void Start()
    {
        if (player4Health != null)
            totalhealthBar.fillAmount = player4Health.currentHealth / 10f;
    }

    private void Update()
    {
        if (player4Health != null)
            currenthealthBar.fillAmount = player4Health.currentHealth / 10f;
    }
}