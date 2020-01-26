using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetBehaviour : MonoBehaviour
{
    Animator _anim;

    private const float MAX_SPEED = 1.25f;
    private const float RUN_SPEED = 1.01f;
    private const float IDLE_SPEED = 0f;
    public float speed = IDLE_SPEED;

    private bool goingForSnack = false;

    private Vector3? targetPos = null;

    private Vector3? playerForwardHit = null;

    // Start is called before the first frame update
    void Start()
    {
        _anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(targetPos != null)
        {
            MoveToTargetPos();
        }

        FixSpeedGlitch();
        _anim.SetFloat("speed", speed);
    }

    private void MoveToTargetPos()
    {
        if((targetPos.Value - transform.position).magnitude < 0.1f)
        {
            targetPos = null;
        }

        var newPos = Vector3.MoveTowards(transform.position, targetPos.Value, 0.1f);
        newPos.y = targetPos.Value.y;

        transform.LookAt(newPos, Vector3.up);

        if ((newPos - transform.position).magnitude >= 0.7f)
        {
            speed = MAX_SPEED;
        }
        else if ((newPos - transform.position).magnitude > 0.1f)
        {
            speed = 0.99f;
        }
        else
        {
            _anim.SetTrigger("bark");
            speed = IDLE_SPEED;

            if (goingForSnack)
            {
                goingForSnack = false;
            }
        }
        transform.position = newPos;
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
        targetPos = position;
        goingForSnack = true;
    }
}
