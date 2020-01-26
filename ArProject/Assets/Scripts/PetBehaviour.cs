using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetBehaviour : MonoBehaviour
{
    class MovementGoal
    {
        Vector3 startPosition;
        public Vector3 targetPosition;
        float maxSpeed; // in u/s
        float expectedTime;
        float currentTime;

        public MovementGoal(Vector3 startPosition, Vector3 targetPosition, float maxSpeed)
        {
            this.startPosition = startPosition;
            this.targetPosition = targetPosition;
            this.maxSpeed = maxSpeed;
            currentTime = 0;
            expectedTime = (targetPosition - startPosition).magnitude / maxSpeed;
        }

        public Vector3 GetDisplacement(float deltaTime)
        {
            currentTime += deltaTime;
            float progress;
            if (expectedTime == 0)
            {
                progress = 0;
            }
            else
            {
                progress = currentTime / expectedTime;
            }
            progress = Mathf.Clamp(progress, 0, 1);
            return Vector3.Lerp(startPosition, targetPosition, progress);
        }

        public bool IsDone()
        {
            return currentTime >= expectedTime;
        }
    }
    Animator _anim;

    private const float MAX_SPEED = 1.25f;
    private const float RUN_SPEED = 1.01f;
    private const float IDLE_SPEED = 0f;
    public float speed = IDLE_SPEED;

    private bool goingForSnack = false;

    private MovementGoal goal = null;

    private Vector3? playerForwardHit = null;

    // Start is called before the first frame update
    void Start()
    {
        _anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(goal != null)
        {
            MoveToTargetPos();
        }

        FixSpeedGlitch();
        _anim.SetFloat("speed", speed);
    }

    private void MoveToTargetPos()
    {
        if (goal == null) { return; }
        transform.position = goal.GetDisplacement(Time.deltaTime);
        transform.LookAt(goal.targetPosition);
        speed = MAX_SPEED;
        if (goal.IsDone())
        {
            speed = IDLE_SPEED;
            goal = null;
            // We bark upon arriving
            _anim.SetTrigger("bark");
            
        }
    }

    public void OnUserClickHit(float mood)
    {
        if (mood > 0.66)
        {
            _anim.SetTrigger("bark");
        } 
        else if (mood > 0.33)
        {
            _anim.SetTrigger("pet");
        }
        else
        {
            _anim.SetTrigger("jump");
        }
    }

    private void FixSpeedGlitch()
    {
        if (speed > 0.99999f && speed < 1.01f)
        {
            speed = RUN_SPEED;
        }
        else if (speed > MAX_SPEED)
        {
            speed = MAX_SPEED;
        }
        else if (speed < IDLE_SPEED)
        {
            speed = IDLE_SPEED;
        }
    }

    internal void SetPlayerForwardHit(Vector3 forwardHit)
    {
        if (playerForwardHit != null)
        {
            var mag = (playerForwardHit.Value - forwardHit).magnitude;

            if (mag > 0.5f)
            {
                playerForwardHit = forwardHit;
            }

            if (!goingForSnack) 
            {
                SetTargetPos(playerForwardHit.Value);
            }

            return;
        }

        playerForwardHit = forwardHit;
    }

    internal void SetTargetPos(Vector3 position)
    {
        goal = new MovementGoal(transform.position, position, MAX_SPEED);             
        goingForSnack = true;
    }
}
