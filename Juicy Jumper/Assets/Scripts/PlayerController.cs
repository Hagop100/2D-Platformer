using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Inspector Variables
    [SerializeField] private float groundMoveSpeed = 1f;
    [SerializeField] private float aerialMobility = 1f;
    [SerializeField] private float jumpHeight = 1f;
    [SerializeField] private float speedLimit = 1f;
    [SerializeField] private float fallVelocity = 1f;
    [SerializeField] private float fastFallSpeed = 1f;
    [SerializeField] private float waveDashForce = 1f;
    [SerializeField] private float slidyness = 2f;
    [SerializeField] private float defaultFriction = 10f;
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
    private bool isJumpingAnimation = false;
    private bool isJumpingPhysically = false;
    private float faceRight = 1f;
    private float moveHorizontal;
    private float moveVertical;
    private float jumpVertical;
    private bool waveDashButtonDown;
    private bool waveDashState = false;

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
        moveVertical = Input.GetAxisRaw("Vertical");
        jumpVertical = Input.GetAxisRaw("Jump");
        if (Input.GetButtonDown("Fire1")) { waveDashButtonDown = true; }

        RunningAnimation(moveHorizontal);
        JumpAnimation(jumpVertical); //Handles Jump and Land Animations
        LimitSpeed(speedLimit);
        OnLand();
    }

    private void FixedUpdate()
    {
        if (waveDashState == false)
        {
            PlayerMoveHorizontal(moveHorizontal);
            Jump(jumpVertical);
            FallFast(fallVelocity);
            FastFall(fastFallSpeed);
        }

        if (waveDashButtonDown) 
        { 
            WaveDash(moveHorizontal, moveVertical);
            waveDashButtonDown = false;
        }
    }

    private void FallFast(float fallVelocity) //determines general falling speed
    {
        if(isJumpingAnimation == true && myRigidBody.velocity.y <= 0)
        {
            myRigidBody.AddForce(new Vector2(0f, -fallVelocity), ForceMode2D.Force);
        }
    }

    private void FastFall(float fastFallSpeed) //determines fast fall speed when pressing down key (akin to Melee)
    {
        if(isJumpingAnimation == true && myRigidBody.velocity.y < 0 && moveVertical < 0)
        {
            myRigidBody.AddForce(new Vector2(0f, -fastFallSpeed), ForceMode2D.Impulse);
        }
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
            isJumpingAnimation = true;
        }
        else if (myRigidBody.velocity.y == 0 && isJumpingAnimation == true)
        {
            LandAnimation();
        }
    }

    private void LandAnimation()
    {
        animator.SetTrigger(IS_LANDING);
        animator.ResetTrigger(IS_JUMPING);
        isJumpingAnimation = false;
    }

    private void PlayerMoveHorizontal(float input)
    {
        if (input > 0 && isJumpingAnimation == false)
        {
            myRigidBody.AddForce(new Vector2(groundMoveSpeed, 0f), ForceMode2D.Force);
        }
        else if (input < 0 && isJumpingAnimation == false)
        {
            myRigidBody.AddForce(new Vector2(-groundMoveSpeed, 0f), ForceMode2D.Force);
        }
        else if (input > 0 && isJumpingAnimation == true)
        {
            myRigidBody.AddForce(new Vector2(aerialMobility, 0f), ForceMode2D.Force);
        }
        else if (input < 0 && isJumpingAnimation == true)
        {
            myRigidBody.AddForce(new Vector2(-aerialMobility, 0f), ForceMode2D.Force);
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
            isJumpingPhysically = true;
        }
    }

    private void OnLand()
    {
        if(isJumpingPhysically == true && myBoxCollider.IsTouchingLayers(LayerMask.GetMask(FOREGROUND)))
        {
            isJumpingPhysically = false;
        }
    }

    private void WaveDash(float inputX, float inputY)
    {
        if(isJumpingPhysically == true)
        {
            if(inputX > 0 && inputY < 0)
            {
                waveDashState = true;
                myBoxCollider.sharedMaterial.friction = slidyness;
                myBoxCollider.enabled = false;
                myBoxCollider.enabled = true;
                myRigidBody.AddForce(new Vector2(waveDashForce, -waveDashForce), ForceMode2D.Impulse);
                StartCoroutine(WaveDashLag());
            }
            else if(inputX < 0 && inputY < 0)
            {
                waveDashState = true;
                myBoxCollider.sharedMaterial.friction = slidyness;
                myBoxCollider.enabled = false;
                myBoxCollider.enabled = true;
                myRigidBody.AddForce(new Vector2(-waveDashForce, -waveDashForce), ForceMode2D.Impulse);
                StartCoroutine(WaveDashLag());
            }
        } 
    }

    IEnumerator WaveDashLag()
    {
        yield return new WaitForSeconds(0.5f);
        myBoxCollider.sharedMaterial.friction = defaultFriction;
        myBoxCollider.enabled = false;
        myBoxCollider.enabled = true;
        waveDashState = false;
    }
}
