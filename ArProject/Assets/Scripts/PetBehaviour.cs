using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetBehaviour : MonoBehaviour
{
    Animator _anim;

    public float speed = 1f;

    // Start is called before the first frame update
    void Start()
    {
        _anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        _anim.SetFloat("speed", speed);
    }

    public void OnUserClickHit()
    {
        _anim.SetTrigger("jump");
    }
}
