using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Inspector Variables
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float jumpHeight = 1f;
    [SerializeField] private float speedLimit = 1f;
    [SerializeField] private ParticleSystem dustParticle;

    //cache
    private Rigidbody2D myRigidBody;
    private Animator animator;
    private BoxCollider2D myBoxCollider;

    //constant String references
    private const string IS_RUNNING = "isRunning"; //Animator bool for running animation
    private const string IS_JUMPING = "isJumping"; //Animator trigger for jumping animation
    private const string IS_LANDING = "isLanding"; //Animator trigger for landing animation
    private const string FOREGROUND = "Foreground"; //LayerMask.GetMask()

    //script variables
    private bool isJumping = false;
    private float faceRight = 1f;
    private float moveHorizontal;
    private float jumpVertical;

    // Start is called before the first frame update
    void Start()
    {
        myRigidBody = this.GetComponent<Rigidbody2D>();
        animator = this.GetComponent<Animator>();
        myBoxCollider = this.GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        moveHorizontal = Input.GetAxisRaw("Horizontal");
        jumpVertical = Input.GetAxisRaw("Vertical");

        RunningAnimation(moveHorizontal);
        JumpAnimation(jumpVertical); //Handles Jump and Land Animations
        LimitSpeed(speedLimit);
    }

    private void FixedUpdate()
    {
        PlayerMoveHorizontal(moveHorizontal);
        Jump(jumpVertical);
    }

    private void RunningAnimation(float input)
    {
        if(input != 0)
        {
            animator.SetBool(IS_RUNNING, true);
        }
        else
        {
            animator.SetBool(IS_RUNNING, false);
        }
        FlipPlayer(input);

        if(myRigidBody.velocity.y != 0)
        {
            animator.SetBool(IS_RUNNING, false);
        }
    }

    private void JumpAnimation(float input)
    {
        if(input > 0)
        {
            animator.SetBool(IS_RUNNING, false);
            animator.SetTrigger(IS_JUMPING);
            isJumping = true;
        }
        else if (myRigidBody.velocity.y == 0 && isJumping == true)
        {
            LandAnimation();
        }
    }

    private void LandAnimation()
    {
        animator.SetTrigger(IS_LANDING);
        animator.ResetTrigger(IS_JUMPING);
        isJumping = false;
    }

    private void PlayerMoveHorizontal(float input)
    {
        if (input > 0)
        {
            myRigidBody.AddForce(new Vector2(moveSpeed, 0f), ForceMode2D.Force);
            //LimitSpeed(speedLimit);
        }
        else if (input < 0)
        {
            myRigidBody.AddForce(new Vector2(-moveSpeed, 0f), ForceMode2D.Force);
            //LimitSpeed(speedLimit);
        }
    }

    private void LimitSpeed(float speedLimit)
    {
        if(myRigidBody.velocity.x > speedLimit)
        {
            myRigidBody.velocity = new Vector2(speedLimit, myRigidBody.velocity.y);
        }
        else if(myRigidBody.velocity.x < -speedLimit)
        {
            myRigidBody.velocity = new Vector2(-speedLimit, myRigidBody.velocity.y);
        }
    }

    private void FlipPlayer(float input)
    {
        if(input > 0)
        {
            this.transform.localScale = new Vector2(faceRight, 1f);
            dustParticle.Play();
        }
        else if(input < 0)
        {
            this.transform.localScale = new Vector2(-faceRight, 1f);
            dustParticle.Play();
        }
        else
        {
            this.transform.localScale = new Vector2(faceRight, 1f);
        }
    }

    private void Jump(float input)
    {
        if(input > 0 && myBoxCollider.IsTouchingLayers(LayerMask.GetMask(FOREGROUND)))
        {
            myRigidBody.AddForce(new Vector2(0f, jumpHeight), ForceMode2D.Impulse);
        }
    }
}
