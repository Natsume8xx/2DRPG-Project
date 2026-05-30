using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnailSkillState : BaseState
{
    public override void OnEnter(Enemy enemy)
    {
        currentEnemy = enemy;
        currentEnemy.currentState = currentEnemy.skillState;
        currentEnemy.currentSpeed = 0;
            currentEnemy.anim.SetTrigger("skill");
        currentEnemy.anim.SetBool("hide",true);
        currentEnemy.lostTimeCount = currentEnemy.lostTime;
        //缩起来时无敌
        currentEnemy.GetComponent<Character>().invulnerable = true;
        currentEnemy.GetComponent<Character>().invulnerableCount = currentEnemy.GetComponent<Character>().invulnerableDuratioin;
    }

    public override void LogicUpdated()
    {
        if(currentEnemy.lostTimeCount <= 0)
        {
            currentEnemy.SwitchState(NPCState.Patrol);
        }
    }

    public override void PhysicsUpdated()
    {
        currentEnemy.GetComponent<Character>().invulnerableCount = currentEnemy.lostTimeCount;
    }

    public override void OnExit()
    {
        currentEnemy.anim.SetBool("hide",false);
    }
}
