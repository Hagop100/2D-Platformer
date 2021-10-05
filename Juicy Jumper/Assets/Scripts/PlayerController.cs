using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Inspector Variables
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private ParticleSystem dustParticle;

    //cache
    private Rigidbody2D myRigidBody;
    private Animator animator;

    //constant String references
    private const string IS_RUNNING = "isRunning"; //Animator bool for running animation

    // Start is called before the first frame update
    void Start()
    {
        myRigidBody = this.GetComponent<Rigidbody2D>();
        animator = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        PlayerMove();
    }

    private void PlayerMove()
    {
        float input = Input.GetAxisRaw("Horizontal");
        if(input > 0)
        {
            animator.SetBool(IS_RUNNING, true);
            myRigidBody.velocity = new Vector2(moveSpeed, myRigidBody.velocity.y);
        }
        else if(input < 0)
        {
            animator.SetBool(IS_RUNNING, true);
            myRigidBody.velocity = new Vector2(-moveSpeed, myRigidBody.velocity.y);
        }
        else
        {
            animator.SetBool(IS_RUNNING, false);
        }
        FlipPlayer(input);
    }

    private void FlipPlayer(float input)
    {
        float faceRight = 1f;
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
}
