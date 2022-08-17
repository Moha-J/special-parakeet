using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SyncCharacter: BaseCharacter
{
	private Vector3 lastPos;
	private Vector3 lastRot;
	private Vector3 forecastPos;
	private Vector3 forecastRot;
	private float lastTurretY;
	private float forecastTurretY;
	private float lastGunX;
	private float forecastGunX;
	private float forecastTime;
	public override void Init(string skinPath)
	{
		
		base.Init(skinPath);
		
		//初始化预测信息
		lastPos = transform.position;
		lastRot = transform.eulerAngles;
		forecastPos = transform.position;
		forecastRot = transform.eulerAngles;
		lastGunX = gun.localEulerAngles.x;
		forecastGunX = gun.localEulerAngles.x;
		forecastTime = Time.time;
	}
	new void Update()
	{
		base.Update();
		//更新位置
		ForecastUpdate();
	}
	//移动同步
	public void SyncPos(MsgSyncCharacter msg)
	{
		//预测位置
		Vector3 pos = new Vector3(msg.x, msg.y, msg.z);
		Vector3 rot = new Vector3(msg.ex, msg.ey, msg.ez);
		//forecastPos = pos + 2*(pos - lastPos);
		//forecastRot = rot + 2*(rot - lastRot);
		forecastPos = pos;  //跟随不预测
		forecastRot = rot;
		forecastGunX = msg.gunX;
		//更新
		lastPos = pos;
		lastRot = rot;
		lastGunX = gun.localEulerAngles.x;
		forecastTime = Time.time;
	}

	public void ForecastUpdate()
	{
		//时间
		float t = (Time.time - forecastTime) / CtrlCharacter.syncInterval;
		t = Mathf.Clamp(t, 0f, 1f);
		//位置
		Vector3 pos = transform.position;
		pos = Vector3.Lerp(pos, forecastPos, t);
		transform.position = pos;
		//旋转
		Quaternion quat = transform.rotation;
		Quaternion forcastQuat = Quaternion.Euler(forecastRot);
		quat = Quaternion.Lerp(quat, forcastQuat, t);
		transform.rotation = quat;
		//炮塔
		Vector3 le = gun.localEulerAngles;
		le.x = Mathf.LerpAngle(le.x, forecastGunX, t);
		gun.localEulerAngles = le;
	}
	public void SyncFire(MsgFire msg)
	{
		Bullet bullet = Fire();
		//更新坐标
		Vector3 pos = new Vector3(msg.x, msg.y, msg.z);
		Vector3 rot = new Vector3(msg.ex, msg.ey, msg.ez);
		bullet.transform.position = pos;
		bullet.transform.eulerAngles = rot;
		bullet.character = this;
	}
}

