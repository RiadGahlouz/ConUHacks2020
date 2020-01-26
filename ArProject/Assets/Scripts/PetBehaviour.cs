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

    private Camera FpsCamera = null;

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

        var newPos = Vector3.MoveTowards(transform.position, targetPos.Value, 0.05f);
        newPos.y = transform.position.y;

        transform.LookAt(newPos, Vector3.up);

        if ((newPos - transform.position).magnitude > 3f)
        {
            speed = MAX_SPEED;
        }
        else if ((newPos - transform.position).magnitude > 0.3f)
        {
            speed = 0.99f;
        }
        else
        {
            speed = IDLE_SPEED;
            _anim.SetTrigger("bark");

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

    internal void SetFirstPersonCamera(Camera firstPersonCamera)
    {
        // if (FpsCamera != null)
        // {
        //     // Change to raycast instead of camera
        //     float mag = (firstPersonCamera.transform.position - FpsCamera.transform.position).magnitude;
        //     float rot = (firstPersonCamera.transform.rotation.eulerAngles - FpsCamera.transform.rotation.eulerAngles).magnitude;

        //     if (mag > 0.5f || rot > 0.5f)
        //     {
        //         FpsCamera = firstPersonCamera;
        //         SetTargetPos(FpsCamera.transform.forward.normalized * 3f);
        //     }
        //     return;
        // }

        // First time behavior
        FpsCamera = firstPersonCamera;
    }

    internal void SetTargetPos(Vector3 position)
    {
        targetPos = position;
    }
}
