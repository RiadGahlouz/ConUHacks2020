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

    private Camera FpsCamera;

    // Start is called before the first frame update
    void Start()
    {
        _anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

        var newPos = Vector3.MoveTowards(transform.position, FpsCamera.transform.position, 0.03f);
        newPos.y = transform.position.y;

        transform.LookAt(newPos, Vector3.up);
        transform.position = newPos;

        if((newPos - transform.position).magnitude > 0f)
        {
            speed = 0.75f;
        }else
        {
            speed = IDLE_SPEED;
        }

        FixSpeedGlitch();
        _anim.SetFloat("speed", speed);
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
}
