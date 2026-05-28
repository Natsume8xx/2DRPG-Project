using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnailPatrolState : BaseState
{
    public override void OnEnter(Enemy enemy)
    {
        currentEnemy = enemy;
        currentEnemy.currentSpeed = currentEnemy.normalSpeed;
    }

    public override void LogicUpdated()
    {
       // 如果找到玩家，则切换为追击状态
        if (currentEnemy.FoundPlayer())
        {
            currentEnemy.SwitchState(NPCState.Skill);
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
        currentEnemy.anim.SetBool("walk",false);
    }
}
