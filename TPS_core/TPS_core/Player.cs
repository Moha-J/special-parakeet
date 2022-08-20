using System;

public class Player
{
	//id
	public int id = 0;
	//指向ClientState
	public ClientState state;
	//构造函数
	public Player(ClientState state)
	{
		this.state = state;
	}
	//坐标和旋转
	public float x;
	public float y;
	public float z;
	public float ex;
	public float ey;
	public float ez;

	//坦克生命值
	public int hp = 100;

	//发送信息
	public void Send(MsgBase msgBase)
	{
		NetManager.Send(state, msgBase);
	}

}


