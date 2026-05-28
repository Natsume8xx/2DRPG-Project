using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Character : MonoBehaviour,ISaveable
{
    [Header("事件监听")]
    public VoidEventSO newGameEvent;
    [Header("基本属性")]
    public float maxHealthy;
    public float currentHealthy;
    public float maxPower;
    public float currentPower;
    public float slidePowerCost;
    public float powerRecoverSpeed;
    public bool isRecoveryPower;
    public UnityEvent<Transform> OnTakeDamage;
    public UnityEvent OnDie;
    public UnityEvent<Character> OnHealthChange;

    [Header("受击无敌")]
    public bool invulnerable;
    public float invulnerableDuratioin; //无敌时间
    public float invulnerableCount;  //当前的无敌倒计时

    void OnEnable()
    {
        newGameEvent.OnEventRaised += newGame;
        ISaveable saveable = this;
        saveable.RegisterSaveData();
    }
    void OnDisable()
    {
        newGameEvent.OnEventRaised -= newGame;
        ISaveable saveable = this;
        saveable.UnRegisterSaveData();
    }
    
    void Update()
    {
        if (invulnerable)
        {
            invulnerableCount -= Time.deltaTime;
            if(invulnerableCount <= 0)
            {
                invulnerable = false;
            }
        }
        if(currentPower < maxPower)
        {
            isRecoveryPower = true;
            currentPower += powerRecoverSpeed * Time.deltaTime;
            OnHealthChange?.Invoke(this);
        }
    }

    // 新游戏时重置数值
    void newGame()
    {
        Debug.Log("new game");
        currentHealthy = maxHealthy;
        currentPower = maxPower;
        invulnerable = false;
        OnHealthChange?.Invoke(this);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Water"))
        {
            if(currentHealthy>0){
                // 在水中直接死亡
                currentHealthy = 0;
                OnHealthChange?.Invoke(this);
                OnDie?.Invoke();
            }
        }
    }

    // 受伤
    public void TakeDamage(Attack attacker)
    {
        if(invulnerable)
            return;
        if(currentHealthy - attacker.damage >0){
            currentHealthy -= attacker.damage;
            TriggerInvulnerable();
            //  执行受伤动作 触发受伤事件
            OnTakeDamage?.Invoke(attacker.transform);
        }
        else
        {
            currentHealthy = 0;
            //执行死亡
            OnDie?.Invoke();
        }
        OnHealthChange?.Invoke(this);
    }

    //无敌触发器
    public void TriggerInvulnerable()
    {
        if (!invulnerable)
        {
            invulnerable = true;
            invulnerableCount = invulnerableDuratioin;  //重置计时器
        }
    }

    //在滑铲时消耗能量
    public void OnSlide()
    {
        currentPower -= slidePowerCost;
        OnHealthChange?.Invoke(this);
    }
#region 消耗物品进行各种回复效果
    public void HPRecover(float amount)
    {
        currentHealthy = Mathf.Min(currentHealthy + amount, maxHealthy);
        OnHealthChange?.Invoke(this);
    }
#endregion

#region 实现ISaveable接口，保存和加载数据
    public DataDefination GetDataID()
    {
        return GetComponent<DataDefination>();
    }

    //保存当前的数据
    public void GetSaveData(Data data)
    {
        Debug.Log($"data is null? {data == null}");
        if(data.characterPosDic.ContainsKey(GetDataID().ID))
        {
            data.characterPosDic[GetDataID().ID] = new SerilizeVector3(this.transform.position);
            data.floatDataDic[GetDataID().ID+"Health"] = this.currentHealthy;
            data.floatDataDic[GetDataID().ID+"Power"] = this.currentPower;
        }
        else
        {
            data.characterPosDic.Add(GetDataID().ID,new(this.transform.position));
            data.floatDataDic.Add(GetDataID().ID+"Health",this.currentHealthy);
            data.floatDataDic.Add(GetDataID().ID+"Power",this.currentPower);
        }
    }

    //加载数据
    public void LoadSaveData(Data data)
    {
        if(data.characterPosDic.ContainsKey(GetDataID().ID))
        {
            this.transform.position = data.characterPosDic[GetDataID().ID].returnVector3();
            this.currentHealthy = data.floatDataDic[GetDataID().ID+"Health"];
            this.currentPower = data.floatDataDic[GetDataID().ID+"Power"];

            //通知UI进行更新
            OnHealthChange?.Invoke(this);
        }
    }
#endregion
}


