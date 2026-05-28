using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class Tina_Orb : MonoBehaviour
{
	private Rigidbody2D rb; // 刚体引用
	private float remainingLife; // 剩余生存时间
	private LayerMask hitMask; // 命中检测层
	private System.Action<Tina_Orb> releaseAction; // 归还到对象池的回调
	private bool released; // 是否已归还
	private float speed; // 光球速度
	public string currentHit;  //光球上一次击中的对象

	// 初始化刚体参数
	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		if (rb != null)
		{
			rb.gravityScale = 0f;
		}
	}

	// 设置归还回调
	public void Initialize(System.Action<Tina_Orb> release)
	{
		releaseAction = release;
	}

	// 发射并初始化参数
	public void Launch(Vector2 direction, float speed, float lifeTime, LayerMask mask)
	{
		released = false;
		remainingLife = lifeTime;
		hitMask = mask;
		this.speed = speed;

		if (rb != null)
		{
			rb.velocity = direction.normalized * speed;
		}
	}

	// 计时到期后自动回收
	void Update()
	{
		remainingLife -= Time.deltaTime;
		if (remainingLife <= 0f)
		{
			Release();
		}
	}

	// 命中目标后回收
	void OnTriggerEnter2D(Collider2D other)
	{
		if(currentHit == other.gameObject.name && (other.CompareTag("Player") || other.CompareTag("Enemy")))
		{
			return;
		}
		Debug.Log("我碰到了玩家");
		// 玩家弹回光球（只有玩家可以弹回光球），且要处理反复检测的问题
		if (other.GetComponent<Attack>() != null && other.CompareTag("Player"))
		{
			currentHit = other.gameObject.name;
			Debug.Log("光球被打了回来");
			// 加速反弹回去
			rb.velocity = new Vector2(-rb.velocity.x*1.5f, 0f); // 保持水平飞行
			return;
		}
		other.GetComponent<Character>()?.TakeDamage(GetComponent<Attack>());
		//Debug.Log("光球碰到了"+other.gameObject.name+"，造成伤害");
		Release();
	}

	// 执行回收逻辑
	private void Release()
	{
		if (released)
		{
			return;
		}

		released = true;
		if (rb != null)
		{
			rb.velocity = Vector2.zero;
		}

		// 用来演示没有使用对象池的情况，直接销毁
		if(releaseAction == null)
		{
			Destroy(gameObject);
			return;
		}

		releaseAction?.Invoke(this);
	}
}
