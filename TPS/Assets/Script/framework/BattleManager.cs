
using System.Collections.Generic;
using UnityEngine;

public class BattleManager
{
	//战场中的玩家
	public static Dictionary<int, BaseCharacter> characters = new Dictionary<int, BaseCharacter>();

	//初始化
	public static void Init()
	{
		//添加监听
		NetManager.AddMsgListener("MsgAddPlayer", OnMsgAddPlayer);
		NetManager.AddMsgListener("MsgLeaveBattle", OnMsgLeaveBattle);

		NetManager.AddMsgListener("MsgSyncCharacter", OnMsgSyncCharacter);
		NetManager.AddMsgListener("MsgFire", OnMsgFire);
		NetManager.AddMsgListener("MsgHit", OnMsgHit);
	}
	//
	public static void SetCharacters()
    {

    }
	//获取角色
	public static BaseCharacter GetCharacter(int id)
	{
		if (characters.ContainsKey(id))
		{
			return characters[id];
		}
		return null;
	}
	public static void AddCharacter(int id, BaseCharacter character)
	{
		characters[id] = character;
	}
	//删除角色
	public static void Removecharacter(int id)
	{
		characters.Remove(id);
	}
	//玩家进入
	public static void OnMsgAddPlayer(MsgBase msgBase)
    {
		
		MsgAddPlayer msg = (MsgAddPlayer)msgBase;
		GenerateCharacter(msg.id,msg.x,msg.y,msg.z);
	}
	//生成角色
	public static void GenerateCharacter(int id,float x,float y,float z)
    {
        if (id == Test.id)
        {
            CtrlCharacter ctrlCharacter;
            GameObject CharacterObj = new GameObject("myCharacter");
			ctrlCharacter = CharacterObj.AddComponent<CtrlCharacter>();
			ctrlCharacter.Init("Player");
			GameObject cameraP = new GameObject("CameraP");
			cameraP.transform.parent = CharacterObj.transform;
			ctrlCharacter.transform.position = new Vector3(x,y,z);
			ctrlCharacter.canvas = Test.canvas;
			ctrlCharacter.id = id;
			CharacterObj.AddComponent<CameraFollow>();
			AddCharacter(id, ctrlCharacter);
		}
        else
        {
			Debug.Log("对手进入房间");
			GameObject CharacterObj = new GameObject("enemyCharacter");
            BaseCharacter character = CharacterObj.AddComponent<SyncCharacter>();
            character.Init("Player");
			character.transform.position = new Vector3(x, y, z);
			character.id = id;
			AddCharacter(id, character);
		}
	}
	//收到玩家退出协议
	public static void OnMsgLeaveBattle(MsgBase msgBase)
	{
		MsgLeaveBattle msg = (MsgLeaveBattle)msgBase;
		//查找玩家
		BaseCharacter character = GetCharacter(msg.id);
		if (character == null)
		{
			return;
		}
		//删除玩家
		Removecharacter(msg.id);
		MonoBehaviour.Destroy(character.gameObject);
	}
	
	public static void OnMsgSyncCharacter(MsgBase msgBase)
    {
		MsgSyncCharacter msg = (MsgSyncCharacter)msgBase;
		//不同步自己
		if (msg.id == Test.id)
		{
			return;
		}
		//查找玩家
		SyncCharacter character = (SyncCharacter)GetCharacter(msg.id);
		if (character == null)
		{
			return;
		}
		//移动同步
		character.SyncPos(msg);
	}
	//收到开火协议
	public static void OnMsgFire(MsgBase msgBase)
	{
		MsgFire msg = (MsgFire)msgBase;
		//不同步自己
		if (msg.id == Test.id)
		{
			return;
		}
		//查找玩家
		SyncCharacter character = (SyncCharacter)GetCharacter(msg.id);
		if (character == null)
		{
			return;
		}
		//开火
		character.SyncFire(msg);
	}
	//收到击中协议
	public static void OnMsgHit(MsgBase msgBase)
	{
		MsgHit msg = (MsgHit)msgBase;
		//查找玩家
		BaseCharacter character = GetCharacter(msg.targetId);
		if (character == null)
		{
			return;
		}
		bool isDie = character.IsDie();
		//被击中
		character.Attacked(msg.damage);
	}
}
