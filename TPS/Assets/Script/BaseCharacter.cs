using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCharacter : MonoBehaviour
{
    public GameObject skin;
    protected CharacterController characterC;
    public Vector3 aimPoint;
    public Ray aimRay,bulletRay;
    public int id = 0;
    //移动
    protected Vector3 moveDirection;
    //移动速度
    public float speed = 10f;
    //跳跃速度
    public float jumpSpeed = 4f;
    //旋转速度
    public float rotateSpeed = 3f;
    //枪
    public Transform gun;
    //射击点
    public Transform firePoint;
    //子弹数量
    public int bulletCountMax = 30;
    public int bulletCount = 30;
    //子弹CD时间
    public float fireCd = 0.1f;
    public float lastFireTIme = 0;
    //装填时间
    public bool isReRoll = false;
    public float reRollTime = 2.5f;
    public float theReRollTime = 0;
    //生命值
    public float hp = 100;
    //武器对象
    public Weapon weapon;
    //身上的装备
    public Dictionary<int, Item> guns;
    //当前使用的武器栏位
    public int number = 1;
    //瞄准方向
    public Vector3 aimRotation;
    //瞄准点
    public RaycastHit hit;
    //瞄准射击偏移
    public Vector3 aimOffset;
    //偏移目标值
    public Vector3 aimOffsetTarget;

    //是否在瞄准状态
    public bool isAim;
    //武器方向
    public Vector3 gunRotation;
    //瞄准时装弹
    public bool isAimReRoll;
    //瞄准时切换武器
    public bool isAimSwitch;
    //切换武器
    int theI;
    public bool isSwitch = false;
    public float SwitchTime = 1.2f;
    public float theSwitchTime = 0;
    //结束射击时的时间
    public float time2;
    //射击状态
    public bool isFire;
    //复活时间
    public float rebirth = 3f;
    public float lastDieTime;
    //下一次复活点
    Vector3 rePosition;
    //是否死亡
    public bool IsDie()
    {
        return hp <= 0;
    }
    //重生
    public void Die()
    {
        if (IsDie())
        {
            rePosition = new Vector3(Random.Range(-80, 80), 35, Random.Range(-80, 80));
            if (Time.time - lastDieTime + Time.deltaTime * 4 >= rebirth)
            {
                transform.eulerAngles = new Vector3(0, 0, 0);
                transform.position = rePosition;

            }
            if (Time.time - lastDieTime >= rebirth)
            {
                hp = 100;
            }
        }
        else
        {
            lastDieTime = Time.time;
        }
    }
    // Start is called before the first frame update
    protected void Start()
    {
        
        guns = new Dictionary<int, Item>();
        guns[1] = new Item();
        guns[1].weapon = Weapon.GetWeapon(1);
        guns[1].bulletCount = guns[1].weapon.GetPrimaryClipSize();
        guns[2] = new Item();
        guns[2].weapon = Weapon.GetWeapon(2);
        guns[2].bulletCount = guns[2].weapon.GetPrimaryClipSize();
        guns[3] = new Item();
        guns[3].weapon = Weapon.GetWeapon(3);
        guns[3].bulletCount = guns[3].weapon.GetPrimaryClipSize();
        weapon = guns[number].weapon;
        aimPoint = transform.position + transform.forward * 100f;
        fireCd = weapon.GetCycleTime();
    }
    public virtual void Init(string skinPath)
    {
        GameObject skinRes = ResManager.LoadPrefab(skinPath);
        skin = (GameObject)Instantiate(skinRes);
        skin.transform.parent = this.transform;
        skin.transform.localPosition = Vector3.zero;
        skin.transform.localEulerAngles = Vector3.zero;
        //物理

        characterC = gameObject.AddComponent<CharacterController>();
        characterC.center = new Vector3(0, 1, 0);

        //枪管
        gun = skin.transform.Find("Gun");
        firePoint = gun.transform.Find("GunBody").Find("FirePoint");
    }

    // Update is called once per frame
    protected void Update()
    {
        
        
        ReRollEnd();
        SwitchEnd(theI);
        Die();
    }

    //切换武器
    public void Switch(int i)
    {
        if (IsDie())
        {
            return;
        }
        if (isAim)
        {
            isAimSwitch = true;
        }
        isReRoll = false;
        isSwitch = true;
        theI = i;
        theSwitchTime = Time.time;
    }
    //切换完成
    public void SwitchEnd(int i)
    {
        if (IsDie())
        {
            return;
        }
        if (!isSwitch)
        {
            return;
        }
        if(Time.time - theSwitchTime > SwitchTime)
        {
            Debug.Log("切换武器为：" + guns[i].weapon.GetweaponName());
            guns[number].bulletCount = bulletCount;
            weapon = guns[i].weapon;
            fireCd = weapon.GetCycleTime();
            bulletCountMax = weapon.GetPrimaryClipSize();
            bulletCount = guns[i].bulletCount;
            number = i;
            isSwitch = false;
        }
    }
    //开火
    public Bullet Fire()
    {
        //已经死亡
        if (IsDie())
        {
            return null;
        }
        if (bulletCount == 0)
        {
            return null;
        }
        GameObject bulletRes = ResManager.LoadPrefab("bullet");
        GameObject bulletObj = ObjectPool.me.GetObject(bulletRes,firePoint.position,Quaternion.LookRotation(aimPoint));
        
        bulletObj.layer = 2;
        Bullet bullet = bulletObj.AddComponent<Bullet>();
        bullet.Init();
        bullet.character = this;
        BulletSet(bullet);
        //播放声音
        GameObject audioRes = guns[number].weapon.GetAudioRes();
        GameObject audio = ObjectPool.me.GetObject(audioRes, firePoint.position, Quaternion.identity);
        ObjectPool.me.PutObject(audio, 3f);
        bulletCount -= 1;


        return bullet;
    }
    public virtual void BulletSet(Bullet bullet) {  }
    //瞄准
    public void Aim()
    {
        if (IsDie())
        {
            return;
        }
        if (isAim)
        {
            isAim = false;
            gun.localPosition = new Vector3(0.4f, 1.2f, 0.314f);
        }
        else
        {
            isAim = true;
            gun.localPosition = new Vector3(0, 1.2f, 0.314f);
        }
        
    }
    //装弹
    public void ReRoll()
    {
        if (IsDie())
        {
            return;
        }
        if (bulletCount == bulletCountMax)
        {
            return;
        }
        if (isAim)
        {
            isAimReRoll = true;
        }
        if (isReRoll)
        {
            return;
        }
        if (isSwitch)
        {
            return;
        }
        //装弹状态中
        isReRoll = true;
        theReRollTime = Time.time;
    }
    //装弹完成
    public void ReRollEnd()
    {
        if (IsDie())
        {
            return;
        }
        if (!isReRoll)
        {
            return;
        }
        if (Time.time - theReRollTime > reRollTime)
        {
            Debug.Log("装弹");
            bulletCount = bulletCountMax;
            isReRoll = false;
        }
    }

    //受到攻击
    public void Attacked(float att)
    {
        if (IsDie())
        {
            return;
        }
        //扣血
        hp -= att;
        //死亡
        if (IsDie())
        {
            GameObject obj = ResManager.LoadPrefab("explosion");
            GameObject expLosion = Instantiate(obj, transform.position, transform.rotation);
            transform.eulerAngles = new Vector3(90f,0,90f);
            transform.position += Vector3.up;
            expLosion.AddComponent<ParticleSelf>();
            moveDirection = Vector3.zero;
            expLosion.transform.SetParent(transform);
        }
    }
}
