using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public float gravity;
    public float jumpForce = 20;
    public float groundHeight = 10;
    public float maxHoldJumpTime = 0.4f;
    public float maxMaxHoldJumpTime = 0.4f;
    public float holdJumpTimer;
    public float jumpGroundThreshold = 1;

    public bool isGrounded = false;
    public bool isHoldingJump;
    public bool isDead = false;

    public Vector2 velocity;
    public float maxXVelocity = 100;
    public float acceleration = 10;
    public float maxAcceleration = 10;
    public float distance = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 pos = transform.position;

        // jump
        float groundDistance = Mathf.Abs(pos.y - groundHeight); // calculate the distance between our position y and whre the ground is  

        if(isGrounded || groundDistance <= jumpGroundThreshold) 
        {
            if(Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))
            {
                isGrounded = false;
                velocity.y = jumpForce;
                isHoldingJump = true;
                holdJumpTimer = 0;
            }
        }

        if(Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.W))
        {
            isHoldingJump = false;
        }
    }

    private void FixedUpdate()
    {
        Vector2 pos = transform.position;

        if(isDead)
        {
            Scene scene = SceneManager.GetActiveScene(); SceneManager.LoadScene(scene.name);
        }

        if(pos.y < -20)
        {
            isDead = true;
        }

         // set a max time holding the jump
        if(!isGrounded)
        {
            if(isHoldingJump)
            {
                holdJumpTimer += Time.fixedDeltaTime;
                if(holdJumpTimer >= maxHoldJumpTime)
                {
                    isHoldingJump = false;
                }
            }

            // apply velocity
            pos.y += velocity.y * Time.fixedDeltaTime; 

            // apply gravity
            if(!isHoldingJump)
            {
            velocity.y += gravity * Time.fixedDeltaTime; 
            }

            // create a ray
            Vector2 rayOrigin = new Vector2(pos.x + 0.7f, pos.y);
            Vector2 rayDirection = Vector2.up;
            float rayDistance = velocity.y * Time.fixedDeltaTime;
            RaycastHit2D hit2D = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance);

            // if we are colliding with the floor
            if (hit2D.collider != null)
            {
                Ground ground = hit2D.collider.GetComponent<Ground>();

                if (ground != null)
                {
                    if(pos.y >= ground.groundHeight)
                    {
                        groundHeight = ground.groundHeight;
                        pos.y = groundHeight;
                        velocity.y = 0;
                        isGrounded = true;
                    }
                }
            }

            Debug.DrawRay(rayOrigin, rayDirection * rayDistance, Color.red);

            Vector2 wallOrigin = new Vector2(pos.x, pos.y);
            RaycastHit2D wallHit = Physics2D.Raycast(wallOrigin, Vector2.right, velocity.x * Time.fixedDeltaTime);
            if(wallHit.collider != null)
            {
                Ground ground = wallHit.collider.GetComponent<Ground>();
                if(ground != null)
                {
                    if(pos.y < ground.groundHeight)
                    {
                        velocity.x = 0;
                    }
                }
            }
        }
        distance += velocity.x * Time.fixedDeltaTime;

        // velocity adjustment
        if(isGrounded)
        {
            float velocityRatio = velocity.x / maxXVelocity;
            acceleration = maxAcceleration * (1 - velocityRatio);
            maxHoldJumpTime = maxMaxHoldJumpTime * velocityRatio;
            velocity.x += acceleration * Time.fixedDeltaTime;

            if(velocity.x >= maxXVelocity)
            {
                velocity.x = maxXVelocity;
            }

            // create a ray
            Vector2 rayOrigin = new Vector2(pos.x - 0.7f, pos.y);
            Vector2 rayDirection = Vector2.up;
            float rayDistance = velocity.y * Time.fixedDeltaTime;
            RaycastHit2D hit2D = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance);

            // if we are colliding with nothing
            if (hit2D.collider == null)
            {
                isGrounded = false;
            }

            Debug.DrawRay(rayOrigin, rayDirection * rayDistance, Color.yellow);

        }

        Vector2 obstOrigin = new Vector2(pos.x, pos.y);
        RaycastHit2D obstHitX = Physics2D.Raycast(obstOrigin, Vector2.right, velocity.x * Time.fixedDeltaTime);
        if(obstHitX.collider != null)
        {
            Obstacle obstacle = obstHitX.collider.GetComponent<Obstacle>();
            if(obstacle != null)
            {
                hitObstacle(obstacle);
            }
        }

        RaycastHit2D obstHitY= Physics2D.Raycast(obstOrigin, Vector2.up, velocity.y * Time.fixedDeltaTime);
        if(obstHitY.collider != null)
        {
            Obstacle obstacle = obstHitY.collider.GetComponent<Obstacle>();
            if(obstacle != null)
            {
                hitObstacle(obstacle);
            }
        }

        transform.position = pos;
     }

     void hitObstacle(Obstacle obstacle)
     {
         Destroy(obstacle.gameObject);
         velocity.x *= 0.7f;
     }
}
