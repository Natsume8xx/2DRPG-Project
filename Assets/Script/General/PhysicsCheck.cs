using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsCheck : MonoBehaviour
{
    CapsuleCollider2D coll;
    PlayerController playerController;
    Rigidbody2D rb;
    [Header("检测参数参数")]
    public bool manual;    //是否手动设置检测点位置
    public float checkRadius;
    public LayerMask ground;
    public Vector2 bottomOffset;  //脚底中心位移误差
    public Vector2 leftOffset;
    public Vector2 rightOffset;
    [Header("状态参数")]
    public bool isPlayer;
    public bool isGround;
    public bool touchLeftWall;
    public bool touchRightWall;
    public bool OnWall;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if(isPlayer)
            playerController = GetComponent<PlayerController>();
        coll = GetComponent<CapsuleCollider2D>();
        if (!manual) 
        {
            //自动计算设置检测点位置
            rightOffset = new Vector2(coll.offset.x+coll.bounds.size.x/2,coll.bounds.size.y/2);
            leftOffset = new Vector2(coll.offset.x - coll.bounds.size.x/2,coll.bounds.size.y/2);
        }
    }
    void Update()
    {
        Check();
    }

    /// 通用物理检测的函数
    public void Check()
    {
        //地面检测
        if(OnWall)
            isGround = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(bottomOffset.x * transform.localScale.x, bottomOffset.y), checkRadius*1.1f, ground);
        else
        isGround = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(bottomOffset.x * transform.localScale.x, 0), checkRadius, ground);
        //墙体检测
        touchLeftWall = Physics2D.OverlapCircle((Vector2)transform.position+new Vector2(leftOffset.x,leftOffset.y),checkRadius,ground);
        touchRightWall = Physics2D.OverlapCircle((Vector2)transform.position+new Vector2(rightOffset.x,rightOffset.y),checkRadius,ground);
        //玩家的上墙检测
        if(isPlayer)
            OnWall = (touchLeftWall && playerController.inputDirection.x<0f || touchRightWall && playerController.inputDirection.x>0f) && rb.velocity.y<0f;
    }

    /// 在场景中绘制检测线
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere((Vector2)transform.position+new Vector2(bottomOffset.x * transform.localScale.x,bottomOffset.y),checkRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position+new Vector2(leftOffset.x,leftOffset.y),checkRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position+new Vector2(rightOffset.x,rightOffset.y),checkRadius);
    }
}
