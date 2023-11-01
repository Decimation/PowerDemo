// Read S PowerDemo.Lib UserCmd.cs
// 2023-11-01 @ 12:45 PM

namespace PowerDemo.Lib;

public class UserCmd
{

	/*
			static string ParseButtons(uint buttons)
			{
				string res = "(none)";
				// TODO: IMPLEMENT
				return res;
			}
	*/

	public static void ParseIntoTreeNode(byte[] data, List<string> node)
	{
		var bb = new BitBuffer(data);
		if (bb.ReadBool()) node.Add("Command number: " + bb.ReadBits(32));
		if (bb.ReadBool()) node.Add("Tick count: " + bb.ReadBits(32));
		if (bb.ReadBool()) node.Add("Viewangle pitch: " + bb.ReadFloat());
		if (bb.ReadBool()) node.Add("Viewangle yaw: " + bb.ReadFloat());
		if (bb.ReadBool()) node.Add("Viewangle roll: " + bb.ReadFloat());
		if (bb.ReadBool()) node.Add("Foward move: " + bb.ReadFloat());
		if (bb.ReadBool()) node.Add("Side move: " + bb.ReadFloat());
		if (bb.ReadBool()) node.Add("Up move: " + bb.ReadFloat().ToString());
		if (bb.ReadBool()) node.Add("Buttons: 0x" + bb.ReadBits(32).ToString("X8"));
		if (bb.ReadBool()) node.Add("Impulse: " + bb.ReadBits(8));
		// TODO: weaponselect/weaponsubtype, mousedx/dy
	}

}