using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class Tina_Life : Enemy
{
	[Header("Boss Combat")]
	[SerializeField] private float meleeRange = 1.2f; // 近战攻击范围
	[SerializeField] private Vector2 meleeCenterOffset = new Vector2(0.6f, 0f); // 近战判定中心偏移
	[SerializeField] private float meleeHitDelay = 0.2f; // 近战出手到命中的延迟
	[SerializeField] private float meleeCooldown = 1.2f; // 近战冷却
	[SerializeField] private string meleeTrigger = "attack"; // 近战动画触发器
	public bool isAttacking; // 是否正在攻击
	public bool isWaliking; // 是否正在移动

	[Header("Ranged Skill")]
	[SerializeField] private float skillRange = 6f; // 远程技能最大释放距离
	[SerializeField] private float skillCooldown = 6f; // 远程技能冷却
	[SerializeField] private int waveCount = 3; // 光球波数
	[SerializeField] private int orbPerWave = 3; // 每波光球数量
	[SerializeField] private float waveInterval = 0.8f; // 波与波之间的间隔
	[SerializeField] private int maxOrbsAlive = 15; // 场上光球上限
	[SerializeField] private float waveSpreadAngle = 18f; // 扇形散射角度
	[SerializeField] private string skillTrigger = "skill"; // 技能动画触发器

	[Header("光球对象池")]
	[SerializeField] private GameObject orbPrefab; // 光球预制体
	[SerializeField] private Transform firePoint; // 发射点（可为空）
	[SerializeField] private float firePointForwardOffset = 0.6f; // 无发射点时的前方偏移距离
	[SerializeField] private float orbSpeed = 6f; // 光球速度
	[SerializeField] private float orbLifetime = 3f; // 光球生存时间
	[SerializeField] private int orbDamage = 1; // 光球伤害
	[SerializeField] private int poolDefaultCapacity = 15; // 池默认容量
	[SerializeField] private int poolMaxSize = 30; // 池最大容量
	[SerializeField] private int poolPrewarm = 10; // 预热数量

	[Header("静默")]
	[SerializeField] private float silenceAfterAttack = 0.5f; // 攻击后静默时间

	private ObjectPool<GameObject> orbPool; // 光球对象池
	private Attack meleeAttack; // 近战攻击组件引用
	private int activeOrbs; // 当前场上光球数量
	private float nextMeleeTime; // 下一次近战允许时间
	private float nextSkillTime; // 下一次技能允许时间
	private Coroutine bossLoop; // Boss 主循环协程句柄
	private BaseState nullState; // 空状态，避免状态机逻辑影响
	[Header("组件")]
	public BossStateBar bossStateBar; // Boss 血条 UI 引用
	public GameObject GameEndPanel; // 游戏结束界面引用
	private Character character; // 角色组件引用 

	// 初始化组件引用
	protected override void Awake()
	{
		base.Awake();
		meleeAttack = GetComponent<Attack>();
		character = GetComponent<Character>();
		nullState = new NullState();
		patrolState = nullState;
		chaseState = nullState;
		skillState = nullState;
		currentState = nullState;
        //热更新部分：有两个函数可以选择，调用一个函数，来给配置参数进行赋值。
		show();
	}

	void OnEnable()
	{
		character.OnHealthChange.AddListener(OnHealthChange);
		character.OnDie.AddListener(OnDead);
	}

    // 启动对象池与主循环
    void Start()
	{
		if (orbPrefab != null)
		{
			CreatePool();
		}

		bossLoop = StartCoroutine(BossLoop());

        //找到玩家
        target = GameObject.FindGameObjectWithTag("Player")?.transform;
		
	}

	public override void Update()
    {
        faceDir = new Vector3(transform.localScale.x,0,0);
        TimeCount();
    }

    // 销毁时清理对象池
    void OnDestroy()
	{
		if (orbPool != null)
		{
			orbPool.Clear();
		}
	}

	// 生命值改变事件的注册方法
	private void OnHealthChange(Character character)
    {
		float healthPercentage = character.currentHealthy / character.maxHealthy;
		bossStateBar?.OnHealthChange(healthPercentage);
		if(character.currentHealthy > 0)
			StartCoroutine(PlayHurtAnimation());
    }
	// 播放受伤动画的协程
    private IEnumerator PlayHurtAnimation()
    {
		// 播放受击动画
        isHurt = true;
		anim.SetTrigger("hurt");
		yield return new WaitForSeconds(0.45f); // 受击动画持续时间
		isHurt = false;
    }

	// 死亡事件的注册方法
	private void OnDead()
	{
		bossStateBar?.OnHealthChange(0f);
		anim.SetBool("isDead", true);
		StartCoroutine(PlayDieAnimation());
	}

	// 播放死亡动画的协程
	IEnumerator PlayDieAnimation()
	{
		yield return new WaitForSeconds(1.3f); // 死亡动画持续时间
		isDead = true;
		GameEndPanel.SetActive(true);
		Destroy(this.gameObject);	
	}


    #region 对象池相关回调函数
    // 创建并预热光球对象池
    private void CreatePool()
	{
		orbPool = new ObjectPool<GameObject>(
			CreateOrb,
			OnGetOrb,
			OnReleaseOrb,
			OnDestroyOrb,
			collectionCheck: false,
			defaultCapacity: poolDefaultCapacity,
			maxSize: poolMaxSize
		);
        // 预热对象池
		int warmCount = Mathf.Min(poolPrewarm, poolMaxSize);
		for (int i = 0; i < warmCount; i++)
		{
			GameObject orb = orbPool.Get();
			orbPool.Release(orb);
		}
	}

	// 生成单个光球实例
	private GameObject CreateOrb()
	{
		GameObject orb = Instantiate(orbPrefab, transform);
		if (!orb.TryGetComponent(out Tina_Orb orbLogic))
		{
			orbLogic = orb.AddComponent<Tina_Orb>();
		}

		orbLogic.Initialize(ReleaseOrb);
        // 确保光球有攻击组件以处理伤害逻辑
		if (!orb.TryGetComponent(out Attack attack))
		{
			attack = orb.AddComponent<Attack>();
		}
		attack.damage = orbDamage;
		orb.SetActive(false);
		return orb;
	}

	// 从池中取出光球时的回调
	private void OnGetOrb(GameObject orb)
	{
		activeOrbs++;
		orb.SetActive(true);
	}

	// 归还光球到池时的回调
	private void OnReleaseOrb(GameObject orb)
	{
		activeOrbs = Mathf.Max(0, activeOrbs - 1);
		orb.SetActive(false);
	}

	// 池销毁光球时的回调
	private void OnDestroyOrb(GameObject orb)
	{
		Destroy(orb);
	}

	// 光球生命周期结束时的归还逻辑
	private void ReleaseOrb(Tina_Orb orb)
	{
		if (orbPool == null)
		{
			return;
		}

		orbPool.Release(orb.gameObject);
	}
#endregion


	// Boss 行为主循环：根据距离与冷却选择出招
	private IEnumerator BossLoop()
	{
		while(target == null)
		{
			target = GameObject.FindGameObjectWithTag("Player")?.transform;
			yield return null;
		}
		while (true)
		{
			if (isDead)
			{
				Debug.Log("Boss已死亡，停止循环");
				yield break;
			}

			if (isHurt)
			{
				Debug.Log("受伤中，等待...");
				isWaliking = false;
				anim.SetBool("isWalk", isWaliking);
				yield return null;
				continue;
			}

			if (!FoundPlayer())
			{
				Debug.Log("未找到玩家，等待...");
				isWaliking = false;
				anim.SetBool("isWalk", isWaliking);
				yield return null;
				continue;
			}
            if(!isHurt && !isDead && !isWait && !isAttacking)
            {
				//Debug.Log("满足条件，可以走路");
                Move();
            }else
            {
				//Debug.Log("未满足条件，不能走路");
                anim.SetBool("isWalk", false);
            }

			float distance = Vector2.Distance(transform.position, target.position);

			if (distance <= meleeRange && Time.time >= nextMeleeTime)
			{
				Debug.Log("近战攻击");
				yield return StartCoroutine(DoMeleeAttack());
			}
			else if (distance <= skillRange && Time.time >= nextSkillTime && orbPool != null)
			{
				Debug.Log("远程技能攻击");
				yield return StartCoroutine(DoRangedAttack());
			}

			yield return null;
		}
	}

	// 执行近战攻击
	private IEnumerator DoMeleeAttack()
	{
		isWaliking = false;
		anim.SetBool("isWalk", isWaliking);
		isAttacking = true;
		nextMeleeTime = Time.time + meleeCooldown;
		float totalSilence = meleeHitDelay + silenceAfterAttack;
		EnterSilence(totalSilence);

		if (!string.IsNullOrEmpty(meleeTrigger))
		{
			anim.SetTrigger(meleeTrigger);
		}

		if (meleeHitDelay > 0f)
		{
			yield return new WaitForSeconds(meleeHitDelay);
		}

		DealMeleeDamage();

		if (silenceAfterAttack > 0f)
		{
			yield return new WaitForSeconds(silenceAfterAttack);
		}
		isAttacking = false;
	}

	// 近战伤害判定
	private void DealMeleeDamage()
	{
		if (meleeAttack == null)
		{
			return;
		}

		Vector2 origin = (Vector2)transform.position + meleeCenterOffset * new Vector2(faceDir.x, 1f);
		Collider2D hit = Physics2D.OverlapCircle(origin, meleeRange, attackLayer);
		if (hit == null)
		{
			return;
		}

		hit.GetComponent<Character>()?.TakeDamage(meleeAttack);
	}

	// 执行远程技能攻击
	private IEnumerator DoRangedAttack()
	{
		isWaliking = false;
		anim.SetBool("isWalk", isWaliking);
		isAttacking = true;
		nextSkillTime = Time.time + skillCooldown;
		float totalSilence = waveCount * waveInterval + silenceAfterAttack;
		EnterSilence(totalSilence);

		if (!string.IsNullOrEmpty(skillTrigger))
		{
			anim.SetTrigger(skillTrigger);
			yield return new WaitForSeconds(0.7f);  // 配合动画出手时机
		}

		for (int wave = 0; wave < waveCount; wave++)
		{
			if (isDead)
			{
				yield break;
			}

			SpawnWave();
			if (waveInterval > 0f)
			{
				yield return new WaitForSeconds(waveInterval);
			}
		}

		if (silenceAfterAttack > 0f)
		{
			yield return new WaitForSeconds(silenceAfterAttack);
		}
		isAttacking = false;
	}

	// 生成一波光球
	private void SpawnWave()
	{
		if (orbPool == null || target == null)
		{
			return;
		}

		int available = Mathf.Max(0, maxOrbsAlive - activeOrbs);
		int spawnCount = Mathf.Min(orbPerWave, available);
		if (spawnCount <= 0)
		{
			return;
		}

		Vector3 origin = firePoint != null
			? firePoint.position
			: transform.position + new Vector3(faceDir.x, 5.5f, 0f) * firePointForwardOffset;
		Vector2 baseDir = (target.position - origin).normalized;

		float startAngle = spawnCount > 1 ? -waveSpreadAngle * 0.5f : 0f;
		float step = spawnCount > 1 ? waveSpreadAngle / (spawnCount - 1) : 0f;

		for (int i = 0; i < spawnCount; i++)
		{
			float angle = startAngle + step * i;
			Vector2 dir = Quaternion.Euler(0f, 0f, angle) * baseDir;
			SpawnOrb(origin, dir);
		}
	}

	// 生成单个光球并发射（使用对象池）
	private void SpawnOrb(Vector3 origin, Vector2 direction)
	{
		if (orbPool == null)
		{
			return;
		}

		GameObject orb = orbPool.Get();
		if (!orb.TryGetComponent(out Tina_Orb orbLogic))
		{
			orbPool.Release(orb);
			return;
		}

		orb.transform.position = origin;
		// 测试打断动态批处理
		//orb.GetComponent<SpriteRenderer>().material = new Material(orb.GetComponent<SpriteRenderer>().material);
		orbLogic.Launch(direction, orbSpeed, orbLifetime, attackLayer);
	}

	// 生成单个光球并发射 , 不使用对象池的版本
	private void SpawnOrb2(Vector3 origin, Vector2 direction)
	{
		GameObject orb = Instantiate(orbPrefab);
		// 测试打断动态批处理
		//orb.GetComponent<SpriteRenderer>().material = new Material(orb.GetComponent<SpriteRenderer>().material);
		orb.transform.position = origin;
		orb.GetComponent<Tina_Orb>()?.Launch(direction, orbSpeed, orbLifetime, attackLayer);
	}

	// 进入静默状态，暂时停止移动/行为
	private void EnterSilence(float duration)
	{
		if (duration <= 0f)
		{
			return;
		}
		//进入静默状态
		Debug.Log($"进入静默状态，持续 {duration} 秒");
		isWait = true;
		waitTimeCount = duration;
	}

	//重写寻找玩家函数
	public override bool FoundPlayer(){
        return target!= null;
    }  

	// 重写移动：不依赖状态机，直接根据玩家位置追击或停留
	public override void Move()
	{
		if (anim.GetCurrentAnimatorStateInfo(0).IsName("snailPreMove") || anim.GetCurrentAnimatorStateInfo(0).IsName("snailHideRecover"))
		{
			return;
		}

        // 播放移动动画
		isWaliking = true;
        anim.SetBool("isWalk", isWaliking);
		bool hasTarget = FoundPlayer() && target != null;
		if (!hasTarget)
		{
			Debug.Log("未找到玩家，停止移动");
			currentSpeed = normalSpeed;
			rb.velocity = new Vector2(0f, rb.velocity.y);
			isWaliking = false;
			anim.SetBool("isWalk", isWaliking);
			return;
		}

		float dir = Mathf.Sign(target.position.x - transform.position.x)*0.7f;
		if (dir != 0f)
		{
			transform.localScale = new Vector3(dir, 0.7f, 0.7f);
		}
		currentSpeed = chaseSpeed;
		rb.velocity = new Vector2(currentSpeed * faceDir.x * Time.deltaTime, rb.velocity.y);
	}

	/// 重写计时器函数 不止一个计时器
    public override void TimeCount()
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

	//重写辅助划线函数
	public override void OnDrawGizmosSelected()
    {
		// 绘制近战攻击范围
		Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
		// 绘制远程技能范围
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(transform.position, skillRange);
    }


	// 用于面试展示时快速修改参数的函数(正常版)
	public void show()
	{
		waveCount = 3; // 光球波数
		orbPerWave = 3; // 每波光球数量
		waveInterval = 0.8f; // 波与波之间的间隔
		maxOrbsAlive = 15; // 场上光球上限
		waveSpreadAngle = 18f; // 扇形散射角度
		orbSpeed = 6f; // 光球速度
		orbLifetime = 3f; // 光球生存时间
		orbDamage = 1; // 光球伤害
		poolDefaultCapacity = 15; // 池默认容量
		poolMaxSize = 30; // 池最大容量
		poolPrewarm = 10; // 预热数量
	}
	// 用于面试展示时快速修改参数的函数(修改版，展示对象池)
	public void show2()
	{
		waveCount = 10; // 光球波数
		orbPerWave = 90; // 每波光球数量
		waveInterval = 1f; // 波与波之间的间隔
		maxOrbsAlive = 90; // 场上光球上限
		waveSpreadAngle = 50f; // 扇形散射角度
		orbSpeed = 10f; // 光球速度
		orbLifetime = 0.7f; // 光球生存时间
		orbDamage = 1; // 光球伤害
		poolDefaultCapacity = 200; // 池默认容量
		poolMaxSize = 250; // 池最大容量
		poolPrewarm = 90; // 预热数量
	}
	// 用于面试展示时快速修改参数的函数(疯狂版，展示动态批处理)
	public void show3()
	{
		waveCount = 1; // 光球波数
		orbPerWave = 900; // 每波光球数量
		waveInterval = 0.8f; // 波与波之间的间隔
		maxOrbsAlive = 1000; // 场上光球上限
		waveSpreadAngle = 50f; // 扇形散射角度
		orbSpeed = 6f; // 光球速度
		orbLifetime = 3f; // 光球生存时间
		orbDamage = 1; // 光球伤害
		poolDefaultCapacity = 1000; // 池默认容量
		poolMaxSize = 1000; // 池最大容量
		poolPrewarm = 900; // 预热数量
	}
}

// 空状态：确保基类状态机调用不会影响 Boss 行为
internal class NullState : BaseState
{
	public override void OnEnter(Enemy enemy)
	{
	}

	public override void LogicUpdated()
	{
	}

	public override void PhysicsUpdated()
	{
	}

	public override void OnExit()
	{
	}
}

