﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideCannon_projectile : MonoBehaviour
{
    private Rigidbody2D rb;
    public float speed;
    private Vector2 vel;
    public float maxTimer;
    private float timer, original_Speed;
    private Vector3 Floor;
    public bool move_Right;
    public ParticleSystem explosion_Particles, trail_Particles;

    private void Awake()
    {
        original_Speed = speed;
    }

    // Start is called before the first frame update
    void Start()
    {
        timer = maxTimer;
        
        rb = GetComponent<Rigidbody2D>();
        Floor = new Vector3(0,-3,0);
    }

    private void OnEnable()
    {
        timer = maxTimer;
        speed = original_Speed;
        this.gameObject.GetComponent<SpriteRenderer>().enabled = true;
        trail_Particles.Play();
        explosion_Particles.Stop();
        explosion_Particles.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        Projectile_Move();
    }

    void Projectile_Move()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            Vector2 current_Target = Floor - transform.position;
            Quaternion.Lerp(transform.rotation, Quaternion.Euler(current_Target), Time.deltaTime * speed);
            timer = -1;
        }
        if (move_Right) {
            rb.MovePosition(transform.position + transform.right * speed * Time.deltaTime);
        }else
        {
            rb.MovePosition(transform.position - transform.right * speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Has touched something");
        StartCoroutine(wait_To_Deactivate());
        float angle = Mathf.Atan2(other.transform.position.y, other.transform.position.x);
        if (other.gameObject.tag == "Player") {
            other.gameObject.GetComponent<BasicPlayerScript>().GetHit(150f, angle, 0, 0.2f, 20f, 1.5f, true, 0.1f, 0.3f, 0.2f);
        }
    }


    IEnumerator wait_To_Deactivate()
    {
        speed = 0;
        this.gameObject.GetComponent<SpriteRenderer>().enabled = false;
        explosion_Particles.gameObject.SetActive(true);
        explosion_Particles.Play();
        trail_Particles.Stop();
        yield return new WaitForSeconds(1.2f);
        explosion_Particles.Stop();
        explosion_Particles.gameObject.SetActive(false);
        this.gameObject.SetActive(false);
    }
}
