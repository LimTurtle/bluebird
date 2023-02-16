﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf.Protocol;
using static Define;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;


//PlayerMove 패킷 핸들러로부터 playerinfo 및 State 정보를 최신화하여 현재 상태와 비교한다. 이를 통해, playerd의 위치 및 애니메이션을 변경한다.

public class PlayerController : MonoBehaviour
{
    public Int64 playerId { get; set; }
    [SerializeField]
    public float speed = 10.0f;
    public float jumpPower = 5.0f;

    protected Vector3 moveVec;
    protected Vector3 prevVec;

    protected Camera cam;

    protected bool pressedJump = false;
    protected bool isJumping = false;
    protected bool isSliding = false;

    protected Animator animator;

    protected Rigidbody rigid;
    protected Google.Protobuf.Protocol.PlayerState playerState;

    protected int clearStageNum = 0;

    [SerializeField]
    protected BirdState _state;

    public virtual BirdState State
    {
        get { return _state; }
        set
        {
            if (_state == value)
                return;

            _state = value;
          
        }
    }

    Player _playerInfo = new Player();
    Queue<Move> moves = new Queue<Move>();
    public Player playerInfo
    {
        get { return _playerInfo; }
        set
        {
            if (_playerInfo.Equals(value))
                return;
            _playerInfo = value;
        }
    }
    
    public Move Moves
    {
        set
        {
            StartCoroutine(MoveSync(0.1f, value));
            
            moves.Enqueue(value);
        }
        get 
        { 
            if (moves.Count == 0)
            {
                return null;
            }
            return moves.Dequeue(); 
        }
    }

    void Start()
    {
        Init();
    }

    void Update()
    {
        
        UpdateController();

    }

    public IEnumerator MoveSync(float time, Move data)
    {
        yield return new WaitForSeconds(time);
        playerInfo.Position = data.Position;
        playerInfo.Rotation = data.Rotation;
        SetAnim(data.State);
    }

    protected virtual void Init()
    {
        cam = Camera.main.gameObject.GetComponent<Camera>();
        rigid = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        prevVec = transform.position;
        State = BirdState.Idle;
    }

    protected virtual void UpdateController()
    {
        switch (State)
        {
            case BirdState.Idle:
                UpdateIdle();
                break;
            case BirdState.Moving:
                UpdateMoving();
                break;
            case BirdState.Jumping:
                UpdateJumping();
                break;
        }

       // Debug.Log("Name: " + this.gameObject.name + " State : " + State + " isJumping: " + isJumping + " moveVec: " + moveVec + " isSliding:  " + isSliding) ; 

    }

    //State와 transform 정보를 계속해서 수신 받기에, State에 따라 애니메이션을 재생시키며 이동시켜주면 된다.
    //문제: Idle 패킷은 오지 않아서 알아서 판별해야한다...

    protected virtual void UpdateIdle()
    {
        isJumping = false;
        isSliding = false;
        UpdateAnimation();
    }

    protected virtual void UpdateMoving()
    {

        if ((playerInfo.Position.X == transform.position.x && playerInfo.Position.Y == transform.position.y && playerInfo.Position.Z == transform.position.z))
        {
            State = BirdState.Idle;
            return;
        }
        else
        {
            Vector3 moveVec = new Vector3(playerInfo.Position.X, playerInfo.Position.Y, playerInfo.Position.Z);
            Vector3 moveRot = new Vector3(playerInfo.Rotation.X, playerInfo.Rotation.Y, playerInfo.Rotation.Z);

            transform.position = moveVec;
            transform.rotation = Quaternion.Euler(moveRot);

            UpdateAnimation();
        }
   

    }
    //바닥에 착지하여도, 그 사이에 다량의 Jump 패킷이 넘어와서 Jump가 되버린다...
    protected virtual void UpdateJumping()
    {

        if ((playerInfo.Position.X == prevVec.x && playerInfo.Position.Y == prevVec.y && playerInfo.Position.Z == prevVec.z))
        {
            State = BirdState.Idle;
            isJumping = false;

            return;
        }

        Vector3 moveVec = new Vector3(playerInfo.Position.X, playerInfo.Position.Y, playerInfo.Position.Z);
        Vector3 moveRot = new Vector3(playerInfo.Rotation.X, playerInfo.Rotation.Y, playerInfo.Rotation.Z);
        transform.rotation = Quaternion.Euler(moveRot);

        if (!isJumping)
        {
            animator.SetTrigger("doJump");
            isJumping = true;   
        }
        
        transform.position = moveVec;
        UpdateAnimation();
        
    }

    protected void HideCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
       
    }

    protected virtual void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Victory Ground"))
        {
            transform.GetChild(0).gameObject.SetActive(false);


        }


        if (collision.gameObject.CompareTag("Ground"))
        {
            if (isJumping)
            {
                State = BirdState.Idle;
                isJumping = false;
                isSliding = false;
                animator.SetBool("isSlide", false);
                UpdateAnimation();

            }

            //Debug.Log("Collision Stay");
        }
    }

    public void SetAnim(Google.Protobuf.Protocol.PlayerState state)
    {

        switch(state)
        {
            case Google.Protobuf.Protocol.PlayerState.Idle:
                State = BirdState.Idle;
            
                break;
            case Google.Protobuf.Protocol.PlayerState.Move:
                State = BirdState.Moving;
          
                break;
            case Google.Protobuf.Protocol.PlayerState.Jump:
                State = BirdState.Jumping;
                break;
            case Google.Protobuf.Protocol.PlayerState.Slide:
                State = BirdState.Jumping;
                isSliding = true;
                break;
        }
    }

    void UpdateAnimation()
    {
        switch (State)
        {
            case BirdState.Idle:
                animator.SetBool("MoveForward", false);
                animator.SetBool("inAir", false);
                animator.SetTrigger("backJump");
                animator.SetBool("isSlide", false);
                break;
            case BirdState.Moving:
                animator.SetBool("MoveForward", true);
                animator.SetBool("inAir", false);
                break;
            case BirdState.Jumping:
                animator.SetBool("MoveForward", false);
                animator.SetBool("inAir", true);

                if (isSliding)
                    animator.SetBool("isSlide", true);
                else
                    animator.SetBool("isSlide", false);
                break;
        }
    }

    public void GoalPlayer()
    {
        //Debug.Log("Player destroy");
  

    }

    public void PlayerComplete(bool success)
    {
        /*
        if (success)
            Destroy(this.gameObject);
        else
        {
            Destroy(this.gameObject);
        }
        */
    }


}
