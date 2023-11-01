// Read S PowerDemo.Lib DataTables.cs
// 2023-11-01 @ 12:46 PM

namespace PowerDemo.Lib;

public class DataTables
{
	enum SendPropType : uint
	{
		Int = 0,
		Float,
		Vector,
		VectorXY,
		String,
		Array,
		DataTable,
		Int64
	}

	[Flags]
	enum SendPropFlags : uint
	{
		UNSIGNED              = 1,
		COORD                 = 2,
		NOSCALE               = 4,
		ROUNDDOWN             = 8,
		ROUNDUP               = 16,
		NORMAL                = 32,
		EXCLUDE               = 64,
		XYZE                  = 128,
		INSIDEARRAY           = 256,
		PROXY_ALWAYS_YES      = 512,
		CHANGES_OFTEN         = 1024,
		IS_A_VECTOR_ELEM      = 2048,
		COLLAPSIBLE           = 4096,
		COORD_MP              = 8192,
		COORD_MP_LOWPRECISION = 16384,
		COORD_MP_INTEGRAL     = 32768
	}

	static void ParseTables(BitBuffer bb, IList<string> node)
	{
		while (bb.ReadBool())
		{
			bool needsdecoder = bb.ReadBool();
			var  x            = bb.ReadString();
			node.Add(x);
			if (needsdecoder) x += "*";

			var numprops = bb.ReadBits(10);

			for (int i = 0; i < numprops; i++)
			{
				var type = (SendPropType)bb.ReadBits(5);
				// var propnode = dtnode.Add("DPT_" + type + " " + bb.ReadString());
				var propnode = bb.ReadString();
				node.Add($"DPT_{type} {propnode}");
				var flags = (SendPropFlags)bb.ReadBits(16);

				if (type == SendPropType.DataTable || (flags & SendPropFlags.EXCLUDE) != 0)
					propnode += " : " + bb.ReadString();
				else
				{
					if (type == SendPropType.Array)
						propnode += "[" + bb.ReadBits(10) + "]";
					else
					{
						bb.Seek(64);
						propnode += " (" + bb.ReadBits(7) + " bits)";
					}
				}
				node.Add(propnode);
			}
		}
	}

	static void ParseClassInfo(BitBuffer bb, List<string> node)
	{
		var classes = bb.ReadBits(16);

		for (int i = 0; i < classes; i++)
			node.Add("[" + bb.ReadBits(16) + "] " + bb.ReadString() + " (" + bb.ReadString() + ")");
	}

	public static void Parse(byte[] data, List<string> node)
	{
		var bb = new BitBuffer(data);
		node.Add("Send tables");
		ParseTables(bb, node);
		node.Add("Class info");
		ParseClassInfo(bb,node);
	}
}