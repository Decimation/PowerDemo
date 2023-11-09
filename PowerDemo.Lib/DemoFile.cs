using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Novus.Streams;

namespace PowerDemo.Lib;

public class DemoFile
{

	Stream                       fstream;
	public DemoInfo              Info;
	public List<BaseDemoCommand> Messages;

	public DemoFile(Stream s)
	{
		fstream  = s;
		Messages = new List<BaseDemoCommand>();
		Parse();
	}

	public void Parse()
	{
		var reader = new BinaryReader(fstream);
		var id     = reader.ReadBytes(8);

		if (Encoding.ASCII.GetString(id) != "HL2DEMO\0")
			throw new Exception("Unsupported file format.");

		Info.DemoProtocol = reader.ReadInt32();

		if (Info.DemoProtocol >> 8 > 0)
			throw new Exception("Demos recorded on L4D branch games are currently unsupported.");

		Info.NetProtocol = reader.ReadInt32();

		Info.ServerName    = new string(reader.ReadChars(260)).Replace("\0", "");
		Info.ClientName    = new string(reader.ReadChars(260)).Replace("\0", "");
		Info.MapName       = new string(reader.ReadChars(260)).Replace("\0", "");
		Info.GameDirectory = new string(reader.ReadChars(260)).Replace("\0", "");

		Info.Seconds    = reader.ReadSingle();
		Info.TickCount  = reader.ReadInt32();
		Info.FrameCount = reader.ReadInt32();

		Info.SignonLength = reader.ReadInt32();

		/*while (true) {
			var msg = new DemoMessage { Type = (MessageType) reader.ReadByte() };

			if (msg.Type == MessageType.Stop)
				break;
			msg.Tick = reader.ReadInt32();

			switch (msg.Type) {
				case MessageType.Signon:
				case MessageType.Packet:
				case MessageType.ConsoleCmd:
				case MessageType.UserCmd:
				case MessageType.DataTables:
				case MessageType.StringTables:
					if (msg.Type == MessageType.Packet || msg.Type == MessageType.Signon)
						reader.BaseStream.Seek(0x54, SeekOrigin.Current); // command/sequence info
					else if (msg.Type == MessageType.UserCmd)
						reader.BaseStream.Seek(0x4, SeekOrigin.Current); // unknown
					msg.Data = reader.ReadBytes(reader.ReadInt32());
					break;
				case MessageType.SyncTick:
					msg.Data = new byte[0]; // lol wut
					break;
				default:
					throw new Exception("Unknown demo message type encountered.");
			}

			Messages.Add(msg);
		}*/

		while (true) {
			var type = (DemoCommandType) reader.ReadByte();

			BaseDemoCommand msg;

			switch (type) {
				default:
				case DemoCommandType.Stop:
					break;
				case DemoCommandType.ConsoleCmd:
					msg = new DemoConsoleCommand();
					msg.Read(reader);
					break;

			}

			/*msg.Tick = reader.ReadInt32();

			switch (msg.Type)
			{
				case MessageType.Signon:
				case MessageType.Packet:
				case MessageType.ConsoleCmd:
				case MessageType.UserCmd:
				case MessageType.DataTables:
				case MessageType.StringTables:
					if (msg.Type == MessageType.Packet || msg.Type == MessageType.Signon)
						reader.BaseStream.Seek(0x54, SeekOrigin.Current); // command/sequence info
					else if (msg.Type == MessageType.UserCmd)
						reader.BaseStream.Seek(0x4, SeekOrigin.Current); // unknown
					msg.Data = reader.ReadBytes(reader.ReadInt32());
					break;
				case MessageType.SyncTick:
					msg.Data = new byte[0]; // lol wut
					break;
				default:
					throw new Exception("Unknown demo message type encountered.");
			}

			Messages.Add(msg);*/
		}
	}

}

public struct DemoInfo
{

	public int    DemoProtocol, NetProtocol, TickCount, FrameCount, SignonLength;
	public string ServerName,   ClientName,  MapName,   GameDirectory;
	public float  Seconds;

}

public class DemoConsoleCommand : TimestampedDemoCommand
{

	public string Command { get; private set; }

	public override bool Read(BinaryReader r)
	{
		var b = base.Read(r);
		var i = r.ReadInt32();
		Command = r.ReadCString(i);
		return b;
	}

}

interface IStreamElement
{

	public bool Read(BinaryReader r);

}

/// <summary>
/// https://github.com/PazerOP/DemoLib2/blob/master/DemoLib2/demos/DemoViewpoint.cpp
/// https://github.com/PazerOP/DemoLib2/blob/master/DemoLib2/demos/DemoViewpoint.hpp
/// https://github.com/PazerOP/DemoLib2/blob/master/DemoLib2/demos/DemoViewpointFlags.hpp
/// </summary>
public class DemoViewpoint : IStreamElement
{

	private DemoViewpointFlags m_flags;
	private Vector3            m_viewOrigin1;
	private Vector3            m_viewAngles1;
	private Vector3            m_localViewAngles1;

	private Vector3 m_viewOrigin2;
	private Vector3 m_viewAngles2;
	private Vector3 m_localViewAngles2;

	public bool Read(BinaryReader r)
	{
		m_flags            = (DemoViewpointFlags) r.ReadInt32();
		m_viewOrigin1      = r.ReadVec();
		m_viewAngles1      = r.ReadVec();
		m_localViewAngles1 = r.ReadVec();
		m_viewOrigin2      = r.ReadVec();
		m_viewAngles2      = r.ReadVec();
		m_localViewAngles2 = r.ReadVec();

		return true;
	}

}

static class BinaryReaderExtensions
{

	public static Vector3 ReadVec(this BinaryReader r)
	{
		return new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
	}

	/*public static TEnum ReadEnum<TEnum>(this BinaryReader r) where TEnum : Enum
	{
		var under = Enum.GetUnderlyingType(typeof(TEnum));

		if (under == typeof(byte)) {
			return (TEnum) r.ReadByte();
		}
	}*/

}

public enum DemoViewpointFlags : int
{

	None       = 0,
	UseOrigin2 = 1 << 0,
	UseAngles2 = 1 << 1,
	NoInterp   = 1 << 2

}

public class DemoPacketCommand : TimestampedDemoCommand
{

	public override bool Read(BinaryReader r)
	{
		return base.Read(r);
	}

}

public class TimestampedDemoCommand : BaseDemoCommand
{

	public override bool Read(BinaryReader r)
	{
		Tick = r.ReadInt32();
		return base.Read(r);
	}

}

public class DemoSignonCommand : DemoPacketCommand
{

	public override bool Read(BinaryReader r)
	{
		return base.Read(r);
	}

}

public class BaseDemoCommand : IStreamElement
{

	public DemoCommandType Type;
	public int         Tick;
	public byte[]      Data;

	public virtual bool Read(BinaryReader r)
	{
		return true;
	}

}

public enum DemoCommandType : byte
{

	Invalid = 0,
	Signon  = 1,
	Packet,
	SyncTick,
	ConsoleCmd,
	UserCmd,
	DataTables,
	Stop,

	// CustomData, // L4D2
	StringTables,
	LastCmd = StringTables

}