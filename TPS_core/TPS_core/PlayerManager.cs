using System;
using System.Collections.Generic;

public class PlayerManager
{
	//玩家列表
	public static Dictionary<int, Player> players = new Dictionary<int, Player>();
	//玩家是否在线
	public static bool IsOnline(int id)
	{
		return players.ContainsKey(id);
	}
	//获取玩家
	public static Player GetPlayer(int id)
	{
		if (players.ContainsKey(id))
		{
			return players[id];
		}
		return null;
	}
	//添加玩家
	public static void AddPlayer(int id, Player player)
	{
		players.Add(id, player);
	}
	//删除玩家
	public static void RemovePlayer(int id)
	{
		players.Remove(id);
	}
}


