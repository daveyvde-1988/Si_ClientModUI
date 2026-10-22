using System;
using System.IO;
using UnityEngine;
using Utilities;

public class GameByteStreamReader
{
	private MemoryStream m_MemoryStream;

	private BinaryReader m_BinaryReader;

	private bool m_ReadDebug;

	private uint ForNetID;

	private string ForFunction = string.Empty;

	protected static GameByteStreamReader s_Instance = new GameByteStreamReader();

	public static GameByteStreamReader GetGameByteStreamReader(byte[] data, int dataSizeBytes, uint netID, string funcName, bool readDebug = true)
	{
		s_Instance.m_ReadDebug = readDebug;
		s_Instance.Reset(data, dataSizeBytes);
		s_Instance.ForNetID = netID;
		s_Instance.ForFunction = funcName;
		return s_Instance;
	}

	public static GameByteStreamReader ReadFromFile(Stream stream)
	{
		if (!stream.CanRead)
		{
			Debug.LogError($"Cannot read from stream {stream}");
			return null;
		}
		GameByteStreamReader gameByteStreamReader = new GameByteStreamReader();
		stream.CopyTo(gameByteStreamReader.m_MemoryStream);
		gameByteStreamReader.m_MemoryStream.Position = 0L;
		return gameByteStreamReader;
	}

	public static GameByteStreamReader FromRawByteArray(byte[] data)
	{
		GameByteStreamReader gameByteStreamReader = new GameByteStreamReader();
		gameByteStreamReader.Reset(data, data.Length);
		return gameByteStreamReader;
	}

	public static byte[] ToRawByteArray(GameByteStreamReader dataReader)
	{
		long position = dataReader.m_MemoryStream.Position;
		dataReader.m_MemoryStream.Position = 0L;
		long length = dataReader.m_MemoryStream.Length;
		if (length > int.MaxValue)
		{
			Debug.LogError("Data is too large to convert to byte array");
			return null;
		}
		byte[] array = new byte[length];
		int num = dataReader.m_MemoryStream.Read(array, 0, (int)length);
		if (num != length)
		{
			Debug.LogError($"Error while reading byte data, expected {length} bytes, read {num}");
			return null;
		}
		dataReader.m_MemoryStream.Position = position;
		return array;
	}

	public long GetDataSize()
	{
		return m_MemoryStream.Length;
	}

	public GameByteStreamReader()
	{
		m_MemoryStream = new MemoryStream();
		m_BinaryReader = new BinaryReader(m_MemoryStream);
	}

	public void WriteTo(GameByteStreamWriter writer)
	{
		long position = m_MemoryStream.Position;
		m_MemoryStream.Position = 0L;
		MemoryStream memoryStream = writer.GetMemoryStream();
		memoryStream.Position = 0L;
		m_MemoryStream.CopyTo(memoryStream);
		m_MemoryStream.Position = position;
	}

	~GameByteStreamReader()
	{
		m_MemoryStream.Dispose();
		m_BinaryReader.Dispose();
	}

	public void SetNetID(uint netID)
	{
		ForNetID = netID;
	}

	public void SetFunction(string funcName)
	{
		ForFunction = funcName;
	}

	public string GetLogName()
	{
		if (ForNetID == 0)
		{
			return "<" + ForFunction + "> - [0] NULL";
		}
		NetworkComponent netObject = NetworkComponent.GetNetObject(ForNetID);
		if (!netObject)
		{
			return "<" + ForFunction + "> - [" + ForNetID + "] NULL";
		}
		BaseGameObject baseGameObject = netObject.transform.GetBaseGameObject();
		if ((bool)baseGameObject)
		{
			if ((bool)baseGameObject.ObjectInfo)
			{
				return "<" + ForFunction + "> - [" + ForNetID + "] (" + baseGameObject.ObjectInfo.DisplayName + ") '" + baseGameObject.gameObject.name + "'";
			}
			return "<" + ForFunction + "> - [" + ForNetID + "] (NULL INFO) '" + baseGameObject.gameObject.name + "'";
		}
		return "<" + ForFunction + "> - [" + ForNetID + "] (NOT BASE GAME OBJECT) '" + netObject.gameObject.name + "'";
	}

	public void Reset(byte[] data, int dataSizeBytes)
	{
		m_MemoryStream.Position = 0L;
		m_MemoryStream.Write(data, 0, dataSizeBytes);
		m_MemoryStream.Position = 0L;
	}

	public void ResetToStart()
	{
		m_MemoryStream.Position = 0L;
	}

	private float ReadFloatNonAlloc()
	{
		return GameByteStreamConvert.UInt32ToFloat(m_BinaryReader.ReadUInt32());
	}

	private double ReadDoubleNonAlloc()
	{
		return GameByteStreamConvert.UInt64ToDouble(m_BinaryReader.ReadUInt64());
	}

	public void ReadType(EGameByteStreamType expectedDataType)
	{
		if (m_ReadDebug)
		{
			EGameByteStreamType eGameByteStreamType = (EGameByteStreamType)m_BinaryReader.ReadByte();
			if (eGameByteStreamType != expectedDataType)
			{
				Debug.LogError(GetLogName() + " - Wrong data type read - " + eGameByteStreamType.ToString() + ", should be " + expectedDataType);
			}
		}
	}

	public int ReadByteArray(byte[] byteData)
	{
		ReadType(EGameByteStreamType.ByteArray);
		ushort num = m_BinaryReader.ReadUInt16();
		if (num == 0)
		{
			return 0;
		}
		return m_BinaryReader.Read(byteData, 0, num);
	}

	public int ReadLargeByteArray(byte[] byteData)
	{
		ReadType(EGameByteStreamType.LargeByteArray);
		uint num = m_BinaryReader.ReadUInt32();
		if (num == 0)
		{
			return 0;
		}
		return m_BinaryReader.Read(byteData, 0, (int)num);
	}

	public bool[] ReadLargeBoolArray()
	{
		ReadType(EGameByteStreamType.LargeBoolArray);
		uint num = m_BinaryReader.ReadUInt32();
		if (num == 0)
		{
			return Array.Empty<bool>();
		}
		bool[] array = new bool[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = m_BinaryReader.ReadBoolean();
		}
		return array;
	}

	public bool ReadBool()
	{
		ReadType(EGameByteStreamType.Bool);
		return m_BinaryReader.ReadBoolean();
	}

	public byte ReadByte()
	{
		ReadType(EGameByteStreamType.Byte);
		return m_BinaryReader.ReadByte();
	}

	public sbyte ReadSByte()
	{
		ReadType(EGameByteStreamType.SByte);
		return m_BinaryReader.ReadSByte();
	}

	public short ReadInt16()
	{
		ReadType(EGameByteStreamType.Int16);
		return m_BinaryReader.ReadInt16();
	}

	public ushort ReadUInt16()
	{
		ReadType(EGameByteStreamType.UInt16);
		return m_BinaryReader.ReadUInt16();
	}

	public int ReadInt32()
	{
		ReadType(EGameByteStreamType.Int32);
		return m_BinaryReader.ReadInt32();
	}

	public uint ReadUInt32()
	{
		ReadType(EGameByteStreamType.UInt32);
		return m_BinaryReader.ReadUInt32();
	}

	public uint ReadPackedUInt32(bool isBGO = false)
	{
		if (!isBGO)
		{
			ReadType(EGameByteStreamType.PackedUInt32);
		}
		byte b = m_BinaryReader.ReadByte();
		if (b < 241)
		{
			return b;
		}
		byte b2 = m_BinaryReader.ReadByte();
		if (b >= 241 && b <= 248)
		{
			return (uint)(240 + 256 * (b - 241) + b2);
		}
		byte b3 = m_BinaryReader.ReadByte();
		if (b == 249)
		{
			return (uint)(2288 + 256 * b2 + b3);
		}
		byte b4 = m_BinaryReader.ReadByte();
		if (b == 250)
		{
			return (uint)(b2 + (b3 << 8) + (b4 << 16));
		}
		byte b5 = m_BinaryReader.ReadByte();
		if (b >= 251)
		{
			return (uint)(b2 + (b3 << 8) + (b4 << 16) + (b5 << 24));
		}
		throw new IndexOutOfRangeException("ReadPackedUInt32() failure: " + b);
	}

	public long ReadInt64()
	{
		ReadType(EGameByteStreamType.Int64);
		return m_BinaryReader.ReadInt64();
	}

	public ulong ReadUInt64()
	{
		ReadType(EGameByteStreamType.UInt64);
		return m_BinaryReader.ReadUInt64();
	}

	public int ReadUInt64Array(ulong[] ids)
	{
		ReadType(EGameByteStreamType.UInt64Array);
		int num = m_BinaryReader.ReadUInt16();
		if (num == 0)
		{
			return 0;
		}
		for (int i = 0; i < num; i++)
		{
			ids[i] = m_BinaryReader.ReadUInt64();
		}
		return num;
	}

	public ulong ReadPackedUInt64()
	{
		ReadType(EGameByteStreamType.PackedUInt64);
		byte b = m_BinaryReader.ReadByte();
		if (b < 241)
		{
			return b;
		}
		byte b2 = m_BinaryReader.ReadByte();
		if (b >= 241 && b <= 248)
		{
			return (ulong)(240 + 256 * ((long)b - 241L) + b2);
		}
		byte b3 = m_BinaryReader.ReadByte();
		if (b == 249)
		{
			return (ulong)(2288 + 256L * (long)b2 + b3);
		}
		byte b4 = m_BinaryReader.ReadByte();
		if (b == 250)
		{
			return b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16);
		}
		byte b5 = m_BinaryReader.ReadByte();
		if (b == 251)
		{
			return b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16) + ((ulong)b5 << 24);
		}
		byte b6 = m_BinaryReader.ReadByte();
		if (b == 252)
		{
			return b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16) + ((ulong)b5 << 24) + ((ulong)b6 << 32);
		}
		byte b7 = m_BinaryReader.ReadByte();
		if (b == 253)
		{
			return b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16) + ((ulong)b5 << 24) + ((ulong)b6 << 32) + ((ulong)b7 << 40);
		}
		byte b8 = m_BinaryReader.ReadByte();
		if (b == 254)
		{
			return b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16) + ((ulong)b5 << 24) + ((ulong)b6 << 32) + ((ulong)b7 << 40) + ((ulong)b8 << 48);
		}
		byte b9 = m_BinaryReader.ReadByte();
		if (b == byte.MaxValue)
		{
			return b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16) + ((ulong)b5 << 24) + ((ulong)b6 << 32) + ((ulong)b7 << 40) + ((ulong)b8 << 48) + ((ulong)b9 << 56);
		}
		throw new IndexOutOfRangeException("ReadPackedUInt64() failure: " + b);
	}

	public float ReadInputAxis()
	{
		ReadType(EGameByteStreamType.InputAxis);
		return (float)(m_BinaryReader.ReadByte() - 100) * 0.01f;
	}

	public float ReadFloatAsByte(float min, float max)
	{
		ReadType(EGameByteStreamType.FloatAsByte);
		byte b = m_BinaryReader.ReadByte();
		float num = max - min;
		if (num == 0f)
		{
			return min;
		}
		return (float)(int)b * (1f / 255f) * num + min;
	}

	public float ReadFloatAsUShort(float min, float max)
	{
		ReadType(EGameByteStreamType.FloatAsUShort);
		ushort num = m_BinaryReader.ReadUInt16();
		float num2 = max - min;
		if (num2 == 0f)
		{
			return min;
		}
		return (float)(int)num * (1f / 65535f) * num2 + min;
	}

	public float ReadFloat()
	{
		ReadType(EGameByteStreamType.Float);
		return ReadFloatNonAlloc();
	}

	public double ReadDouble()
	{
		ReadType(EGameByteStreamType.Double);
		return ReadDoubleNonAlloc();
	}

	public decimal ReadDecimal()
	{
		ReadType(EGameByteStreamType.Decimal);
		return m_BinaryReader.ReadDecimal();
	}

	public char ReadChar()
	{
		ReadType(EGameByteStreamType.Char);
		return m_BinaryReader.ReadChar();
	}

	public string ReadString()
	{
		ReadType(EGameByteStreamType.String);
		return m_BinaryReader.ReadString();
	}

	public Quaternion ReadQuaternion()
	{
		ReadType(EGameByteStreamType.Quaternion);
		return new Quaternion(ReadFloatNonAlloc(), ReadFloatNonAlloc(), ReadFloatNonAlloc(), ReadFloatNonAlloc());
	}

	public Quaternion ReadPackedQuaternion()
	{
		ReadType(EGameByteStreamType.PackedQuaternion);
		uint num = m_BinaryReader.ReadUInt32();
		int num2 = (int)(num >> 30);
		float num3 = (float)((num >> 20) & 0x3FF) / 1023f * 1.4142131f + -0.70710653f;
		float num4 = (float)((num >> 10) & 0x3FF) / 1023f * 1.4142131f + -0.70710653f;
		float num5 = (float)(num & 0x3FF) / 1023f * 1.4142131f + -0.70710653f;
		Quaternion identity = Quaternion.identity;
		float value = Mathf.Sqrt(1f - num3 * num3 - num4 * num4 - num5 * num5);
		identity[num2] = value;
		identity[(num2 + 1) % 4] = num3;
		identity[(num2 + 2) % 4] = num4;
		identity[(num2 + 3) % 4] = num5;
		return identity;
	}

	public Vector2 ReadVector2()
	{
		ReadType(EGameByteStreamType.Vector2);
		return new Vector2(ReadFloatNonAlloc(), ReadFloatNonAlloc());
	}

	public Vector3 ReadVector3()
	{
		ReadType(EGameByteStreamType.Vector3);
		return new Vector3(ReadFloatNonAlloc(), ReadFloatNonAlloc(), ReadFloatNonAlloc());
	}

	public Vector3 ReadPackedVector3(float maxLength)
	{
		ReadType(EGameByteStreamType.PackedVector3);
		ushort num = m_BinaryReader.ReadUInt16();
		if (num == 0)
		{
			return Vector3.zero;
		}
		float num2 = (float)(int)num * (1f / 65535f) * maxLength;
		Vector3 zero = Vector3.zero;
		zero.x = ((float)(int)m_BinaryReader.ReadByte() * (1f / 255f) - 0.5f) * 2f;
		zero.y = ((float)(int)m_BinaryReader.ReadByte() * (1f / 255f) - 0.5f) * 2f;
		zero.z = ((float)(int)m_BinaryReader.ReadByte() * (1f / 255f) - 0.5f) * 2f;
		return zero.normalized * num2;
	}

	public Vector3 ReadPackedPosition()
	{
		ReadType(EGameByteStreamType.PackedPosition);
		Vector3 zero = Vector3.zero;
		zero.x = m_BinaryReader.ReadInt16();
		zero.y = m_BinaryReader.ReadInt16();
		zero.z = m_BinaryReader.ReadInt16();
		return zero;
	}

	public Vector4 ReadVector4()
	{
		ReadType(EGameByteStreamType.Vector4);
		return new Vector4(ReadFloatNonAlloc(), ReadFloatNonAlloc(), ReadFloatNonAlloc(), ReadFloatNonAlloc());
	}

	public Color ReadColor()
	{
		ReadType(EGameByteStreamType.Color);
		return new Color(ReadFloatNonAlloc(), ReadFloatNonAlloc(), ReadFloatNonAlloc(), ReadFloatNonAlloc());
	}

	public Color32 ReadColor32()
	{
		ReadType(EGameByteStreamType.Color32);
		return new Color32(m_BinaryReader.ReadByte(), m_BinaryReader.ReadByte(), m_BinaryReader.ReadByte(), m_BinaryReader.ReadByte());
	}

	public Rect ReadRect()
	{
		ReadType(EGameByteStreamType.Rect);
		return new Rect(ReadFloatNonAlloc(), ReadFloatNonAlloc(), ReadFloatNonAlloc(), ReadFloatNonAlloc());
	}

	public Plane ReadPlane()
	{
		ReadType(EGameByteStreamType.Plane);
		return new Plane(new Vector3(ReadFloatNonAlloc(), ReadFloatNonAlloc(), ReadFloatNonAlloc()), ReadFloatNonAlloc());
	}

	public Ray ReadRay()
	{
		ReadType(EGameByteStreamType.Ray);
		return new Ray(new Vector3(ReadFloatNonAlloc(), ReadFloatNonAlloc(), ReadFloatNonAlloc()), new Vector3(ReadFloatNonAlloc(), ReadFloatNonAlloc(), ReadFloatNonAlloc()));
	}

	public Matrix4x4 ReadMatrix4x4()
	{
		ReadType(EGameByteStreamType.Matrix4x4);
		return new Matrix4x4
		{
			m00 = ReadFloatNonAlloc(),
			m01 = ReadFloatNonAlloc(),
			m02 = ReadFloatNonAlloc(),
			m03 = ReadFloatNonAlloc(),
			m10 = ReadFloatNonAlloc(),
			m11 = ReadFloatNonAlloc(),
			m12 = ReadFloatNonAlloc(),
			m13 = ReadFloatNonAlloc(),
			m20 = ReadFloatNonAlloc(),
			m21 = ReadFloatNonAlloc(),
			m22 = ReadFloatNonAlloc(),
			m23 = ReadFloatNonAlloc(),
			m30 = ReadFloatNonAlloc(),
			m31 = ReadFloatNonAlloc(),
			m32 = ReadFloatNonAlloc(),
			m33 = ReadFloatNonAlloc()
		};
	}

	public Type ReadType()
	{
		ReadType(EGameByteStreamType.Type);
		string text = m_BinaryReader.ReadString();
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		Type type = Type.GetType(text);
		if (type == null)
		{
			type = TypeIndexer.TryGetMigratedType(text);
		}
		if (type == null)
		{
			throw new InvalidOperationException("Type not found: " + text);
		}
		return type;
	}

	public Team ReadTeam()
	{
		ReadType(EGameByteStreamType.Team);
		return Team.GetTeamByIndex(m_BinaryReader.ReadByte() - 1);
	}

	public BaseGameObject ReadBaseGameObject()
	{
		ReadType(EGameByteStreamType.BaseGameObject);
		NetworkComponent netObject = NetworkComponent.GetNetObject(ReadPackedUInt32(isBGO: true));
		if ((bool)netObject)
		{
			return netObject.Owner;
		}
		return null;
	}

	public NetworkID ReadNetworkID()
	{
		return new NetworkID(ReadUInt64());
	}
}
