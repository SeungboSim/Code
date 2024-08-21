using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.ConstrainedExecution;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    GameManager gameManager;
    InGameManager inGameManager;

    public Vector2 inputVec;
    public float speed;
    public Scanner scanner;
    public List<Weapon> weapons = new List<Weapon>();
    public Hand[] hands;
    public RuntimeAnimatorController[] animCon;

    public CircleCollider2D magArea;
    public Rigidbody2D rigid;
    public bool state_Unharmed = false;
    SpriteRenderer spriteRenderer;
    Animator anim;

    private void Awake()
    {
        gameManager = GameManager.instance;
        inGameManager = InGameManager.instance;
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        scanner = GetComponent<Scanner>();
        hands = GetComponentsInChildren<Hand>(true);
    }

    private void OnEnable()
    {
        speed = 3 * (1 + inGameManager.originalSpeedRate + Character.Speed);
        anim.runtimeAnimatorController = animCon[inGameManager.characterID];
    }

    private void FixedUpdate()
    {
        if (!inGameManager.isLive || inGameManager.dashState)
            return;

        Vector2 nextVec = inputVec.normalized * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
    }

    private void OnMove(InputValue value)
    {
        inputVec = value.Get<Vector2>();
    }
    private void LateUpdate()
    {
        if (!inGameManager.isLive)
            return;

        for (int i = inGameManager.expObjects.Count - 1; i >= 0; i--)
        {
            float dist = Vector3.SqrMagnitude(transform.position - inGameManager.expObjects[i].transform.position);
            if (dist < magArea.radius * magArea.radius)
            {
                inGameManager.expObjects[i].SetMag(rigid, 40);
            }

            if (dist < 0.5f)
            {
                inGameManager.GetExp(inGameManager.expObjects[i].exp);
                inGameManager.expObjects[i].magState = false;
                StopCoroutine(inGameManager.expObjects[i].enumerator);
                inGameManager.expObjects[i].gameObject.SetActive(false);
                inGameManager.expObjects.Remove(inGameManager.expObjects[i]);
            }
        }

        anim.SetFloat("Speed", inputVec.magnitude);
        if (inputVec.x != 0)
        {
            spriteRenderer.flipX = inputVec.x < 0;
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("EnemyBullet"))
            return;

        EnemyBullet eBullet = collision.gameObject.GetComponent<EnemyBullet>();

        if (!(eBullet.id == 124))
            return;

        if (eBullet.id == 124)
        {
            inGameManager.health -= Mathf.Max(1, eBullet.damage - inGameManager.deffence) * Time.deltaTime;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!(collision.CompareTag("Mag") || collision.CompareTag("Hp") || collision.CompareTag("Gold") || collision.CompareTag("EnemyBullet")))
            return;

        if (collision.CompareTag("Mag"))
        {
            Mag m = collision.gameObject.GetComponent<Mag>();
            GetAllExps();
            StopCoroutine(m.enumerator);
            m.coll.enabled = false;
            m.gameObject.SetActive(false);
            gameManager.AudioManager.PlaySfx(AudioManager.Sfx.GetItem);
        }
        else if (collision.CompareTag("Hp"))
        {
            Hp h = collision.gameObject.GetComponent<Hp>();
            inGameManager.health = Mathf.Min(inGameManager.health + inGameManager.maxHealth * 0.5f, inGameManager.maxHealth);
            StopCoroutine(h.enumerator);
            h.coll.enabled = false;
            h.gameObject.SetActive(false);
            gameManager.AudioManager.PlaySfx(AudioManager.Sfx.GetItem);
        }
        else if (collision.CompareTag("Gold"))
        {
            Gold g = collision.gameObject.GetComponent<Gold>();
            inGameManager.earnedGold += g.gold;
            StopCoroutine(g.enumerator);
            g.coll.enabled = false;
            g.gameObject.SetActive(false);
            gameManager.AudioManager.PlaySfx(AudioManager.Sfx.GetCoin);
        }
        else if (collision.CompareTag("EnemyBullet"))
        {
            EnemyBullet bullet = collision.gameObject.GetComponent<EnemyBullet>();
            if (bullet.id == 124)
                return;

            inGameManager.health -= Mathf.Max(1, bullet.damage - inGameManager.deffence);
            if (bullet.per == 1)
                bullet.gameObject.SetActive(false);
        }
    }   
    public void GetAllExps()
    {
        foreach (Exp e in inGameManager.expObjects)
        {
            if (Vector3.SqrMagnitude(e.transform.position - rigid.transform.position) < 2500)
                e.SetMag(rigid, 40);
            else
                e.transform.position = rigid.transform.position;
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!(inGameManager.isLive || collision.rigidbody.CompareTag("Enemy") || collision.rigidbody.CompareTag("BossArea")))
            return;

        if (collision.rigidbody.CompareTag("Enemy"))
            inGameManager.health -= Time.deltaTime * (Mathf.Max(1, collision.gameObject.GetComponent<Enemy>().damage - inGameManager.deffence));
        else if (collision.rigidbody.CompareTag("BossArea"))
            inGameManager.health -= Time.deltaTime * 20;
    }
    public void Dead()
    {
        for (int i = 2; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
        anim.SetTrigger("Dead");
    }
}
