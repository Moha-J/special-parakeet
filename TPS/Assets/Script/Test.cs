using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class Test : MonoBehaviour
{
    public static Canvas canvas;
    RectTransform rectTransform;
    public static int id;
    // Start is called before the first frame update
    void Start()
    {
        
        canvas = GetComponent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.width);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Screen.height);

        BattleManager.Init();
        NetManager.AddMsgListener("MsgLogin", OnMsgLogin);
        NetManager.Connect("111.229.10.22", 8888);
        ObjectPool.me.Preload(ResManager.LoadPrefab("bullet"), 200);
        ObjectPool.me.Preload(ResManager.LoadPrefab("fire"), 200);
        
        
    }
    void Update()
    {
        NetManager.Update();
    }
    public static void Login()
    {
        
        MsgLogin msgLogin = new MsgLogin();
        NetManager.Send(msgLogin);
    }
    public void OnMsgLogin(MsgBase msgBase)
    {
        MsgLogin msg = (MsgLogin)msgBase;
        id = msg.id;
    }
}
