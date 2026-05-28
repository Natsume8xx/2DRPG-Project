using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    private PhysicsCheck physicsCheck;
    private PlayerController playerController;
    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        physicsCheck = GetComponent<PhysicsCheck>();
        playerController = GetComponent<PlayerController>();
    }
    void Update()
    {
        ChangeAnimation();
    }
    public void ChangeAnimation()
    {
        animator.SetFloat("velocityX",Math.Abs(rb.velocity.x));
        animator.SetFloat("velocityY",Math.Abs(rb.velocity.y));
        animator.SetBool("isGround",physicsCheck.isGround);
        animator.SetBool("isCrouch",playerController.isCrouch);
        animator.SetBool("isDead",playerController.isDead);
        animator.SetBool("isAttack",playerController.isAttack);
        animator.SetBool("OnWall",physicsCheck.OnWall);
        animator.SetBool("isSlide",playerController.isSlide);
    }

    //触发玩家受伤动作
    public void PlayerHurt()
    {
        animator.SetTrigger("Hurt");
    }

    //触发玩家攻击动作
    public void PlayerAttack()
    {
        animator.SetTrigger("attack");
    }
}
