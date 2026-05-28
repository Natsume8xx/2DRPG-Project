using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoarChaseState : BaseState
{
    public override void OnEnter(Enemy enemy)
    {
        Debug.Log("Enter Chase");
        currentEnemy = enemy;
        currentEnemy.currentSpeed = currentEnemy.chaseSpeed;
        currentEnemy.anim.SetBool("run",true);
        //currentEnemy.lostTimeCount = currentEnemy.lostTime;
    }

    public override void LogicUpdated()
    {
        // 丢失玩家且计时器超时
        if (currentEnemy.lostTimeCount <= 0)
        {
            currentEnemy.SwitchState(NPCState.Patrol);
            //return;
        }
        //碰墙不等待直接冲锋
        if(!currentEnemy.physicsCheck.isGround || (currentEnemy.physicsCheck.touchLeftWall && currentEnemy.faceDir.x<0) || (currentEnemy.physicsCheck.touchRightWall && currentEnemy.faceDir.x>0))
        {
            currentEnemy.transform.localScale = new Vector3(currentEnemy.faceDir.x,1,1);
            //更改左右碰墙的位移差值判定  交换
            if(currentEnemy.faceDir.x == currentEnemy.transform.localScale.x){
                float a ;
                a = currentEnemy.physicsCheck.leftOffset.x ;
                currentEnemy.physicsCheck.leftOffset.x = -currentEnemy.physicsCheck.rightOffset.x;
                currentEnemy.physicsCheck.rightOffset.x = -a;
            }
        }
    }


    public override void PhysicsUpdated()
    {
        
    }

    public override void OnExit()
    {
        //重置计时器
        //currentEnemy.lostTimeCount = currentEnemy.lostTime;
        Debug.Log("Chase Exit");
        currentEnemy.anim.SetBool("run",false);
    }
}
