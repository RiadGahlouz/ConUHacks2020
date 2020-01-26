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

    private Vector3? targetPos = null;
    private Camera FpsCamera;

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

        var newPos = Vector3.MoveTowards(transform.position, targetPos.Value, 0.03f);
        newPos.y = transform.position.y;

        transform.LookAt(newPos, Vector3.up);
        transform.position = newPos;

        if ((newPos - transform.position).magnitude > 0f)
        {
            speed = 0.99f;
        }
        else
        {
            speed = IDLE_SPEED;
        }
    }

    public void OnUserClickHit()
    {
        _anim.SetTrigger("jump");
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

    internal void SetFirstPersonCamera(Camera firstPersonCamera)
    {
        FpsCamera = firstPersonCamera;
    }

    internal void SetTargetPos(Vector3 position)
    {
        targetPos = position;
    }
}
