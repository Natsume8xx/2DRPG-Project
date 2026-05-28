using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XInput;

public class Sign : MonoBehaviour
{
    Animator animator;
    public GameObject signSprite;
    public bool canPress;
    public Transform playerTransform;
    private PlayerInputControl playerInputControl;
    public enum devices{
        Keyboard,XboxGamepadMacOS,mobile
    }
    public devices currentDevice;
    private IInteractable signTarget;
    
    void Awake()
    {
        //animator = signSprite.GetComponent<Animator>();
        canPress = false;
        playerInputControl = new PlayerInputControl();
        playerInputControl.Enable();
        playerInputControl.GamePlay.Sign.started += OnConfirm;
    }


    void OnEnable()
    {
        InputSystem.onActionChange += OnActionChange;
        currentDevice = devices.Keyboard;
    }


    void Update()
    {
        signSprite.SetActive(canPress);
        signSprite.transform.localScale = playerTransform.localScale;
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.CompareTag("Interactable") || collision.CompareTag("NPC")) {
            signTarget = collision.GetComponent<IInteractable>();
            canPress = true;
            if(signSprite.activeSelf == false)
                return;
            var anim = signSprite.GetComponent<Animator>();
            // 根据当前输入设备切换交互提示动画
            switch (currentDevice)
            {
                case devices.Keyboard:
                    anim.Play("sign_but_get");
                    break;
                case devices.XboxGamepadMacOS:
                    anim.Play("sign_ps_get");
                    break;
                case devices.mobile:
                    anim.Play("mobilesign");
                    break;
            }
        }
    }


    void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Interactable") || collision.CompareTag("NPC")) {
            canPress = false;
        }
    }

    // 当按下交互键时，触发交互对象的事件
    private void OnConfirm(InputAction.CallbackContext context)
    {
        if(canPress && signTarget != null) {
            signTarget.TriggerAction();
        }
    }

    // 当 检测到输入设备切换时，同时切换交互的动画
    private void OnActionChange(object obj, InputActionChange actionChange)
    {
        if(actionChange == InputActionChange.ActionStarted){
            var device =((InputAction)obj).activeControl.device;
            switch (device)
            {
                case Keyboard:
                    //animator.Play("sign_but_get");
                    currentDevice = devices.Keyboard;
                    break;
                case XboxGamepadMacOS:
                    //animator.Play("sign_ps_get");
                    currentDevice = devices.XboxGamepadMacOS;
                    break;
                default :
                    currentDevice = devices.mobile;
                    break;
            }
        }   
    }

}
