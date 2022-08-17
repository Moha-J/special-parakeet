using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    //子弹当前速度向量
    public Vector3 bulletMoveDirection;
    //子弹速度
    public float speed;
    //子弹生命时间
    public float bulletSelfTime = 3f;
    public float fireTime = 0;
    //子弹伤害
    int damege;
    //发射者
    public BaseCharacter character;
    //子弹模型
    private GameObject skin;
    //子弹射线
    Ray ray;
    Vector3 lastPosition, newPosition;
    RaycastHit hit;
    //初始化
    public void Init()
    {
        GameObject skinRes = ResManager.LoadPrefab("bulletPrefab");
        skin = (GameObject)Instantiate(skinRes);
        skin.transform.parent = transform;
        skin.transform.localPosition = Vector3.zero;
        skin.transform.localEulerAngles = Vector3.zero;
        fireTime = Time.time;
    }
    // Start is called before the first frame update
    void Start()
    {
        //获取角色使用的武器中的属性
        speed = character.weapon.GetSpeed();
        damege = character.weapon.GetDamege();
        //设定子弹的方向与速度
        bulletMoveDirection = transform.forward * speed;
        lastPosition = character.firePoint.position;
    }
    // Update is called once per frame
    void Update()
    {
        //炮弹飞行时间过长自动销毁
        if(Time.time-fireTime >bulletSelfTime)
        {
            ObjectPool.me.PutObject(gameObject, 0);
            Destroy(this);
        }
        bulletMoveDirection += Physics.gravity * Time.deltaTime;
        transform.position += bulletMoveDirection * Time.deltaTime;
        newPosition = transform.position;
        //产生一条上一帧到这一帧的射线，可以避免子弹速度过快穿过模型
        ray = new Ray(lastPosition, newPosition - lastPosition);
        Debug.DrawLine(lastPosition, newPosition,Color.red);
        //射线检测
        if (Physics.Raycast(ray,out hit,Vector3.Distance(lastPosition,newPosition)))
        {
            GameObject collObj = hit.collider.gameObject;
            Debug.Log(hit.collider);
            BaseCharacter hitCharacter = collObj.GetComponent<BaseCharacter>();
            //受到攻击的对象为角色
            if (hit.collider != null)
            {
                if (hitCharacter != null)
                {

                    if (hitCharacter != character)
                    {
                        if (character.id == Test.id)
                        {
                            //调用该角色脚本组件中的受击方法
                            MsgHit msg = new MsgHit();
                            msg.damage = damege;
                            msg.targetId = hitCharacter.id;
                            msg.id = character.id;
                            Debug.Log("膜" + msg.id);
                            NetManager.Send(msg);

                        }
                    }
                }
                Delete();
                return;
            }
        }
        lastPosition = newPosition;
    }
    void Delete()
    {
        //播放特效与删除自身
        GameObject explodeRes = ResManager.LoadPrefab("fire");
        GameObject explode = ObjectPool.me.GetObject(explodeRes, transform.position, transform.rotation);
        ObjectPool.me.PutObject(explode, 1f);
        ObjectPool.me.PutObject(gameObject, 0);
        Destroy(this);
    }
}
