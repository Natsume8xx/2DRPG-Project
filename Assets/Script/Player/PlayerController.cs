using System;
using System.Collections;
using System.Collections.Generic;
//using System.Numerics;
//using System;
//using System.Collections;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    Character character;
    Animator anim;
    public PlayerInputControl inputControl;      //操作集
    public Vector2 inputDirection;         //输入方向
    private Rigidbody2D rb;
    private CapsuleCollider2D coll;   //碰撞体组件
    private PhysicsCheck physicsCheck;
    private PlayerAnimation playerAnimation;
    [Header("事件监听")]
    public SceneLoadEventSO sceneLoadEventSO;  //场景加载事件
    public VoidEventSO afterSceneLoadEventSO;  //场景加载完成事件
    public VoidEventSO loadDataEventSO;  //数据加载事件
    public VoidEventSO newGameEventSO;   //新游戏事件

    [Header("基本参数")]
    public float jumpforce;
    public float speed ;
    private float runSpeed;
    private float walkSpeed => speed / 2.5f;
    private Vector2 origionOffset;
    private Vector2 origionSize;
    public float hurtForce;
    public float wallJumpForce;
    public float slideDistance;
    public float slideSpeed;
    [Header("物理材质")]
    public PhysicsMaterial2D Wall;
    public PhysicsMaterial2D Normal;
    [Header("状态")]
    public bool isCrouch;
    public bool isHurt;
    public bool isDead;
    public bool isAttack;
    public bool isWallJump;
    public bool isSlide;

    private void Awake()
    {
        character = GetComponent<Character>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        physicsCheck = GetComponent<PhysicsCheck>();
        playerAnimation = GetComponent<PlayerAnimation>();
        inputControl = new PlayerInputControl();
        //跳跃
        inputControl.GamePlay.Jump.started += Jump;
        coll = GetComponent<CapsuleCollider2D>();
        origionOffset = coll.offset;
        origionSize = coll.size;
        //行走
        #region 通过按键控制行走
        runSpeed = speed;
        inputControl.GamePlay.Walk.performed += ctx =>
        {
            if(physicsCheck.isGround)
                speed = walkSpeed; 
        };
        inputControl.GamePlay.Walk.canceled += ctx =>
        {
            //if(physicsCheck.isGround)  注释后可以防止空中切换的bug
            speed = runSpeed; 
        };
        #endregion
        //攻击
        inputControl.GamePlay.Attack.started += Attack;
        //滑行
        inputControl.GamePlay.Slide.started += Slide;
    }


    private void OnEnable()
    {
        inputControl.Enable();
        sceneLoadEventSO.LoadRequestEvent += OnLoadRequestEvent;
        afterSceneLoadEventSO.OnEventRaised += OnAfterSceneLoad;
        loadDataEventSO.OnEventRaised += OnloadDataEvent;
        newGameEventSO.OnEventRaised += OnNewGameEvent;
    }


    private void OnDisable()
    {
        inputControl.Disable();
        sceneLoadEventSO.LoadRequestEvent -= OnLoadRequestEvent;
        afterSceneLoadEventSO.OnEventRaised -= OnAfterSceneLoad;
        loadDataEventSO.OnEventRaised += OnloadDataEvent;
        newGameEventSO.OnEventRaised -= OnNewGameEvent;
    }


    private void Update()
    {
        inputDirection = inputControl.GamePlay.Move.ReadValue<Vector2>();   //读取输入值
        CheckState();
    }

    private void FixedUpdate()
    {
        if(!isHurt && !isAttack)
            Move();
    }
    //主界面中重新开始游戏的事件响应函数
    private void OnNewGameEvent()
    {
        character.currentHealthy = character.maxHealthy;
        character.currentPower = character.maxPower;
        character.OnHealthChange?.Invoke(character);
        isDead = false;
    }
    //数据加载事件的响应函数  读取游戏进度
    private void OnloadDataEvent()
    {
        isDead = false;
    }

    //加载场景时关闭玩家输入
    public void OnLoadRequestEvent(GameSceneSO loadScene, Vector3 posToGo, bool fadeScreen)  //场景加载事件响应函数
    {
        inputControl.GamePlay.Disable();  //加载场景时禁止玩家输入
    }
    //场景加载完成后启用玩家输入
    public void OnAfterSceneLoad()  //场景加载完成事件响应函数
    {
        inputControl.GamePlay.Enable();  //加载完成后恢复玩家输入
    }


    /// 移动函数，被FixedUpdate函数调用
    public void Move()
    {
        //人物移动
        if(!isCrouch && !isWallJump)
            rb.velocity = new Vector2(inputDirection.x * speed * Time.deltaTime,rb.velocity.y);
        //人物翻转
        int change = inputDirection.x*transform.localScale.x<0? -1 : 1;
        transform.localScale = new Vector3(transform.localScale.x * change,1,1); 
        //人物蹲下
        isCrouch = inputDirection.y<-0.5f && physicsCheck.isGround;
        if (isCrouch)
        {
            rb.velocity = new Vector2(0,rb.velocity.y);   //修复移动中按下下蹲并松开移动时出现的持续移动bug
            //更改碰撞体的大小
            coll.offset = new Vector2(-0.09f,0.84f);
            coll.size = new Vector2(0.8f,1.68f);
        }  //1.03 2.06
        else
        {
            coll.offset = new Vector2(origionOffset.x,origionOffset.y);
            coll.size = new Vector2(origionSize.x,origionSize.y);
        }
    }
    

    /// 跳跃函数
    private void Jump(InputAction.CallbackContext obj)
    {
        if(physicsCheck.isGround ){
            //播放跳跃音效
            this.GetComponent<AudioDefination>().PlayAudio();
            if (isSlide)
            {
                StopCoroutine("SlideMove");
                isSlide = false;
                character.invulnerable = false;  //取消滑铲无敌
            }
                
            rb.AddForce(transform.up * jumpforce , ForceMode2D.Impulse);
        }else if(physicsCheck.OnWall)
        {
            isWallJump = true;
            rb.AddForce(new Vector2(-inputDirection.x,2f) * wallJumpForce, ForceMode2D.Impulse);
        }
    }

    /// 攻击函数
    private void Attack(InputAction.CallbackContext obj)
    {
        if(isSlide || isHurt || physicsCheck.OnWall || isWallJump) return;
        isAttack = true;
        playerAnimation.PlayerAttack();
    }


    //滑铲相关函数
    private void Slide(InputAction.CallbackContext context)
    {
        if (!isSlide && physicsCheck.isGround)
        {
            if(character.currentPower < character.slidePowerCost)
                return;
            isSlide = true;
            character.invulnerable = true;  //滑铲时无敌
            character.invulnerableCount = 0.5f;  //滑铲无敌计时 1f;
            var target = new Vector3(transform.position.x + slideDistance*transform.localScale.x,transform.position.y);
            StartCoroutine(SlideMove(target));
            character.OnSlide();  //调用消耗能量的函数
        }
    }

    // 滑铲移动的协程
    public IEnumerator SlideMove(Vector3 target)
    {
        do
        {
            inputControl.GamePlay.Move.Disable();  //滑铲时禁止移动输入
            yield return null;
            if(!physicsCheck.isGround){
                break;
            }
            if(physicsCheck.touchLeftWall && transform.localScale.x <0f || physicsCheck.touchRightWall && transform.localScale.x >0f){
                break;
            }
            //var target = new Vector3(transform.position.x + slideDistance*transform.localScale.x,transform.position.y);
            //rb.MovePosition(target * slideSpeed*Time.deltaTime);
            if(anim.GetCurrentAnimatorStateInfo(3).IsName("blueSlide2"))
                rb.MovePosition(new Vector3(transform.position.x + transform.localScale.x*slideSpeed,transform.position.y));
        }while(Mathf.Abs(transform.position.x - target.x) > 0.1f);
        isSlide = false;
        inputControl.GamePlay.Move.Enable();  //恢复移动输入
    }


    // 在墙上的时候，改变物理材质，增加摩擦力，防止滑落 
    // 同时限制下落速度，防止过快滑落
    public void CheckState()
    {
        coll.sharedMaterial = physicsCheck.isGround? Normal : Wall;
        if(physicsCheck.OnWall)
        {
            //isWallJump = true;
            rb.velocity = new Vector2(rb.velocity.x,Mathf.Clamp(rb.velocity.y,-0.5f,float.MaxValue));
        }
        if(isWallJump && rb.velocity.y<5f)
        {
            isWallJump = false;
        }
    }

#region 事件方法
    //受伤后击飞
    public void getHurt(Transform attacker)
    {
        isHurt = true;
        //停下当前移动
        rb.velocity = Vector2.zero;
        //获取击飞方向
        Vector2 Dir = new Vector2(transform.position.x - attacker.position.x,0).normalized;
        rb.AddForce(Dir * hurtForce,ForceMode2D.Impulse);
    }

    //死亡事件
    public void PlayerDead()
    {
        isDead = true;
        //禁止移动
        inputControl.GamePlay.Disable();
    }
#endregion
}
