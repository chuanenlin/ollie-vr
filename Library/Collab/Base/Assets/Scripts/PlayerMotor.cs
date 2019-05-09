using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    private const float LANE_DISTANCE = 2.5f;
    private const float TURN_SPEED = 0.05f;

    private bool lowGravity = false;
    private bool isRunning = false;
    public AudioClip jumpSound;
    public AudioClip fallSound;
    public AudioClip crashSound;

    // TODO: Animation - 3

    // Movement
    private CharacterController controller;
    private float jumpForce = 5.0f;
    private float gravity = 12.0f;
    private float verticalVelocity;
    private int desiredLane = 1; // 0 = left, 1 = middle, 2 = right

    // Speed modifier
    private float originalSpeed = 7.0f;
    private float speed;
    private float speedIncreaseLastTick;
    private float speedIncreaseTime = 10.0f;
    private float speedIncreaseAmount = 0.1f;

    private void Start()
    {
        speed = originalSpeed;
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (!isRunning)
        {
            return;
        }
        if (Time.time - speedIncreaseLastTick > speedIncreaseTime)
        {
            speedIncreaseLastTick = Time.time;
            speed += speedIncreaseAmount;
            // Change modifier text
            GameManager.Instance.UpdateModifier(speed - originalSpeed);
        }
        // Gather the inputs on which lane we should be
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveLane(false);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveLane(true);
        }
        // Calculate where we should be in the future
        Vector3 targetPosition = transform.position.z * Vector3.forward;
        if (desiredLane == 0)
        {
            targetPosition += Vector3.left * LANE_DISTANCE;
        }
        else if (desiredLane == 2)
        {
            targetPosition += Vector3.right * LANE_DISTANCE;
        }
        // Calculate move vector
        Vector3 moveVector = Vector3.zero;
        moveVector.x = (targetPosition - transform.position).normalized.x * speed;

        // Calculate y
        if (IsGrounded())
        {
            verticalVelocity = -0.1f;
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                AudioSource audio = GetComponent<AudioSource>();
                audio.PlayOneShot(jumpSound);
                if (lowGravity)
                {
                    verticalVelocity = jumpForce * 2;
                }
                else {
                    // Jump
                    verticalVelocity = jumpForce;
                }
            }
            //else if (Input.GetKeyDown(KeyCode.DownArrow))
            //{
            //    // Slide
            //    StartSliding();

            //    Invoke("StopSliding", 2.0f);
                
            //}
        }
        else
        {
            verticalVelocity -= (gravity * Time.deltaTime);

            // Fast falling
            //if (Input.GetKeyDown(KeyCode.UpArrow))
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                AudioSource audio = GetComponent<AudioSource>();
                audio.PlayOneShot(fallSound);
                verticalVelocity = -jumpForce;
            }
        }

        moveVector.y = verticalVelocity;
        moveVector.z = speed;

        // Move player
        controller.Move(moveVector * Time.deltaTime);

        // Rotate player to where it's going
        Vector3 dir = controller.velocity;
        if (dir != Vector3.zero)
        {
            dir.y = 0;
            //transform.forward = dir;
            transform.forward = Vector3.Lerp(transform.forward, dir, TURN_SPEED);
        }
    }

    //private void StartSliding()
    //{
    //    controller.height /= 2;
    //    controller.center = new Vector3(controller.center.x, controller.center.y / 2, controller.center.z);
    //}

    //private void StopSliding()
    //{
    //    controller.height *= 2;
    //    controller.center = new Vector3(controller.center.x, controller.center.y * 2, controller.center.z);
    //}


    private void MoveLane(bool goingRight)
    {
        desiredLane += (goingRight) ? 1 : -1;
        desiredLane = Mathf.Clamp(desiredLane, 0, 2);
    }

    private bool IsGrounded()
    {
        Ray groundRay = new Ray(new Vector3(controller.bounds.center.x, (controller.bounds.center.y - controller.bounds.extents.y) + 0.2f, controller.bounds.center.z), Vector3.down);
        Debug.DrawRay(groundRay.origin, groundRay.direction, Color.cyan, 1.0f);

        return Physics.Raycast(groundRay, 0.2f + 0.1f);
    }

    public void StartRunning()
    {
        isRunning = true;
    }

    private void Crash()
    {
        // anim.SetTrigger("Death");
        AudioSource audio = GetComponent<AudioSource>();
        audio.PlayOneShot(crashSound);
        isRunning = false;
        GameManager.Instance.OnDeath();
    }

    private void Hifey()
    {
        lowGravity = !lowGravity;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        switch (hit.gameObject.tag)
        {
            case "Obstacle":
                Crash();
                break;
        }
    }


}
