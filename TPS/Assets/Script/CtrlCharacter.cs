using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CtrlCharacter : BaseCharacter
{
    Vector3 centerPoint = new Vector3(Screen.width / 2, Screen.height / 2, 0);
    private GameObject hpobj, bulletCountobj, bulletCountMaxobj;
    private Text hpT, bulletCountT, bulletCountMaxT;
    GameObject darkRes, crossHairRes, aimRes, dark,crossHair, aim;
    public Canvas canvas;
    CameraFollow cF;
    int count = 0;
    //上一次发送同步信息的时间
    private float lastSendSyncTime = 0;
    //同步帧率
    public static float syncInterval = 0.02f;
    //后坐力
    float inaccuracy;
    float fRand1, fRand2, fRandPi1, fRandPi2, fRandInaccuracy, fRandSpread, x, y;
    //随机种子
    System.Random randomSpread;
    System.Random randomRecoil;
    int b;
    //水平基础后坐力
    float recoil = 0;
    //水平后坐力系数
    float R = 0;
    //后坐力恢复速度
    float Recovery = 1;


    new void Start()
    {
        base.Start();
        randomSpread = new System.Random();
        weapon = Weapon.GetWeapon(1);
        Cursor.lockState = CursorLockMode.Locked;
        
        hpobj = GameObject.Find("Hp");
        bulletCountobj = GameObject.Find("BulletCount");
        bulletCountMaxobj = GameObject.Find("BulletCountMax");
        hpT = hpobj.GetComponent<Text>();
        bulletCountT = bulletCountobj.GetComponent<Text>();
        bulletCountMaxT = bulletCountMaxobj.GetComponent<Text>();
        darkRes = ResManager.LoadPrefab("Dark");
        crossHairRes = ResManager.LoadPrefab("CrossHair");
        aimRes = ResManager.LoadPrefab("Aim");
        aim = ObjectPool.me.GetObject(aimRes, Vector3.zero, Quaternion.identity);
        aim.transform.parent = canvas.transform;
        aim.transform.localPosition = Vector3.zero;
        cF = GetComponent<CameraFollow>();
    }
    // Update is called once per frame
    new void Update()
    {
        aimOffset = Vector3.Lerp(aimOffset, aimOffsetTarget, 0.1f);
        //武器方向
        gunRotation.x = Mathf.Clamp(aimRotation.x + aimOffset.x, -90, 90);
        gunRotation.y = aimRotation.y + aimOffset.y;
        gun.rotation = Quaternion.Euler(gunRotation);

        ReRollUpdate();
        base.Update();
        //不在开火状态
        if (!isFire)
        {
            //停止射击两个间隔时间后
            if (Time.time - time2 > fireCd * 2)
            {
                recoil = 0;
                b = weapon.GetRecoilSeed();
                R = 0;
            }
        }
        //水平后坐力恢复
        if (aimOffsetTarget.y > 0)
        {
            aimOffsetTarget.y = Mathf.Clamp(aimOffsetTarget.y - Recovery * Time.deltaTime * 10, 0, 90);
        }
        else
        {
            aimOffsetTarget.y = Mathf.Clamp(aimOffsetTarget.y + Recovery * Time.deltaTime * 10, -90, 0);
        }
        //垂直后坐力恢复
        aimOffsetTarget.x = Mathf.Clamp(aimOffsetTarget.x + Recovery * Time.deltaTime * 10, -90, 0);
        if (aimOffsetTarget == Vector3.zero)
        {
            Recovery = 1;
        }
        MoveUpdate();
        GunUpdate();
        FireUptate();
        AimPoint();
        TextSet();
        SwitchUpdate();
        SyncUpdate();
    }
    //移动上传
    public void MoveUpdate()
    {
        if (IsDie())
        {
            return;
        }
        //站在地上
        if (characterC.isGrounded)
        {
            //获取移动数据
            moveDirection.x = Input.GetAxis("Horizontal") * speed;
            moveDirection.z = Input.GetAxis("Vertical") * speed;
            moveDirection = transform.TransformDirection(moveDirection);
            //移动速度限制
            if (Vector3.Distance(Vector3.zero, moveDirection) > speed)
            {
                moveDirection /= Vector3.Distance(Vector3.zero, moveDirection)/speed;
            }
            if (Input.GetButton("Jump"))
                moveDirection.y = jumpSpeed;
        }
        else
        {
            moveDirection += Physics.gravity * Time.deltaTime;
        }
        characterC.Move(moveDirection * Time.deltaTime);
        //控制旋转
        aimRotation += new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0) * rotateSpeed;
        aimRotation.x = Mathf.Clamp(aimRotation.x, -90, 90);
        cF.angle = aimRotation.x + aimOffset.x;
        cF.angle = Mathf.Clamp(cF.angle, -90, 90);
        transform.rotation = Quaternion.Euler(0, aimRotation.y + aimOffset.y, 0);
    }
    public void SyncUpdate()
    {
        //时间间隔判断
        if (Time.time - lastSendSyncTime < syncInterval)
        {
            return;
        }
        lastSendSyncTime = Time.time;
        //发送同步协议
        MsgSyncCharacter msg = new MsgSyncCharacter();
        msg.x = transform.position.x;
        msg.y = transform.position.y;
        msg.z = transform.position.z;
        msg.ex = transform.eulerAngles.x;
        msg.ey = transform.eulerAngles.y;
        msg.ez = transform.eulerAngles.z;
        msg.gunX = gun.localEulerAngles.x;
        NetManager.Send(msg);
    }
    //瞄准
    public void GunUpdate()
    {
        if (IsDie())
        {
            return;
        }
        //使用的武器不是狙击枪时返回
        if (weapon.GetWeaponCategory() != "sniper rifle")
        {
            return;
        }
        //如果按下右键，或者在瞄准时换弹，或者在瞄准时切换武器则通过
        if (!Input.GetMouseButtonDown(1) && !isAimReRoll && !isAimSwitch)
        {
            return;
        }
        //正常情况下按下右键时如果在换弹或切换武器时返回，而瞄准时换弹或者瞄准时切换武器则通过。
        if ((isReRoll || isSwitch) && !isAimReRoll && !isAimSwitch)
        {
            return;
        }
        //执行基类的瞄准方法，将武器坐标切换到角色中心，isAim参数切换为true或者false。
        Aim();
        //调整UI和鼠标移动速度、调整相机位置的方法在CameraFollow类中
        if (isAim)
        {
            
            dark = ObjectPool.me.GetObject(darkRes, Vector3.zero, Quaternion.identity);
            crossHair = ObjectPool.me.GetObject(crossHairRes, Vector3.zero, Quaternion.identity);
            dark.transform.parent = canvas.transform;
            crossHair.transform.parent = canvas.transform;
            dark.transform.localPosition = Vector3.zero;
            crossHair.transform.localPosition = Vector3.zero;
            dark.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.width);
            dark.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Screen.height);
            crossHair.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.height / 9 * 7);
            crossHair.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Screen.height / 9 * 7);
            ObjectPool.me.PutObject(aim,0);
            rotateSpeed /= 8;
        }
        else
        {
            aim = ObjectPool.me.GetObject(aimRes,Vector3.zero,Quaternion.identity);
            aim.transform.parent = canvas.transform;
            aim.transform.localPosition = Vector3.zero;
            ObjectPool.me.PutObject(dark, 0);
            ObjectPool.me.PutObject(crossHair, 0);
            rotateSpeed = 3f;
        }
        isAimReRoll = false;
        isAimSwitch = false;
    }
    //开火检测
    public void FireUptate()
    {
        if (IsDie())
        {
            return;
        }
        //在装弹状态
        if (isReRoll)
        {
            isFire = false;
            count = 0;
            lastFireTIme = 0;
            return;
        }
        //在切换武器状态
        if (isSwitch)
        {
            isFire = false;
            count = 0;
            lastFireTIme = 0;
            return;
        }
        //子弹为空
        if (bulletCount <= 0)
        {

            count = 0;
            lastFireTIme = 0;
            return;
        }
        //按住鼠标左键
        if (Input.GetMouseButton(0))
        {
            isFire = true;
            if (lastFireTIme == 0)
            {
                lastFireTIme = Time.time;
            }
            if ((Time.time - lastFireTIme) / fireCd >= count)
            {
                
                Bullet bullet = Fire();
                MsgFire msg = new MsgFire();
                msg.x = bullet.transform.position.x;
                msg.y = bullet.transform.position.y;
                msg.z = bullet.transform.position.z;
                msg.ex = bullet.transform.eulerAngles.x;
                msg.ey = bullet.transform.eulerAngles.y;
                msg.ez = bullet.transform.eulerAngles.z;
                NetManager.Send(msg);
                count++;
            }
        }
        else
        {
            isFire = false;
            if (Time.time - time2 >= fireCd)
            {
                    count = 0;
                    lastFireTIme = 0;
                    time2 = 0;
            }
        }
    }
    //子弹设置
    public override void BulletSet(Bullet bullet)
    {
        //位置
        bullet.transform.position = firePoint.position;
        bullet.transform.LookAt(aimPoint);
        bullet.transform.Rotate(new Vector3(-0.05f, 0, 0));
        //随机数散布
        fRand1 = (float)randomSpread.NextDouble();
        fRand2 = (float)randomSpread.NextDouble();
        fRandPi1 = (float)randomSpread.NextDouble() * 2 * Mathf.PI;
        fRandPi2 = (float)randomSpread.NextDouble() * 2 * Mathf.PI;
        if (isAim)
        {
            inaccuracy = guns[number].weapon.GetInaccuracyAlt();
        }
        else
        {
            inaccuracy = guns[number].weapon.GetInaccuracyStand();
        }
        fRandInaccuracy = fRand1 * inaccuracy;
        fRandSpread = fRand2 * guns[number].weapon.GetSpread();
        x = Mathf.Cos(fRandPi1) * fRandInaccuracy + Mathf.Cos(fRandPi2) * fRandSpread;
        y = Mathf.Sin(fRandPi1) * fRandInaccuracy + Mathf.Sin(fRandPi2) * fRandSpread;
        x = x * 180 / Mathf.PI / 666;
        y = y * 180 / Mathf.PI / 666;
        bullet.transform.Rotate(new Vector3(x, y, 0));
        //垂直后坐力系数
        aimOffsetTarget.x -= guns[number].weapon.GetRecoilAngle();



        //限制最大值为 每秒子弹数/10 * 垂直后坐力系数
        Recovery = Mathf.Clamp(Recovery +
            (1 / guns[number].weapon.GetCycleTime() / 10 * guns[number].weapon.GetRecoilAngle() - 1)
            / 10, 1, 1 / guns[number].weapon.GetCycleTime() / 10 * guns[number].weapon.GetRecoilAngle());
        b += 5;
        randomRecoil = new System.Random(b);
        float fRand = (float)randomRecoil.NextDouble() * 0.5f;
        recoil += fRand;
        //最大值限制为 水平后坐力
        R = Mathf.Clamp(R + guns[number].weapon.GetRecoilMagnitudeVariance() / 10, 0, guns[number].weapon.GetRecoilMagnitudeVariance());
        if (new System.Random(guns[number].weapon.GetRecoilSeed()).NextDouble() >= 0.5f)
        {
            aimOffsetTarget.y = 2 * R * Mathf.Sin(recoil);
        }
        else
        {
            aimOffsetTarget.y = -2 * R * Mathf.Sin(recoil);
        }
        time2 = Time.time;
    }
    //切换武器监听
    public void SwitchUpdate()
    {
        if (IsDie())
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Switch(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Switch(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Switch(3);
        }
    }
    //装弹状态监听
    public void ReRollUpdate()
    {
        if (IsDie())
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReRoll();
        }
    }
    //获取瞄准点
    public void AimPoint()
    {
        if (IsDie())
        {
            return;
        }
        aimRay = Camera.main.ScreenPointToRay(centerPoint);
        if (Physics.Raycast(aimRay, out hit, 500f))
        {
            aimPoint = hit.point;
        }
        else
        {
            aimPoint = aimRay.origin + aimRay.direction * 100f;
            
        }
    }
    //设置UI界面
    public void TextSet()
    {
        hpT.text = hp + "";
        
        bulletCountMaxT.text = guns[number].weapon.GetPrimaryClipSize() + "";
        if (isReRoll)
        {
            bulletCountT.text =reRollTime - Time.time + theReRollTime+"";
        }else if (isSwitch)
        {
            bulletCountT.text = SwitchTime - Time.time + theSwitchTime + "";
        }
        else
        {
            bulletCountT.text = bulletCount + "";
        }
        
    }
}
