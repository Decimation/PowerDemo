using static PowerDemo.Lib.SourceConstants;
using uchar = System.SByte;

namespace PowerDemo.Lib;

public unsafe struct DemoHeader
{
	/// <remarks>Length: <see cref="SourceConstants.MAGIC_LEN"/></remarks>
	private string m_magic;

	private uint m_demoProtocol;
	private uint m_networkProtocol;

	/// <remarks>Length: <see cref="SourceConstants.MAX_OSPATH"/></remarks>
	private string m_serverName;

	/// <remarks>Length: <see cref="SourceConstants.MAX_OSPATH"/></remarks>
	private string m_clientName;

	/// <remarks>Length: <see cref="SourceConstants.MAX_OSPATH"/></remarks>
	private string m_mapName;

	/// <remarks>Length: <see cref="SourceConstants.MAX_OSPATH"/></remarks>
	private string m_gameDir;

	private float m_playbackTIme;
	private uint  m_playbackTicks;
	private uint  m_playbackFrames;
	private uint  m_signonLen;
}