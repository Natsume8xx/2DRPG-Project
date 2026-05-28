using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bee : Enemy
{
    [Header("蜜蜂独有参数")]
    public float patrolRaduis;
    public float checkGroundRadius;
    protected override void Awake()
    {
        base.Awake();
        patrolState = new BeePatrolState();
        chaseState = new BeeChaseState();
    }

    public override bool FoundPlayer()
    {
        var obj = Physics2D.OverlapCircle(transform.position,checkDistance,attackLayer);
        if(obj)
            target = obj.transform;
        return obj;
    }

    public override void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position,checkDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position,patrolRaduis);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position,checkGroundRadius);
    }

    public override Vector3 GetNewPoint()
    {
        //TODO:这里其实可以加一个检测，检测如果新的点生成在地面的话，那么就重新生成一个点
        var X = Random.Range(-patrolRaduis,patrolRaduis);
        var Y = Random.Range(-patrolRaduis,patrolRaduis);
        Vector3 newPoint = spwanPoint + new Vector3(X,Y);
        // 生成点在地面则重新生成
        while(Physics2D.OverlapCircle(new Vector2(newPoint.x, newPoint.y), checkGroundRadius, physicsCheck.ground))
        {
            X = Random.Range(-patrolRaduis,patrolRaduis);
            Y = Random.Range(-patrolRaduis,patrolRaduis);
            newPoint = spwanPoint + new Vector3(X,Y);
        }
        //Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(newPoint,checkGroundRadius);
        return newPoint;
    }

    public override void Move()
    {
        
    }
}
