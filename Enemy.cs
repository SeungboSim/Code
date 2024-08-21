using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Net;
using UnityEngine;

public enum EnemyType { Melee, Range, Dash, Mole }
public class Enemy : MonoBehaviour
{
    GameManager gameManager;
    [HideInInspector]
    public InGameManager inGameManager;

    public EnemyType enemyType;
    public float damage;
    public float originalDamage;
    public float speed;
    public float originalSpeed;
    public float health;
    public float maxHealth;

    public int exp;
    public RuntimeAnimatorController[] animCon;

    // 상태 변수들
    #region
    [HideInInspector]
    public Rigidbody2D target;
    [HideInInspector]
    public Transform bulletPos;
    [HideInInspector]
    public bool isLive;
    [HideInInspector]
    public bool isInRange;
    [HideInInspector]
    public bool state_glue;
    [HideInInspector]
    public bool state_brush;
    [HideInInspector]
    public bool state_stuned;
    [HideInInspector]
    public bool state_invincible;
    [HideInInspector]
    public int glueLevel;
    [HideInInspector]
    public bool state_knockedback;
    [HideInInspector]
    public bool state_dash;
    float knockedbackPower;
    [HideInInspector]
    public float dotDamage_glue;
    [HideInInspector]
    public float dotDamage_brush;
    [HideInInspector]
    public Rigidbody2D rigid;
    [HideInInspector]
    public Collider2D coll;
    [HideInInspector]
    public Animator anim;
    [HideInInspector]
    public SpriteRenderer spriteRenderer;
    WaitForFixedUpdate wait;
    public int bulletId;
    public float attackRate;
    public float bulletDamage;
    public float bulletSpeed;
    public float range;

    float timer_attack;
    #endregion

    [Header("보스 개체")]
    public Boss boss;
    public List<EnemyBullet> bossBullets;

    private void Awake()
    {
        gameManager = GameManager.instance;
        inGameManager = InGameManager.instance;
        rigid = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        wait = new WaitForFixedUpdate();

        if (boss != null)
            boss.skillList = new List<string>();
    }
    private void OnEnable()
    {
        coll.enabled = true;
        rigid.simulated = true;
        speed = originalSpeed;
        health = maxHealth;

        target = inGameManager.player.GetComponent<Rigidbody2D>();

        spriteRenderer.sortingOrder = 2;
        isLive = true;
        if (boss == null)
        {
            ResetState();
            StartCoroutine(EnemyRoutine());
        }
    }
    IEnumerator EnemyRoutine()
    {
        while (isLive)
        {
            if (state_glue)
            {
                dotDamage_glue += Time.deltaTime * 2;
                speed = originalSpeed / glueLevel;
            }
            if (state_brush)
            {
                dotDamage_brush += Time.deltaTime * 2;
            }

            if (!(!isLive || !inGameManager.isLive))
            {
                if (Vector3.Distance(transform.position, target.position) < range)
                    isInRange = true;
                else
                    isInRange = false;

                if (isInRange)
                {
                    if (!state_knockedback)
                    {
                        rigid.velocity = Vector2.zero;
                    }
                    if (enemyType == EnemyType.Range)
                    {
                        timer_attack += Time.deltaTime;
                        if (timer_attack > attackRate)
                        {
                            timer_attack = 0;
                            if (bulletId != 0)
                            {
                                anim.SetTrigger("Attack");
                                Fire();
                            }
                        }
                    }
                    else if (enemyType == EnemyType.Mole)  // 땅에서 나온 상태
                    {
                        state_invincible = false;
                        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Digging"))
                            anim.SetBool("Dig", false);
                        rigid.velocity = Vector2.zero;
                        rigid.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime * 0.5f);
                    }
                }
                else
                {
                    if (enemyType == EnemyType.Dash)
                    {
                        timer_attack += Time.deltaTime;
                        if (timer_attack > attackRate)
                        {
                            timer_attack = 0;
                            StartCoroutine(Dash());
                        }
                    }
                    else if (enemyType == EnemyType.Mole)  // 땅에 들어간 상태
                    {
                        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Run"))
                            anim.SetBool("Dig", true);
                        state_invincible = true;
                    }
                    if (!state_knockedback || state_dash)
                    {
                        rigid.velocity = Vector2.zero;
                        rigid.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
                    }
                }
            }

            if (inGameManager.isGameOver)
            {
                rigid.velocity = Vector2.zero;
                break;
            }
            yield return null;
        }
    }
    private void LateUpdate()
    {
        if (!isLive || !inGameManager.isLive)
            return;

        spriteRenderer.flipX = target.position.x < rigid.position.x;
    }
    public void Init(EnemyData data)
    {
        anim.runtimeAnimatorController = animCon[data.spriteType];
        anim.SetBool("Dig", false);
        anim.SetBool("Dead", false);
        enemyType = data.enemyType;
        state_invincible = false;
        originalDamage = data.damage;
        damage = originalDamage;
        originalSpeed = data.speed;
        speed = originalSpeed;
        maxHealth = data.health;
        health = maxHealth;
        exp = data.exp;

        attackRate = data.attackRate;
        timer_attack = attackRate;
        bulletDamage = data.bulletDamage;
        bulletSpeed = data.bulletSpeed;
        range = data.range;

        if (enemyType == EnemyType.Range)
        {
            for (int i = 0; i < inGameManager.pool.prefabs.Length; i++)
            {
                if (data.e_bullet == inGameManager.pool.prefabs[i])
                {
                    bulletId = i;
                    break;
                }
            }
        }
        ResetState();
    }
    void ResetState()
    {
        state_glue = false;
        state_brush = false;
        dotDamage_glue = 1;
        dotDamage_brush = 1;
        speed = originalSpeed;
        glueLevel = 0;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Bullet") || !isLive || state_invincible)
            return;

        Bullet bullet = collision.collider.transform.parent.GetComponent<Bullet>();
        if (bullet.name.Contains("Broomstick") || bullet.name.Contains("Mop"))
        {
            if (bullet.damage < 100000)
            {
                Vector3 dir = collision.transform.position - transform.position;
                DamageText damageText = inGameManager.fPool.Get(0).GetComponent<DamageText>();
                damageText.Init(bullet.damage, transform.position, dir);
            }

            health -= bullet.damage;

            if (health > 0)
            {
                if (boss == null)
                    anim.SetTrigger("Hit");
                gameManager.AudioManager.PlaySfx(AudioManager.Sfx.Hit);
            }
            else
            {
                isLive = false;
                coll.enabled = false;
                rigid.simulated = false;
                spriteRenderer.sortingOrder = 1;
                anim.SetBool("Dead", true);
                inGameManager.kill++;
                inGameManager.spawner.enemyList.Remove(this);
                if (inGameManager.isLive)
                {
                    gameManager.AudioManager.PlaySfx(AudioManager.Sfx.Dead);
                    if (boss == null)
                        ExpSpawn();
                }
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Bullet") || !isLive)
            return;

        Bullet bullet = collision.GetComponent<Bullet>();
        if (bullet.id == 1 || bullet.id == 3 || bullet.id == 83 || bullet.id == 84 || bullet.id == 85 || bullet.id == 87 || bullet.id == 89 || bullet.id == 90 || bullet.id == 91 || bullet.id == 92)
            return;

        if (bullet.damage < 100000)
        {
            if (state_invincible)
                return;

            Vector3 dir = collision.transform.position - transform.position;
            DamageText damageText = inGameManager.fPool.Get(0).GetComponent<DamageText>();

            if (bullet.name.Contains("MeasuringTape") || bullet.name.Contains("Explosion"))
                damageText.Init(bullet.damage, transform.position, dir);
            else
                damageText.Init(bullet.damage, bullet.transform.position, dir);
        }

        health -= bullet.damage;

        if (health > 0)
        {
            if (boss == null)
                anim.SetTrigger("Hit");
            gameManager.AudioManager.PlaySfx(AudioManager.Sfx.Hit);
        }
        else
        {
            isLive = false;
            coll.enabled = false;
            rigid.simulated = false;
            spriteRenderer.sortingOrder = 1;
            anim.SetBool("Dead", true);
            inGameManager.spawner.enemyList.Remove(this);
            if (inGameManager.isLive && bullet.damage < 100000)
            {
                inGameManager.kill++;
                gameManager.AudioManager.PlaySfx(AudioManager.Sfx.Dead);
                if (boss == null)
                    ExpSpawn();
            }
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Bullet") || !isLive || state_invincible)
            return;

        Bullet bullet = collision.GetComponent<Bullet>();

        if (!(bullet.id == 3 || bullet.id == 83 || bullet.id == 84 || bullet.id == 85 || bullet.id == 87 || bullet.id == 89 || bullet.id == 90 || bullet.id == 91 || bullet.id == 92))
            return;

        if (bullet.id == 3 || bullet.id == 87)
        {
            state_glue = true;
            if (bullet.id == 87)
                glueLevel = 2;
            else
                glueLevel = 4 / 3;
        }
        else if (bullet.id == 83 || bullet.id == 84 || bullet.id == 85 || bullet.id == 89 || bullet.id == 90 || bullet.id == 91 || bullet.id == 92)
        {
            state_brush = true;
            if (bullet.id == 90 || bullet.id == 92)
                glueLevel = 4 / 3;
        }
        if (dotDamage_glue > 1 || dotDamage_brush > 1)
        {
            if (dotDamage_glue > 1)
            {
                dotDamage_glue = 0;
            }
            else if (dotDamage_brush > 1)
            {
                dotDamage_brush = 0;
            }
            health -= bullet.damage;

            Vector3 dir = collision.transform.position - transform.position;
            DamageText damageText = inGameManager.fPool.Get(0).GetComponent<DamageText>();
            damageText.Init(bullet.damage, transform.position, dir);

            if (health > 0)
            {
                if (boss == null)
                    anim.SetTrigger("Hit");
                gameManager.AudioManager.PlaySfx(AudioManager.Sfx.Hit);
            }
            else
            {
                isLive = false;
                coll.enabled = false;
                rigid.simulated = false;
                spriteRenderer.sortingOrder = 1;
                anim.SetBool("Dead", true);
                inGameManager.kill++;
                inGameManager.spawner.enemyList.Remove(this);

                if (inGameManager.isLive)
                {
                    gameManager.AudioManager.PlaySfx(AudioManager.Sfx.Dead);
                    if (boss == null)
                        ExpSpawn();
                }
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Bullet") || !isLive)
            return;

        ResetState();
    }
    public void ExpSpawn()
    {
        Exp expObject = inGameManager.pool.Get(2).GetComponent<Exp>();
        if (expObject.exp == 0)
            inGameManager.expObjects.Add(expObject);
        expObject.transform.position = transform.position;
        expObject.Init(exp);
    }
    public void Knockedback(Transform bulletPos, float power)
    {
        this.bulletPos = bulletPos;
        knockedbackPower = power;
        StartCoroutine(KnockedBackRoutine());
    }
    IEnumerator KnockedBackRoutine()
    {
        yield return wait;
        state_knockedback = true;
        Vector3 dirVec = transform.position - bulletPos.position;
        rigid.AddForce(dirVec.normalized * knockedbackPower, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.1f);
        state_knockedback = false;
    }
    public void Fire()
    {
        EnemyBullet bullet = inGameManager.pool.Get(bulletId).GetComponent<EnemyBullet>();
        bullet.transform.position = transform.position;
        Vector3 dir = (target.transform.position - transform.position).normalized;
        bullet.Init(bulletDamage, 1, bulletSpeed, dir);
        bullet.transform.rotation = Quaternion.FromToRotation(Vector3.up, dir);

    }
    IEnumerator Dash()
    {
        state_dash = true;
        speed = originalSpeed * 6;
        yield return new WaitForSeconds(0.2f);
        state_dash = false;
        timer_attack = 0;
        speed = originalSpeed;
    }
    void Dead()
    {
        StopAllCoroutines();
        gameObject.SetActive(false);
    }


    // 보스 개체
    public void Init(BossData data)
    {
        anim.runtimeAnimatorController = animCon[data.spriteType];
        boss.skill = data.skill;
        originalDamage = data.damage;
        damage = originalDamage;
        originalSpeed = data.speed;
        speed = originalSpeed;
        maxHealth = data.health;
        health = maxHealth;
        attackRate = data.attackRate;
        bossBullets = new List<EnemyBullet>();
        SkillCheck();
        StartCoroutine(BossRoutine());
    }
    IEnumerator BossRoutine()
    {
        while (isLive)
        {
            if (state_glue)
            {
                dotDamage_glue += Time.deltaTime * 2;
            }
            if (state_brush)
            {
                dotDamage_brush += Time.deltaTime * 2;
            }
            if (!(!isLive || !inGameManager.isLive))
            {
                if (!boss.state_skill)
                {
                    if(!anim.GetCurrentAnimatorStateInfo(0).IsName("Run"))
                        anim.SetBool("Run", true);
                    rigid.velocity = Vector2.zero;
                    timer_attack += Time.deltaTime;
                    rigid.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
                }
                if (timer_attack > attackRate)
                {
                    timer_attack = 0;
                    UseSkill(boss.skillList[Random.Range(0, boss.skillList.Count)]);
                }
            }
            yield return null;
        }
    }
    public void SkillCheck()
    {
        if ((boss.skill & BossSkill.Dash_Once) == BossSkill.Dash_Once)
        {
            boss.skillList.Add("Dash_Once");
            boss.attackGuildeLine = Instantiate(boss.skillPref[0]).GetComponent<LineRenderer>();
            boss.attackGuildeLine.material = Instantiate(boss.attackGuildeLine.material);
            boss.attackGuildeLine.gameObject.SetActive(false);
        }
        if ((boss.skill & BossSkill.Dash_Triple) == BossSkill.Dash_Triple)
        {
            boss.skillList.Add("Dash_Triple");
            boss.attackGuildeLine = Instantiate(boss.skillPref[0]).GetComponent<LineRenderer>();
            boss.attackGuildeLine.material = Instantiate(boss.attackGuildeLine.material);
            boss.attackGuildeLine.gameObject.SetActive(false);
        }
        if ((boss.skill & BossSkill.Fire) == BossSkill.Fire)
        {
            boss.skillList.Add("Fire");
        }
        if ((boss.skill & BossSkill.Jump_1Wave) == BossSkill.Jump_1Wave)
        {
            boss.skillList.Add("Jump_1Wave");
            boss.attackGuideCircles.Add(Instantiate(boss.skillPref[1]).GetComponent<SpriteRenderer>());
            boss.attackGuideCircles[0].gameObject.SetActive(false);
            boss.jumpAttackBullets.Add(Instantiate(boss.skillPref[2]).GetComponent<EnemyBullet>());
            boss.jumpAttackBullets[0].Init(originalDamage / 3, -100, 0, Vector3.zero);
            boss.jumpAttackBullets[0].gameObject.SetActive(false);
        }
        if ((boss.skill & BossSkill.Jump_2Wave) == BossSkill.Jump_2Wave)
        {
            boss.skillList.Add("Jump_2Wave");
            boss.attackGuideCircles.Add(Instantiate(boss.skillPref[1]).GetComponent<SpriteRenderer>());
            boss.attackGuideCircles.Add(Instantiate(boss.skillPref[3]).GetComponent<SpriteRenderer>());
            boss.attackGuideCircles[0].gameObject.SetActive(false);
            boss.attackGuideCircles[1].gameObject.SetActive(false);
            boss.jumpAttackBullets.Add(Instantiate(boss.skillPref[2]).GetComponent<EnemyBullet>());
            boss.jumpAttackBullets[0].Init(originalDamage / 3, -100, 0, Vector3.zero);
            boss.jumpAttackBullets[0].gameObject.SetActive(false);
            boss.jumpAttackBullets.Add(Instantiate(boss.skillPref[4]).GetComponent<EnemyBullet>());
            boss.jumpAttackBullets[1].Init(originalDamage / 3, -100, 0, Vector3.zero);
            boss.jumpAttackBullets[1].gameObject.SetActive(false);
        }
        if ((boss.skill & BossSkill.AirStrike) == BossSkill.AirStrike)
        {
            boss.skillList.Add("AirStrike");
        }
        if ((boss.skill & BossSkill.FlowerBullet) == BossSkill.FlowerBullet)
        {
            boss.skillList.Add("FlowerBullet");
        }
        if ((boss.skill & BossSkill.Swing) == BossSkill.Swing)
        {
            boss.skillList.Add("Swing");
        }
        if ((boss.skill & BossSkill.SwingWave) == BossSkill.SwingWave)
        {
            boss.skillList.Add("SwingWave");
        }
    }
    public void UseSkill(string name)
    {
        anim.SetBool("Run", false);
        boss.state_skill = true;
        switch (name)
        {
            case "Dash_Once":
                StartCoroutine(Boss_DashRoutine(1));
                break;
            case "Dash_Triple":
                StartCoroutine(Boss_DashRoutine(3));
                break;
            case "Fire":
                StartCoroutine(Boss_FireRoutine());
                break;
            case "Jump_1Wave":
                boss.attackGuideCircles[0].gameObject.SetActive(true);
                boss.attackGuideCircles[0].color = new Color(1, 1, 1, 1);
                StartCoroutine(Boss_JumpRoutine());
                break;
            case "Jump_2Wave":
                boss.attackGuideCircles[0].gameObject.SetActive(true);
                boss.attackGuideCircles[0].color = new Color(1, 1, 1, 1);
                StartCoroutine(Boss_JumpRoutine2());
                break;
            case "AirStrike":
                StartCoroutine(Boss_AirStrikeRoutine());
                break;
            case "FlowerBullet":
                StartCoroutine(Boss_FlowerRoutine());
                break;
            case "Swing":
                break;
            case "SwingWave":
                break;
        }
    }
    IEnumerator Boss_DashRoutine(int count)
    {
        for (int i = 0; i < count; i++)
        {
            boss.attackGuildeLine.gameObject.SetActive(true);
            boss.attackGuildeLine.material.color = new Color(1, 1, 1, 1);
            while (boss.state_skill) // 돌진 준비
            {
                boss.timer_skill += Time.deltaTime;
                if (boss.timer_skill < 1f)
                {
                    boss.attackGuildeLine.SetPosition(0, transform.position);
                    boss.attackGuildeLine.SetPosition(1, target.position);
                    boss.destination = target.position;
                }
                else if (boss.timer_skill >= 1f && boss.timer_skill < 1.5f)
                {
                    if(boss.attackGuildeLine.material.color.a > 0)
                        boss.attackGuildeLine.material.color -= new Color(0, 0, 0, 1) * Time.deltaTime * 5f;                    
                }
                else
                {
                    boss.attackGuildeLine.gameObject.SetActive(false);
                    rigid.velocity = (boss.destination - transform.position).normalized * this.speed * 14f;
                    break;
                }
                yield return null;
            }
            anim.SetBool("Dash", true);
            while (boss.state_skill) // 돌진 중
            {
                boss.timer_skill += Time.deltaTime;
                if (boss.timer_skill > 2f)
                {
                    anim.SetBool("Dash", false);
                    boss.timer_skill = 0;
                    rigid.velocity = Vector3.zero;
                    if (i == count - 1)
                        boss.state_skill = false;
                    break;
                }
                yield return null;
            }
        }
    }
    IEnumerator Boss_JumpRoutine()
    {
        damage = 0;
        coll.isTrigger = true;
        boss.destination = target.position;
        boss.attackGuideCircles[0].gameObject.SetActive(true);
        boss.attackGuideCircles[0].transform.position = boss.destination;
        boss.p1 = new Vector3(transform.position.x + 0.33f * (boss.destination.x - transform.position.x), transform.position.y + 5, 0);
        boss.p2 = new Vector3(transform.position.x + 0.66f * (boss.destination.x - transform.position.x), boss.destination.y + 5, 0);
        while (boss.state_skill)
        {
            boss.timer_skill += Time.deltaTime;

            if (boss.timer_skill >= 1.8f)
            {
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                    anim.SetBool("Jump", true);
                boss.value += Time.deltaTime * 2f;
                Vector3 nextPos = BazierMath.Lerp(transform.position, boss.p1, boss.p2, boss.destination, boss.value);
                rigid.position = nextPos;
                boss.attackGuideCircles[0].color -= new Color(0, 0, 0, 1) * Time.deltaTime * 5f;
            }
            if (boss.value >= 1)
            {
                rigid.velocity = Vector2.zero;
                if (!boss.jumpAttackBullets[0].gameObject.activeSelf)
                {
                    boss.jumpAttackBullets[0].transform.position = boss.destination;
                    boss.jumpAttackBullets[0].gameObject.SetActive(true);
                }
                break;
            }
            yield return null;
        }
        yield return new WaitForSeconds(0.2f);
        if (isLive)
        {
            Boss_Fire_Round(Random.Range(0, 45));
            anim.SetBool("Jump", false);
            boss.value = 0;
            boss.attackGuideCircles[0].gameObject.SetActive(false);
            boss.jumpAttackBullets[0].gameObject.SetActive(false);
            damage = originalDamage;
            coll.isTrigger = false;
            boss.state_skill = false;
            boss.timer_skill = 0;
        }        
    }
    IEnumerator Boss_JumpRoutine2()
    {
        damage = 0;
        coll.isTrigger = true;
        boss.destination = target.position;
        boss.attackGuideCircles[0].gameObject.SetActive(true);
        boss.attackGuideCircles[0].transform.position = boss.destination;
        boss.p1 = new Vector3(transform.position.x + 0.33f * (boss.destination.x - transform.position.x), transform.position.y + 5, 0);
        boss.p2 = new Vector3(transform.position.x + 0.66f * (boss.destination.x - transform.position.x), boss.destination.y + 5, 0);
        while (boss.state_skill) // 점프 시작 ~ 1단 웨이브
        {
            boss.timer_skill += Time.deltaTime;

            if (boss.timer_skill >= 1.8f)
            {
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                    anim.SetBool("Jump", true);
                boss.value += Time.deltaTime * 2f;
                Vector3 nextPos = BazierMath.Lerp(transform.position, boss.p1, boss.p2, boss.destination, boss.value);
                rigid.position = nextPos;
                boss.attackGuideCircles[0].color -= new Color(0, 0, 0, 1) * Time.deltaTime * 5f;
            }
            if (boss.value >= 1)
            {
                rigid.velocity = Vector2.zero;
                if (!boss.jumpAttackBullets[0].gameObject.activeSelf)
                {
                    boss.jumpAttackBullets[0].transform.position = boss.destination;
                    boss.jumpAttackBullets[0].gameObject.SetActive(true);
                }
                break;
            }
            yield return null;
        }
        yield return new WaitForSeconds(0.3f);
        if (isLive)
        {
            boss.attackGuideCircles[0].gameObject.SetActive(false);         // 1단 웨이브 제거
            boss.jumpAttackBullets[0].gameObject.SetActive(false);
            boss.timer_skill = 0;
            boss.attackGuideCircles[1].transform.position = boss.destination;
            boss.attackGuideCircles[1].gameObject.SetActive(true);
            boss.attackGuideCircles[1].color = new Color(1, 1, 1, 1);       // 2단 웨이브 시작
        }            
        while (boss.state_skill && isLive)
        {
            boss.timer_skill += Time.deltaTime;

            if (boss.timer_skill >= 0.5f)
                boss.attackGuideCircles[1].color -= new Color(0, 0, 0, 1) * Time.deltaTime * 4f;

            if (boss.attackGuideCircles[1].color.a <= 0)
            {
                if (!boss.jumpAttackBullets[1].gameObject.activeSelf)
                {
                    boss.jumpAttackBullets[1].transform.position = boss.destination;
                    boss.jumpAttackBullets[1].gameObject.SetActive(true);
                }
                break;
            }
            yield return null;
        }
        yield return new WaitForSeconds(0.3f);
        if (isLive)
        {
            anim.SetBool("Jump", false);
            boss.attackGuideCircles[1].gameObject.SetActive(false);
            boss.jumpAttackBullets[1].gameObject.SetActive(false);
            boss.value = 0;
            damage = originalDamage;
            coll.isTrigger = false;
            boss.state_skill = false;
            boss.timer_skill = 0;
        }            
    }
    IEnumerator Boss_FireRoutine()
    {
        for (int i = 0; i < 5; i++)
        {
            anim.SetTrigger("Spit");
            Boss_Fire_Round(i * 15);
            yield return new WaitForSeconds(0.2f);
        }
        boss.state_skill = false;
    }
    public void Boss_Fire_Round(int degree)
    {
        foreach (Vector3 pos in boss.firePos)
        {
            EnemyBullet bullet = inGameManager.pool.Get(35).GetComponent<EnemyBullet>();
            bullet.transform.position = transform.position;
            bullet.Init(originalDamage / 4, 1, 10, Quaternion.AngleAxis(degree, Vector3.forward) * pos.normalized);
        }
    }
    Quaternion LookAt2D(Vector2 forward)
    {
        return Quaternion.Euler(0, 0, Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg);
    }
    IEnumerator Boss_FlowerRoutine()
    {
        anim.SetTrigger("Spit");
        for (int i = 1; i < boss.firePos.Length; i++)
        {
            if (i % 2 == 1)
            {
                EnemyBullet bullet = inGameManager.pool.Get(36).GetComponent<EnemyBullet>();
                bullet.transform.position = transform.position;
                bullet.Init(originalDamage / 3, 1, 5, Quaternion.AngleAxis(36, Vector3.forward) * boss.firePos[i].normalized);
                bossBullets.Add(bullet);
            }
        }
        yield return new WaitForSeconds(1f);
        if (isLive)
        {
            for (int i = bossBullets.Count - 1; i >= 0; i--)
            {
                for (int j = 0; j < 10; j++)
                {
                    EnemyBullet flowerBullet = inGameManager.pool.Get(35).GetComponent<EnemyBullet>();
                    flowerBullet.transform.position = bossBullets[i].transform.position;
                    flowerBullet.Init(originalDamage / 4, 1, 10, Quaternion.AngleAxis(36 * j, Vector3.forward) * Vector3.up);
                }
                bossBullets[i].gameObject.SetActive(false);
                bossBullets.Remove(bossBullets[i]);
            }
            boss.state_skill = false;
        }       
    }
    public void Boss_Fire_Flower()
    {
        for (int i = 1; i < boss.firePos.Length; i++)
        {
            if (i % 2 == 1)
            {
                EnemyBullet bullet = inGameManager.pool.Get(36).GetComponent<EnemyBullet>();
                bullet.transform.position = transform.position;
                bullet.Init(originalDamage / 4, 1, 10, Quaternion.AngleAxis(36, Vector3.forward) * boss.firePos[i].normalized);
            }
        }
    }
    IEnumerator Boss_AirStrikeRoutine()
    {
        anim.SetBool("AirStrike", true);
        Vector3 center = inGameManager.bossArea.transform.position;
        for (int i = 0; i < 30; i++)
        {
            if (isLive)
            {
                Vector3 randomPos = inGameManager.player.transform.position + new Vector3(Random.Range(-8f, 8f), Random.Range(-8f, 8f), 0);
                while (Vector3.Distance(center, randomPos) > 20)
                {
                    randomPos = inGameManager.player.transform.position + new Vector3(Random.Range(-8f, 8f), Random.Range(-8f, 8f), 0);
                }
                inGameManager.pool.Get(37).transform.position = randomPos;
                ProjectileMotion newProjectile = inGameManager.pool.Get(38).GetComponent<ProjectileMotion>();
                newProjectile.Init(false, transform.position, randomPos, 39, damage / 4, 5, 1, 10, false);
                yield return new WaitForSeconds(0.1f);
            }            
        }
        anim.SetBool("AirStrike", false);
        boss.state_skill = false;
    }
}
