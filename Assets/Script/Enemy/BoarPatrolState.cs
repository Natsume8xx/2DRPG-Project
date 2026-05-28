using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoarPatrolState : BaseState
{
    public override void OnEnter(Enemy enemy)
    {
        Debug.Log("Patrol Enter");
        currentEnemy = enemy;
        currentEnemy.currentSpeed = currentEnemy.normalSpeed;
    }

    public override void LogicUpdated()
    {
        // 如果找到玩家，则切换为追击状态
        if (currentEnemy.FoundPlayer())
        {
            // if(!currentEnemy.isHurt)
            currentEnemy.SwitchState(NPCState.Chase);
            //return;
        }
        // 检测撞墙
        if(!currentEnemy.physicsCheck.isGround || (currentEnemy.physicsCheck.touchLeftWall && currentEnemy.faceDir.x<0) || (currentEnemy.physicsCheck.touchRightWall && currentEnemy.faceDir.x>0))
        {
            currentEnemy.isWait = true;
            currentEnemy.anim.SetBool("walk",false);
        }
        else
        {
            currentEnemy.anim.SetBool("walk",true);
        }
    }

    public override void PhysicsUpdated()
    {
        
    }

    public override void OnExit()
    {
        Debug.Log("Patrol Exit");
        currentEnemy.anim.SetBool("walk",false);
    }

}
