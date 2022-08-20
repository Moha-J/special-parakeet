//同步坦克信息
public class MsgSyncCharacter : MsgBase
{
	public MsgSyncCharacter() { protoName = "MsgSyncCharacter"; }
	//位置、旋转
	public float x = 0f;
	public float y = 0f;
	public float z = 0f;
	public float ex = 0f;
	public float ey = 0f;
	public float ez = 0f;
	public float gunX = 0f;
	//服务端补充
	public int id = 0;      //哪个坦克
}

//开火
public class MsgFire : MsgBase
{
	public MsgFire() { protoName = "MsgFire"; }
	//子弹初始位置、旋转
	public float x = 0f;
	public float y = 0f;
	public float z = 0f;
	public float ex = 0f;
	public float ey = 0f;
	public float ez = 0f;
	//服务端补充
	public int id = 0;      //哪个坦克
}

//击中
public class MsgHit : MsgBase
{
	public MsgHit() { protoName = "MsgHit"; }
	//击中谁
	public int targetId = 0;

	//服务端补充
	public int id = 0;      //哪个坦克
	public int hp = 0;          //被击中坦克血量
	public int damage = 0;      //受到的伤害
}
public class MsgLogin : MsgBase
{
	public MsgLogin() { protoName = "MsgLogin"; }
	public int id = 0;
}
public class MsgAddPlayer : MsgBase
{
	public MsgAddPlayer() { protoName = "MsgAddPlayer"; }
	public int id = 0;
	public float x = 0f;
	public float y = 0f;
	public float z = 0f;
}
public class MsgLeaveBattle : MsgBase
{
	public MsgLeaveBattle() { protoName = "MsgLeaveBattle"; }
	//服务端回
	public int id = 0;  //玩家id
}