using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    //相机相对位置
    public Camera camera;
    public Transform cameraP;
    public Vector3 pos;
    public float angle;
    public Vector3 checkE;
    public Vector3 lastD,newD;
    public CtrlCharacter cc;
    Quaternion q;
    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;
        //初始化相机位置
        pos = transform.position - transform.forward * 3f + transform.up * 2f + transform.right * 0.8f;
        camera.transform.position = pos;
        //把相机作为控制角色的子对象
        cameraP = gameObject.transform.GetChild(1);
        camera.transform.parent = cameraP;
        //获取控制角色对象的控制角色类
        cc = transform.GetComponent<CtrlCharacter>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //瞄准
        if (cc.isAim)
        {
            camera.transform.localPosition = Vector3.zero;
            cameraP.transform.localPosition = new Vector3(0,1.2f,0);
            camera.fieldOfView = 6f;
        }
        else
        {
            camera.transform.localPosition = new Vector3(0.8f, 2f, -3f);
            cameraP.transform.localPosition = Vector3.zero;
            camera.fieldOfView = 53f;
        }
        //赋予相机X轴角度，也就是上下旋转，变量 angle 在控制角色类里赋值
        checkE = new Vector3(angle, 0, 0);
        q = Quaternion.Euler(checkE);
        cameraP.transform.localRotation = q;
    }
}
