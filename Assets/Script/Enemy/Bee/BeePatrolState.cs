using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeePatrolState : BaseState
{
    private Vector3 targetPos;
    private Vector3 moveDir;
    public override void OnEnter(Enemy enemy)
    {
        currentEnemy = enemy;
        currentEnemy.currentSpeed = currentEnemy.normalSpeed;
        targetPos = currentEnemy.GetNewPoint();
    }

    public override void LogicUpdated()
    {
        //找到玩家
        if (currentEnemy.FoundPlayer())
        {
            currentEnemy.SwitchState(NPCState.Chase);
        }

        //继续巡逻，检测是否到达巡逻点
        if((Mathf.Abs(targetPos.x-currentEnemy.transform.position.x) < 0.1f) && (Mathf.Abs(targetPos.y-currentEnemy.transform.position.y) < 0.1f))
        {
            currentEnemy.isWait = true;
            targetPos = currentEnemy.GetNewPoint();
            
        }

        moveDir = (targetPos - currentEnemy.transform.position).normalized;

        if(moveDir.x >0)
            currentEnemy.transform.localScale = new Vector3(-1,1,1);
        if(moveDir.x <0)
            currentEnemy.transform.localScale = new Vector3(1,1,1);
    }

    public override void PhysicsUpdated()
    {
       if(!currentEnemy.isDead && !currentEnemy.isHurt && !currentEnemy.isWait)
        {
            currentEnemy.rb.velocity = moveDir * currentEnemy.currentSpeed * Time.deltaTime;
        }
        else
        {
            currentEnemy.rb.velocity = Vector2.zero;
        }
    }

    public override void OnExit()
    {
        
    }

}
