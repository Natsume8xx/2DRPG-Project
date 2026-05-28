using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nuo_Life : MonoBehaviour, IInteractable
{
    public Animator animator;
    public CapsuleCollider2D capsuleCollider2D;
    public Rigidbody2D rb;
    public DialogueController dialogueController;
    [Header("Nuo 属性")]
    public Vector3 stayPoint;
    public bool isWalk = false;
    public bool isStaying = false;
    public float walkSpeed = 1f;
    public float stayTime = 2f;
    public float currentStayTime = 0f;

    void Awake()
    {
        animator = GetComponent<Animator>();
        capsuleCollider2D = GetComponent<CapsuleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        dialogueController = GetComponent<DialogueController>();
    }

    void Update()
    {
        if (isStaying)
        {
            currentStayTime -= Time.deltaTime;
            if (currentStayTime <= 0)
            {
                isStaying = false;
            }
        }else
        {
            WalkToStayPoint();
        }


    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            StopAndLookAtPlayerForAMoment(collision);
        }
    }

    // 停止并看向玩家一会儿
    void StopAndLookAtPlayerForAMoment(Collider2D player)
    {
        isStaying = true;
        rb.velocity = Vector2.zero;
        transform.localScale = new Vector3(player.transform.position.x > transform.position.x ? 1 : -1, 1, 1);
        animator.SetBool("isWalk", false);
        currentStayTime = stayTime;
    }

    // 走向停留点
    public void WalkToStayPoint()
    {
        if(Vector3.Distance(transform.position,stayPoint) > 0.2f)
        {
            isWalk = true;
            transform.localScale = new Vector3(stayPoint.x > transform.position.x ? 1 : -1, 1, 1);
            //transform.position = Vector3.MoveTowards(transform.position, stayPoint, walkSpeed * Time.deltaTime);
            rb.velocity = new Vector2(transform.localScale.x * walkSpeed, rb.velocity.y);
            animator.SetBool("isWalk", true);
        }
        else
        {
            animator.SetBool("isWalk", false);
            isWalk = false;
        }
    }

    // IInteractable接口实现
    public void TriggerAction()
    {
        dialogueController.StartDialogue();
    }
}
