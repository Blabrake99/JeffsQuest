using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HealthBar : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] Gradient grad;
    [SerializeField] Image fill;
    int maxHealth;
    private void Start()
    {
        maxHealth = Mathf.RoundToInt(slider.value);
    }
    public void UpdateHealthBar(int value)
    {
        if (value >= 0)
        {
            slider.value = value;
        }
        if (value > 0)
        {
            fill.color = ColorFromGradient(maxHealth / value);
        }
    }
    Color ColorFromGradient(float value)
    {
        return grad.Evaluate(value);
    }
}