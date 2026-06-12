// HealthbarP3.cs — UI Healthbar untuk Player 3
// Letakkan di folder: HealthP3/

using UnityEngine;
using UnityEngine.UI;

public class HealthbarP3 : MonoBehaviour
{
    [SerializeField] private HealthP3 player3Health;
    [SerializeField] private Image totalhealthBar;
    [SerializeField] private Image currenthealthBar;

    private void Start()
    {
        if (player3Health != null)
            totalhealthBar.fillAmount = player3Health.currentHealth / 10f;
    }

    private void Update()
    {
        if (player3Health != null)
            currenthealthBar.fillAmount = player3Health.currentHealth / 10f;
    }
}