using System.Collections.Generic;
using System;

public partial class MsgHandler
{
	static int i = 0;
	//同步位置协议
	public static void MsgSyncCharacter(ClientState c, MsgBase msgBase)
	{
		MsgSyncCharacter msg = (MsgSyncCharacter)msgBase;
		Player player = c.player;
		if (player == null) return;
		//是否作弊
		if (Math.Abs(player.x - msg.x) > 11 ||
			Math.Abs(player.z - msg.z) > 11)
		{
			Console.WriteLine("疑似作弊 " + player.id);
		}
		//更新信息
		player.x = msg.x;
		player.y = msg.y;
		player.z = msg.z;
		player.ex = msg.ex;
		player.ey = msg.ey;
		player.ez = msg.ez;
		//广播
		msg.id = player.id;
		Broadcast(msg);
	}

	//开火协议
	public static void MsgFire(ClientState c, MsgBase msgBase)
	{
		MsgFire msg = (MsgFire)msgBase;
		Player player = c.player;
		if (player == null) return;
		//广播
		msg.id = player.id;
		Broadcast(msg);
	}

	//击中协议
	public static void MsgHit(ClientState c, MsgBase msgBase)
	{
		MsgHit msg = (MsgHit)msgBase;
		Player player = c.player;
		if (player == null) return;
		//targetPlayer
		Player targetPlayer = PlayerManager.GetPlayer(msg.targetId);
		if (targetPlayer == null)
		{
			return;
		}
		//发送者校验
		if (player.id != msg.id)
		{
			return;
		}
		//状态
		//广播
		msg.id = player.id;
		Broadcast(msg);
	}
	public static void MsgLogin(ClientState c, MsgBase msgBase)
	{
		i++;

		MsgLogin msg = (MsgLogin)msgBase;
		msg.id = i;
		Player player = new Player(c);
		player.id = msg.id;

		PlayerManager.AddPlayer(msg.id, player);
		c.player = player;
		player.Send(msg);
		MsgAddPlayer msgAdd = new MsgAddPlayer();
		msgAdd.id = i;
		msgAdd.x = (float)(new Random().NextDouble() - 0.5) * 200;
		msgAdd.y = 35f;
		msgAdd.z = (float)(new Random().NextDouble() - 0.5) * 200;
		Broadcast(msgAdd);
		foreach(Player p in PlayerManager.players.Values)
        {
			if(p.id == msg.id)
            {
				break;
            }
			msgAdd.id = p.id;
			msgAdd.x = p.x;
			msgAdd.y = p.y;
			msgAdd.z = p.z;
			player.Send(msgAdd);
        }
		foreach (int id in PlayerManager.players.Keys)
		{
			Player player1 = PlayerManager.GetPlayer(id);
			Console.WriteLine("已经对" + player1.id + "广播");
		}
	}
	//广播消息
	public static void Broadcast(MsgBase msg)
	{
		foreach (int id in PlayerManager.players.Keys)
		{
			Player player = PlayerManager.GetPlayer(id);
			player.Send(msg);
		}
	}
}