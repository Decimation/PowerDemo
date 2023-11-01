// Read S PowerDemo.Lib Packet.cs
// 2023-11-01 @ 12:45 PM

using System.Drawing;

namespace PowerDemo.Lib;

public class Packet
{

	delegate void MsgHandler(BitBuffer bb, List<string> node);

	static readonly Dictionary<uint, MsgHandler> Handlers = new Dictionary<uint, MsgHandler>
	{
		{
			0, (_, node) =>
			{
				node.Add("net_nop");
			}
		},
		{ 1, net_disconnect },
		{ 2, net_file },
		{ 3, net_tick },
		{ 4, net_stringcmd },
		{ 5, net_setconvar },
		{ 6, net_signonstate },

		{ 7, svc_print },
		{ 8, svc_serverinfo },
		{ 9, svc_sendtable },
		{ 10, svc_classinfo },
		{ 11, svc_setpause },
		{ 12, svc_createstringtable },
		{ 13, svc_updatestringtable },
		{ 14, svc_voiceinit },
		{ 15, svc_voicedata },
		{ 17, svc_sounds },
		{ 18, svc_setview },
		{ 19, svc_fixangle },
		{ 20, svc_crosshairangle },
		{ 21, svc_bspdecal },
		{ 23, svc_usermessage },
		{ 24, svc_entitymessage },
		{ 25, svc_gameevent },
		{ 26, svc_packetentities },
		{ 27, svc_tempentities },
		{ 28, svc_prefetch },
		{ 29, svc_menu },
		{ 30, svc_gameeventlist },
		{ 31, svc_getcvarvalue },
		{ 32, svc_cmdkeyvalues }
	};

	public static void Parse(byte[] data, List<string> node)
	{
		var bb = new BitBuffer(data);

		while (bb.BitsLeft() > 6) {
			var        type = bb.ReadBits(6);
			MsgHandler handler;

			if (Handlers.TryGetValue(type, out handler)) {
				// var sub = new List<string>();
				node.Add(handler.Method.Name);
				handler(bb, node);
			}
			else {
				node.Add("unknown message type " + type);
				break;
			}
		}
	}

	// do we even encounter these in demo files?
	static void net_disconnect(BitBuffer bb, List<string> node)
	{
		node.Add("Reason: " + bb.ReadString());
	}

	static void net_file(BitBuffer bb, List<string> node)
	{
		node.Add("Transfer ID: " + bb.ReadBits(32));
		node.Add("Filename: " + bb.ReadString());
		node.Add("Requested: " + bb.ReadBool());
	}

	static void net_tick(BitBuffer bb, List<string> node)
	{
		node.Add("Tick: " + (int) bb.ReadBits(32));
		node.Add("Host frametime: " + bb.ReadBits(16));
		node.Add("Host frametime StdDev: " + bb.ReadBits(16));
	}

	static void net_stringcmd(BitBuffer bb, List<string> node)
	{
		node.Add("Command: " + bb.ReadString());
	}

	static void net_setconvar(BitBuffer bb, List<string> node)
	{
		var n = bb.ReadBits(8);

		while (n-- > 0)
			node.Add(bb.ReadString() + ": " + bb.ReadString());
	}

	static void net_signonstate(BitBuffer bb, List<string> node)
	{
		node.Add("Signon state: " + bb.ReadBits(8));
		node.Add("Spawn count: " + (int) bb.ReadBits(32));
	}

	static void svc_print(BitBuffer bb, List<string> node)
	{
		node.Add(bb.ReadString());
	}

	static void svc_serverinfo(BitBuffer bb, List<string> node)
	{
		short version = (short) bb.ReadBits(16);
		node.Add("Version: " + version);
		node.Add("Server count: " + (int) bb.ReadBits(32));
		node.Add("SourceTV: " + bb.ReadBool());
		node.Add("Dedicated: " + bb.ReadBool());
		node.Add("Server client CRC: 0x" + bb.ReadBits(32).ToString("X8"));
		node.Add("Max classes: " + bb.ReadBits(16));

		if (version < 18)
			node.Add("Server map CRC: 0x" + bb.ReadBits(32).ToString("X8"));
		else
			bb.Seek(128); // TODO: display out map md5 hash
		node.Add("Current player count: " + bb.ReadBits(8));
		node.Add("Max player count: " + bb.ReadBits(8));
		node.Add("Interval per tick: " + bb.ReadFloat());
		node.Add("Platform: " + (char) bb.ReadBits(8));
		node.Add("Game directory: " + bb.ReadString());
		node.Add("Map name: " + bb.ReadString());
		node.Add("Skybox name: " + bb.ReadString());
		node.Add("Hostname: " + bb.ReadString());
		node.Add("Has replay: " + bb.ReadBool()); // ???: protocol version
	}

	static void svc_sendtable(BitBuffer bb, List<string> node)
	{
		node.Add("Needs decoder: " + bb.ReadBool());
		var n = bb.ReadBits(16);
		node.Add("Length in bits: " + n);
		bb.Seek(n);
	}

	static void svc_classinfo(BitBuffer bb, List<string> node)
	{
		var n = bb.ReadBits(16);
		node.Add("Number of server classes: " + n);
		var cc = bb.ReadBool();
		node.Add("Create classes on client: " + cc);

		if (!cc)
			while (n-- > 0) {
				node.Add("Class ID: " + bb.ReadBits((uint) Math.Log(n, 2) + 1));
				node.Add("Class name: " + bb.ReadString());
				node.Add("Datatable name: " + bb.ReadString());
			}
	}

	static void svc_setpause(BitBuffer bb, List<string> node)
	{
		node.Add(bb.ReadBool().ToString());
	}

	static void svc_createstringtable(BitBuffer bb, List<string> node)
	{
		node.Add("Table name: " + bb.ReadString());
		var m = bb.ReadBits(16);
		node.Add("Max entries: " + m);
		node.Add("Number of entries: " + bb.ReadBits((uint) Math.Log(m, 2) + 1));
		var n = bb.ReadBits(20);
		node.Add("Length in bits: " + n);
		var f = bb.ReadBool();
		node.Add("Userdata fixed size: " + f);

		if (f) {
			node.Add("Userdata size: " + bb.ReadBits(12));
			node.Add("Userdata bits: " + bb.ReadBits(4));
		}

		// ???: this is not in Source 2007 netmessages.h/cpp it seems. protocol version?
		node.Add("Compressed: " + bb.ReadBool());
		bb.Seek(n);
	}

	static void svc_updatestringtable(BitBuffer bb, List<string> node)
	{
		node.Add("Table ID: " + bb.ReadBits(5));
		node.Add("Changed entries: " + (bb.ReadBool() ? bb.ReadBits(16) : 1));
		var b = bb.ReadBits(20);
		node.Add("Length in bits: " + b);
		bb.Seek(b);
	}

	static void svc_voiceinit(BitBuffer bb, List<string> node)
	{
		node.Add("Codec: " + bb.ReadString());
		node.Add("Quality: " + bb.ReadBits(8));
	}

	static void svc_voicedata(BitBuffer bb, List<string> node)
	{
		node.Add("Client: " + bb.ReadBits(8));
		node.Add("Proximity: " + bb.ReadBits(8));
		var b = bb.ReadBits(16);
		node.Add("Length in bits: " + b);
		bb.Seek(b);
	}

	static void svc_sounds(BitBuffer bb, List<string> node)
	{
		var r = bb.ReadBool();
		node.Add("Reliable: " + r);
		node.Add("Number of sounds: " + (r ? 1 : bb.ReadBits(8)));
		uint b = r ? bb.ReadBits(8) : bb.ReadBits(16);
		node.Add("Length in bits: " + b);
		bb.Seek(b);
	}

	static void svc_setview(BitBuffer bb, List<string> node)
	{
		node.Add("Entity index: " + bb.ReadBits(11));
	}

	static void svc_fixangle(BitBuffer bb, List<string> node)
	{
		node.Add("Relative: " + bb.ReadBool());
		// TODO: handle properly
		bb.Seek(48);
	}

	static void svc_crosshairangle(BitBuffer bb, List<string> node)
	{
		// TODO: see above
		bb.Seek(48);
	}

	static void svc_bspdecal(BitBuffer bb, List<string> node)
	{
		node.Add("Position: " + bb.ReadVecCoord());
		node.Add("Decal texture index: " + bb.ReadBits(9));

		if (bb.ReadBool()) {
			node.Add("Entity index: " + bb.ReadBits(11));
			node.Add("Model index: " + bb.ReadBits(12));
		}

		node.Add("Low priority: " + bb.ReadBool());
	}

	static void svc_usermessage(BitBuffer bb, List<string> node)
	{
		node.Add("Message type: " + bb.ReadBits(8));
		var b = bb.ReadBits(11);
		node.Add("Length in bits: " + b);
		bb.Seek(b);
	}

	static void svc_entitymessage(BitBuffer bb, List<string> node)
	{
		node.Add("Entity index: " + bb.ReadBits(11));
		node.Add("Class ID: " + bb.ReadBits(9));
		var b = bb.ReadBits(11);
		node.Add("Length in bits: " + b);
		bb.Seek(b);
	}

	static void svc_gameevent(BitBuffer bb, List<string> node)
	{
		var b = bb.ReadBits(11);
		node.Add("Length in bits: " + b);
		bb.Seek(b);
	}

	static void svc_packetentities(BitBuffer bb, List<string> node)
	{
		node.Add("Max entries: " + bb.ReadBits(11));
		bool d = bb.ReadBool();
		node.Add("Is delta: " + d);

		if (d)
			node.Add("Delta from: " + bb.ReadBits(32));
		node.Add("Baseline: " + bb.ReadBool());
		node.Add("Updated entries: " + bb.ReadBits(11));
		var b = bb.ReadBits(20);
		node.Add("Length in bits: " + b);
		node.Add("Update baseline: " + bb.ReadBool());
		bb.Seek(b);
	}

	static void svc_tempentities(BitBuffer bb, List<string> node)
	{
		node.Add("Number of entries: " + bb.ReadBits(8));
		var b = bb.ReadBits(17);
		node.Add("Length in bits: " + b);
		bb.Seek(b);
	}

	static void svc_prefetch(BitBuffer bb, List<string> node)
	{
		node.Add("Sound index: " + bb.ReadBits(13));
	}

	static void svc_menu(BitBuffer bb, List<string> node)
	{
		node.Add("Menu type: " + bb.ReadBits(16));
		var b = bb.ReadBits(16);
		node.Add("Length in bytes: " + b);
		bb.Seek(b << 3);
	}

	static void svc_gameeventlist(BitBuffer bb, List<string> node)
	{
		node.Add("Number of events: " + bb.ReadBits(9));
		var b = bb.ReadBits(20);
		node.Add("Length in bits: " + b);
		bb.Seek(b);
	}

	static void svc_getcvarvalue(BitBuffer bb, List<string> node)
	{
		node.Add("Cookie: 0x" + bb.ReadBits(32).ToString("X8"));
		node.Add(bb.ReadString());
	}

	static void svc_cmdkeyvalues(BitBuffer bb, List<string> node)
	{
		var b = bb.ReadBits(32);
		node.Add("Length in bits: " + b);
		bb.Seek(b);
	}

}