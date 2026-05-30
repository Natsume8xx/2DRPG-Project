using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeeChaseState : BaseState
{
    private Attack attack;
    private Vector3 targetPos;
    private Vector3 moveDir;
    private bool isAttack;
    private float attackRateCount;
    public override void OnEnter(Enemy enemy)
    {
        currentEnemy = enemy;
        currentEnemy.currentSpeed = currentEnemy.chaseSpeed;
        attack = currentEnemy.GetComponent<Attack>();
        currentEnemy.lostTimeCount = currentEnemy.lostTime;
        attackRateCount = attack.attackRate;
    }

    public override void LogicUpdated()
    {
        // 丢失玩家且计时器超时
        if (currentEnemy.lostTimeCount <= 0)
        {
            currentEnemy.SwitchState(NPCState.Patrol);
        }

        // 寻找玩家的位置 并且 追击
        targetPos = new Vector3(currentEnemy.target.position.x,currentEnemy.target.position.y + 1.5f,0);

         moveDir = (targetPos - currentEnemy.transform.position).normalized;

        if(moveDir.x >0)
            currentEnemy.transform.localScale = new Vector3(-1,1,1);
        if(moveDir.x <0)
            currentEnemy.transform.localScale = new Vector3(1,1,1);
        if(Mathf.Abs(currentEnemy.transform.position.x - targetPos.x) <= attack.attackRange && Mathf.Abs(currentEnemy.transform.position.y - targetPos.y) <= attack.attackRange)
        {
            //停止追击，准备发起攻击
            isAttack = true;
            if(!currentEnemy.isHurt)
                currentEnemy.rb.velocity = Vector2.zero;
            //计时播放攻击动画
            if(attackRateCount <= 0)
            {
                isAttack = true;
                attackRateCount = attack.attackRate;
                currentEnemy.anim.SetTrigger("attack");
            }
            else
            {
                attackRateCount -= Time.deltaTime;
            }
        }
        else
        {
            isAttack = false;
        }
    }

    public override void PhysicsUpdated()
    {
        if(!currentEnemy.isDead && !currentEnemy.isHurt && currentEnemy.lostTimeCount == currentEnemy.lostTime && !isAttack)
        {
            currentEnemy.rb.velocity = moveDir * currentEnemy.currentSpeed * Time.deltaTime;
        }
    }

    public override void OnExit()
    {
        
    }
}
