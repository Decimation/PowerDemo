using System.Runtime.Intrinsics.Arm;
using Concentus.Structs;
using PowerDemo.Lib;
using System.Threading.Channels;
using System.Xml.Linq;

namespace PowerDemo;

public static class Program
{

	public static void Main(string[] args)
	{
		const string dem1 =
			@"I:\Steam\steamapps\common\Team Fortress 2\tf\autorecorded\autorecording_2023-11-01_02-07-08.dem";

		const string dem2 =
			@"I:\Steam\steamapps\common\Team Fortress 2\tf\autorecorded\autorecording_2024-04-11_19-19-22.dem";

		// 54907680 [345D320h]

		var ofs = 0x345D320;
		var fs  = File.OpenRead(dem2);
		fs.Seek(ofs, SeekOrigin.Begin);

		var br = new BinaryReader(fs);

		var op = new opus_plc(br)
			{ };
		Console.WriteLine(op);

		OpusDecoder decoder = OpusDecoder.Create(op.sampleRate, 1);
		var         opus    = new short[op.payloadLength * 4];
		
		int frameSize = decoder.Decode(op.payload, 0, (int) op.payloadLength, opus,
		                               0, opus.Length, false);

		Console.WriteLine(frameSize);
	}

	public struct opus_plc
	{

		public UInt64 steamID;
		public byte   payloadType;
		public UInt16 sampleRate;
		public byte   payloadType2;
		public UInt16 payloadLength;
		public byte[] payload;
		public UInt32 crc;

		public opus_plc() { }

		public opus_plc(BinaryReader br)
		{
			steamID       = br.ReadUInt64();
			payloadType   = br.ReadByte();
			sampleRate    = br.ReadUInt16();
			payloadType2  = br.ReadByte();
			payloadLength = br.ReadUInt16();
			payload       = br.ReadBytes(payloadLength);
			crc           = br.ReadUInt32();
		}

		public override string ToString()
		{
			return
				$"{nameof(steamID)}: {steamID}, {nameof(payloadType)}: {payloadType}, {nameof(sampleRate)}: {sampleRate}, "
				+ $"{nameof(payloadType2)}: {payloadType2}, {nameof(payloadLength)}: {payloadLength}, "
				+ $"{nameof(payload)}: {payload}, {nameof(crc)}: {crc}";
		}

	}

}