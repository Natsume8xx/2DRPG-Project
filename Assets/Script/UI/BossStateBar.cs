using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossStateBar : MonoBehaviour
{
    public Image healthGreen;
    public Image healthRed;
    public float redSpeed;

    void Start()
    {
        healthGreen.fillAmount = 1;
        healthRed.fillAmount = 1;
    }

    void Update()
    {
        if(healthGreen.fillAmount <= healthRed.fillAmount)
        {
            healthRed.fillAmount -= redSpeed * Time.deltaTime;
        }
    }

    ///  更新人物血量条
    public void OnHealthChange(float persentage)
    {
        healthGreen.fillAmount = persentage;
    }
}

