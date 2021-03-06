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
    [SerializeField] private AudioClip waveDashAudioClip;
    [SerializeField] private AudioClip walkingAudioClip;

    //Cache Variables
    private Rigidbody2D myRigidBody;
    private Animator animator;
    private BoxCollider2D myBoxCollider;
    private AudioSource myAudioSource;

    private Vector2 fallFastVector;
    private Vector2 fastFallVector;
    private Vector2 groundMoveVectorRight;
    private Vector2 groundMoveVectorLeft;
    private Vector2 aerialMoveVectorRight;
    private Vector2 aerialMoveVectorLeft;
    private Vector2 faceRightVector;
    private Vector2 faceLeftVector;
    private Vector2 jumpHeightVector;
    private Vector2 waveDashForceVectorRight;
    private Vector2 waveDashForceVectorLeft;

    //constant String references
    private const string IS_RUNNING = "isRunning"; //Animator bool for running animation
    private const string IS_JUMPING = "isJumping"; //Animator trigger for jumping animation
    private const string IS_LANDING = "isLanding"; //Animator trigger for landing animation
    private const string FOREGROUND = "Foreground"; //LayerMask.GetMask()

    //script variables
    private bool isJumpingAnimation = false;
    private float faceRight = 1f;
    private float moveHorizontal;
    private float moveVertical;
    private bool jumpVertical = false;
    private bool waveDashButtonDown;
    private bool waveDashState = false;
    private WaitForSeconds waveDashDelay = new WaitForSeconds(0.2f);

    // Start is called before the first frame update
    void Start()
    {
        myRigidBody = this.GetComponent<Rigidbody2D>();
        animator = this.GetComponent<Animator>();
        myBoxCollider = this.GetComponent<BoxCollider2D>();
        myAudioSource = this.GetComponent<AudioSource>();
        VectorCache();
    }

    // Update is called once per frame
    void Update()
    {
        moveHorizontal = Input.GetAxisRaw("Horizontal");
        moveVertical = Input.GetAxisRaw("Vertical");
        if (Input.GetButtonDown("Jump")) { jumpVertical = true; }
        if (Input.GetButtonDown("Fire1")) { waveDashButtonDown = true; }

        RunningAnimation(moveHorizontal);
        JumpAnimation(jumpVertical); //Handles Jump and Land Animations
        LimitSpeed(speedLimit);
    }

    private void FixedUpdate()
    {
        if (waveDashState == false)
        {
            PlayerMoveHorizontal(moveHorizontal);
            if (jumpVertical)
            {
                Jump();
                jumpVertical = false;
            }
            FallFast();
            FastFall();
        }

        if (waveDashButtonDown) 
        { 
            WaveDash(moveHorizontal, moveVertical);
            waveDashButtonDown = false;
        }
    }

    private void FallFast() //determines general falling speed
    {
        if(!myBoxCollider.IsTouchingLayers(LayerMask.GetMask(FOREGROUND)) && myRigidBody.velocity.y <= 0)
        {
            myRigidBody.AddForce(fallFastVector, ForceMode2D.Force);
        }
    }

    private void FastFall() //determines fast fall speed when pressing down key (akin to Melee)
    {
        if(!myBoxCollider.IsTouchingLayers(LayerMask.GetMask(FOREGROUND)) && myRigidBody.velocity.y < 0 && moveVertical < 0)
        {
            myRigidBody.AddForce(fastFallVector, ForceMode2D.Impulse);
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

        if(Mathf.Abs(myRigidBody.velocity.y) > 0.0001f)
        {
            animator.SetBool(IS_RUNNING, false);
        }
    }

    private void JumpAnimation(bool input)
    {
        if(input == true || myRigidBody.velocity.y < -0.1f)
        {
            animator.SetBool(IS_RUNNING, false);
            animator.SetTrigger(IS_JUMPING);
            isJumpingAnimation = true;
        }
        else if (Mathf.Abs(myRigidBody.velocity.y) < 0.0001f && isJumpingAnimation == true && myBoxCollider.IsTouchingLayers(LayerMask.GetMask(FOREGROUND)))
        {
            LandAnimation();
        }
    }

    private void LandAnimation()
    {
        animator.SetTrigger(IS_LANDING);
        animator.ResetTrigger(IS_JUMPING);
        isJumpingAnimation = false;
        dustParticle.Play();
        if (waveDashState) { myAudioSource.PlayOneShot(waveDashAudioClip, 0.05f); }
    }

    private void PlayerMoveHorizontal(float input)
    {
        if (input > 0 && isJumpingAnimation == false)
        {
            myRigidBody.AddForce(groundMoveVectorRight, ForceMode2D.Force);
            dustParticle.Play();
        }
        else if (input < 0 && isJumpingAnimation == false)
        {
            myRigidBody.AddForce(groundMoveVectorLeft, ForceMode2D.Force);
            dustParticle.Play();
        }
        else if (input > 0 && isJumpingAnimation == true)
        {
            myRigidBody.AddForce(aerialMoveVectorRight, ForceMode2D.Force);
        }
        else if (input < 0 && isJumpingAnimation == true)
        {
            myRigidBody.AddForce(aerialMoveVectorLeft, ForceMode2D.Force);
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
            this.transform.localScale = faceRightVector;
        }
        else if(input < 0)
        {
            this.transform.localScale = faceLeftVector;
        }
        else
        {
            this.transform.localScale = faceRightVector;
        }
    }

    private void Jump()
    {
        if(myBoxCollider.IsTouchingLayers(LayerMask.GetMask(FOREGROUND)))
        {
            myRigidBody.AddForce(jumpHeightVector, ForceMode2D.Impulse);
        }
    }

    private void WaveDash(float inputX, float inputY)
    {
        if(!myBoxCollider.IsTouchingLayers(LayerMask.GetMask(FOREGROUND)))
        {
            if(inputX > 0 && inputY < 0)
            {
                waveDashState = true;
                myBoxCollider.sharedMaterial.friction = slidyness;
                myBoxCollider.enabled = false;
                myBoxCollider.enabled = true;
                myRigidBody.AddForce(waveDashForceVectorRight, ForceMode2D.Impulse);
                StartCoroutine(WaveDashLag());
            }
            else if(inputX < 0 && inputY < 0)
            {
                waveDashState = true;
                myBoxCollider.sharedMaterial.friction = slidyness;
                myBoxCollider.enabled = false;
                myBoxCollider.enabled = true;
                myRigidBody.AddForce(waveDashForceVectorLeft, ForceMode2D.Impulse);
                StartCoroutine(WaveDashLag());
            }
        } 
    }

    IEnumerator WaveDashLag()
    {
        yield return waveDashDelay;
        myBoxCollider.sharedMaterial.friction = defaultFriction;
        myBoxCollider.enabled = false;
        myBoxCollider.enabled = true;
        waveDashState = false;
        animator.ResetTrigger(IS_LANDING);
        animator.ResetTrigger(IS_JUMPING);
    }

    private void VectorCache()
    {
        fallFastVector = new Vector2(0f, -fallVelocity);
        fastFallVector = new Vector2(0f, -fastFallSpeed);
        groundMoveVectorRight = new Vector2(groundMoveSpeed, 0f);
        groundMoveVectorLeft = new Vector2(-groundMoveSpeed, 0f);
        aerialMoveVectorRight = new Vector2(aerialMobility, 0f);
        aerialMoveVectorLeft = new Vector2(-aerialMobility, 0f);
        faceRightVector = new Vector2(faceRight, 1f);
        faceLeftVector = new Vector2(-faceRight, 1f);
        jumpHeightVector = new Vector2(0f, jumpHeight);
        waveDashForceVectorRight = new Vector2(waveDashForce, -waveDashForce);
        waveDashForceVectorLeft = new Vector2(-waveDashForce, -waveDashForce);
    }
}
