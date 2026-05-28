using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D),typeof(PhysicsCheck),typeof(Animator))]
[RequireComponent(typeof(Character))]
public class Enemy : MonoBehaviour
{
    [HideInInspector]public Rigidbody2D rb;
    [HideInInspector]public PhysicsCheck physicsCheck;
    [HideInInspector]public Animator anim;
    [Header("基本参数")]
    public float normalSpeed; 
    public float chaseSpeed;
    public float currentSpeed;
    public Vector3 faceDir;
    public Transform target;  //需要攻击的目标
    public float hurtForce;
    public Vector3 spwanPoint;  //出生点
    [Header("检测")]
    public Vector2 centerOffset;
    public Vector2 checkSize;
    public float checkDistance;
    public LayerMask attackLayer;
    [Header("计时器")]
    public bool isWait;
    public float waitTime;
    public float waitTimeCount;
    public float lostTime;
    public float lostTimeCount;
    [Header("状态")]
    public bool isHurt;
    public bool isDead;
    public BaseState currentState;
    public BaseState patrolState;
    public BaseState chaseState;
    public BaseState skillState;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        physicsCheck = GetComponent<PhysicsCheck>();
        currentSpeed = normalSpeed;
        spwanPoint = transform.position;
        GetComponent<Character>().currentHealthy = GetComponent<Character>().maxHealthy;
    }

    void OnEnable()
    {
        currentState = patrolState;
        patrolState.OnEnter(this);
    }

    public virtual void Update()
    {
        faceDir = new Vector3(-transform.localScale.x,0,0);
        currentState.LogicUpdated();
        TimeCount();
    }

    void FixedUpdate()
    {
        if(!isHurt && !isDead && !isWait)
            Move();
        currentState.PhysicsUpdated();
    }

    void OnDisable()
    {
        currentState.OnExit();
        // 掉落物品的逻辑
        if(GetComponent<LootSpawner>() && isDead == true)
        {
            GetComponent<LootSpawner>().SpawnLoot();
        }
        // 被击杀后更新任务进度
        if(QuestManager.IsInitialized && isDead)
        {
            QuestManager.Instance.SetUpQuestProgress(gameObject.name,1);
        }
    }

    public  virtual void Move()
    {
        if(!anim.GetCurrentAnimatorStateInfo(0).IsName("snailPreMove") && !anim.GetCurrentAnimatorStateInfo(0).IsName("snailHideRecover"))
            rb.velocity = new Vector2(currentSpeed * faceDir.x * Time.deltaTime,rb.velocity.y);
    }

    /// 计时器函数 不止一个计时器
    public virtual void TimeCount()
    {
        // 撞墙等待计时器
        if (isWait)
        {
            rb.velocity = new Vector2(0,rb.velocity.y);
            waitTimeCount -= Time.deltaTime;
            if (waitTimeCount <= 0)
            {
                isWait = false;
                waitTimeCount = waitTime;
                transform.localScale = new Vector3(faceDir.x,1,1);
                //更改左右碰墙的位移差值判定  交换
                if(faceDir.x == transform.localScale.x){
                    float a ;
                    a = physicsCheck.leftOffset.x ;
                    physicsCheck.leftOffset.x = -physicsCheck.rightOffset.x;
                    physicsCheck.rightOffset.x = -a;
                }
            }
        }

        //  丢失玩家锁定计时器
        if (!FoundPlayer() && lostTimeCount>=0)
        {
            if(lostTimeCount >= 0)
            {
                lostTimeCount -= Time.deltaTime;
            }
        }else if (FoundPlayer())
        {
            lostTimeCount = lostTime;
        }
    }

    //是否找到玩家的检测
    public virtual bool FoundPlayer()
    {
        if(isHurt)
            return false;
        var obj =  Physics2D.BoxCast(transform.position+(Vector3)centerOffset,checkSize,0,faceDir,checkDistance,attackLayer);
        if(obj)
            target = obj.transform;
        return obj;
    }

    // 切换状态
    public void SwitchState(NPCState state)
    {
        var changeToState = state switch
        {
            NPCState.Patrol => patrolState,
            NPCState.Chase => chaseState,
            NPCState.Skill => skillState,
            _ => null
        };
        currentState.OnExit();
        currentState = changeToState;
        currentState.OnEnter(this);
    }

#region  事件函数
    /// 受伤函数，被受伤事件订阅
    public virtual void TakeOnDamage(Transform attackerTrans)
    {
        //记录攻击者——玩家
        target = attackerTrans;
        //受击反转
        if(attackerTrans.position.x - transform.position.x > 0)
            transform.localScale = new Vector3(-1,1,1);
        if(attackerTrans.position.x - transform.position.x < 0)
            transform.localScale = new Vector3(1,1,1);
        //更改左右碰墙的位移差值判定  交换
                if(this.faceDir.x == transform.localScale.x){
                    float a ;
                    a = physicsCheck.leftOffset.x ;
                    physicsCheck.leftOffset.x = -physicsCheck.rightOffset.x;
                    physicsCheck.rightOffset.x = -a;
                }
        //播放受击动画
        isHurt = true;
        anim.SetTrigger("hurt");
        //受到向后的冲击力
        Vector2 faceDir = new Vector2(transform.position.x - attackerTrans.position.x,0).normalized;
        // 受击后速度停止
        rb.velocity = new Vector2(0,rb.velocity.y);
        //启动协程
        StartCoroutine(hurtImpulse(faceDir));
        
    }

    //受伤后受到冲击的协程函数
    public virtual IEnumerator hurtImpulse(Vector2 faceDir)
    {
        rb.AddForce(faceDir*hurtForce , ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.45f);
        isHurt = false;
    }

    //死亡函数——播放死亡动画
    public void OnDie()
    {
        anim.SetBool("isDead",true);
        isDead = true;
        gameObject.layer = 2;
    }

    //死亡后销毁
    public void DestoryAfterDie()
    {
        Destroy(this.gameObject);
    }
    #endregion

    public virtual Vector3 GetNewPoint()
    {
        return transform.position;
    }

    public virtual void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position+(Vector3)centerOffset + new Vector3(checkDistance*-transform.localScale.x,0,0),0.2f);
    }
}
