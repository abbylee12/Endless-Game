using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 direction;
    public float forwardSpeed;
    public float maxSpeed;

    private int desiredLane = 1; //0:Left 1:Middle 2:Right
    public float laneDistance = 4; //The distance between two lanes

    //---------
    public bool isGrounded;
    public LayerMask groundLayer;
    public Transform groundCheck;
    //---------

    public float jumpForce;
    public float Gravity = -20;

    public Animator animator;

    private bool isSliding = false;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }




    // Update is called once per frame
    void Update()
    {
        if (!PlayerManager.isGameStarted)
            return;

        //Increase Speed
        if (forwardSpeed < maxSpeed)
            forwardSpeed += 0.1f * Time.deltaTime;

        animator.SetBool("isGameStarted", true);

        direction.z = forwardSpeed;

        isGrounded = Physics.CheckSphere(groundCheck.position, 0.17f, groundLayer);
        animator.SetBool("isGrounded", controller.isGrounded);

        //if (controller.isGrounded)
        if (controller.isGrounded)
        {
            //direction.y = -1;
            if (Input.GetKeyDown(KeyCode.UpArrow))
                Jump();

            //Added this
            if (Input.GetKeyDown(KeyCode.DownArrow) && !isSliding)
                StartCoroutine(Slide());
        }
        else
        {
            direction.y += Gravity * Time.deltaTime;

        }

        if (SwipeManager.swipeDown && !isSliding)
        {
            StartCoroutine(Slide());
            direction.y = -8;
        }

        //Gather the inputs on which lane we should be

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            desiredLane--;
            if (desiredLane == -1)
                desiredLane = 0;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            desiredLane++;
            if (desiredLane == 3)
                desiredLane = 2;
        }

        //Calculate where we should be in the future


        Vector3 targetPosition = transform.position.z * transform.forward + transform.position.y * transform.up;

        if (desiredLane == 0)
        {
            targetPosition += Vector3.left * laneDistance;
        }
        else if (desiredLane == 2)
        {
            targetPosition += Vector3.right * laneDistance;
        }

        //transform.position = Vector3.Lerp(transform.position, targetPosition, 70 * Time.deltaTime);
        if (transform.position != targetPosition)
        {
            Vector3 diff = targetPosition - transform.position;
            Vector3 moveDir = diff.normalized * 25 * Time.deltaTime;
            //if (moveDir.sqrMagnitude < diff.sqrMagnitude)
            if (moveDir.sqrMagnitude < diff.magnitude)
                controller.Move(moveDir);
            else
                controller.Move(diff);
        }

        //Move Player
        controller.Move(direction * Time.deltaTime);

    }

    //private void FixedUpdate()
    //{
    //    if (!PlayerManager.isGameStarted)
    //            return;

    //      controller.Move(direction * Time.fixedDeltaTime);
    //}

    private void Jump()
    {
        direction.y = jumpForce;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.transform.tag == "Obstacle")
        {
            PlayerManager.gameOver = true;
            FindObjectOfType<AudioManager>().PlaySound("GameOver");
        }
    }

    private IEnumerator Slide()
    {
        isSliding = true;
        animator.SetBool("isSliding", true);
        controller.center = new Vector3(0, -0.5f, 0);
        controller.height = 1;
        yield return new WaitForSeconds(1.3f);

        controller.center = new Vector3(0, 0, 0);
        controller.height = 2;
        animator.SetBool("isSliding", false);
        isSliding = false;
    }
}
