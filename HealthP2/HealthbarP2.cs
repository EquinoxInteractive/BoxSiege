using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthbarP2 : MonoBehaviour
{
    [SerializeField] private HealthP2 player2Health;
    [SerializeField] private Image totalhealthBar;
    [SerializeField] private Image currenthealthBar;

    private void Start()
    {
        totalhealthBar.fillAmount = player2Health.currentHealth / 10;
    }

    private void Update()
    {
        currenthealthBar.fillAmount = player2Health.currentHealth / 10;
    }
}