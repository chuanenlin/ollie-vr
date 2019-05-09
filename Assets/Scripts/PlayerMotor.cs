using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMotor : MonoBehaviour, BluetoothManager.BluetoothKeyListener
{
    private const float LANE_DISTANCE = 2.5f;
    private const float TURN_SPEED = 0.05f;

    public bool isRunning = false;
    public GameManager gameManager;
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

        if (IsGrounded())
        {
            verticalVelocity = -0.1f;
        } else
        {
            verticalVelocity -= (gravity * Time.deltaTime);
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

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        switch (hit.gameObject.tag)
        {
            case "Obstacle":
                Crash();
                break;
        }
    }

    public void onUp()
    {
        if (!gameManager.isGameStarted) return;

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

        if (IsGrounded())
        {
            AudioSource audio = GetComponent<AudioSource>();
            audio.PlayOneShot(jumpSound);
            verticalVelocity = jumpForce;
        }

        // Calculate move vector
        Vector3 moveVector = Vector3.zero;
        moveVector.x = (targetPosition - transform.position).normalized.x * speed;
        moveVector.y = verticalVelocity;
        moveVector.z = speed;

        // Move player
        controller.Move(moveVector * Time.deltaTime);
    }

    public void onDown()
    {
        if (!gameManager.isGameStarted) return;

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

        if (!IsGrounded())
        {
            AudioSource audio = GetComponent<AudioSource>();
            audio.PlayOneShot(fallSound);
            verticalVelocity = -jumpForce;
        }

        // Calculate move vector
        Vector3 moveVector = Vector3.zero;
        moveVector.x = (targetPosition - transform.position).normalized.x * speed;
        moveVector.y = verticalVelocity;
        moveVector.z = speed;

        // Move player
        controller.Move(moveVector * Time.deltaTime);
    }

    public void onLeft()
    {
        if (!gameManager.isGameStarted) return;

        MoveLane(false);
    }

    public void onRight()
    {
        if (!gameManager.isGameStarted) return;

        MoveLane(true);
    }
}
