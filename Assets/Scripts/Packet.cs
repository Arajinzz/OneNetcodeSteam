using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Packet
{

    public enum PacketType : ushort
    {
        SyncTick,
        IntantiatePlayer,
        InstantiatePlayerAtPosition,
        KeyEvent,
        PlayerLeft,
        PlayerRotated,
    }

    public UInt16 packetType;
    public List<byte> buffer;
    public int offset;

    public Packet(PacketType type)
    {
        packetType = Convert.ToUInt16(type);
        buffer = new List<byte>();
        buffer.AddRange(BitConverter.GetBytes(packetType));
    }

    public Packet(byte[] data)
    {
        buffer = new List<byte>(data);
        packetType = BitConverter.ToUInt16(buffer.GetRange(offset, sizeof(ushort)).ToArray());
        offset += sizeof(ushort);
    }

    public PacketType GetPacketType()
    {
        return (PacketType)packetType;
    }

    public void InsertInt(int data)
    {
        buffer.AddRange(BitConverter.GetBytes(data));
    }

    public int PopInt()
    {
        int data = BitConverter.ToInt32(buffer.GetRange(offset, sizeof(int)).ToArray());
        offset += sizeof(int);
        return data;
    }

    public void InsertUInt32(UInt32 data)
    {
        buffer.AddRange(BitConverter.GetBytes(data));
    }

    public UInt32 PopUInt32()
    {
        UInt32 data = BitConverter.ToUInt32(buffer.GetRange(offset, sizeof(UInt32)).ToArray());
        offset += sizeof(UInt32);
        return data;
    }

    public void InsertFloat(float data)
    {
        buffer.AddRange(BitConverter.GetBytes(data));
    }

    public float PopFloat()
    {
        float data = BitConverter.ToSingle(buffer.GetRange(offset, sizeof(float)).ToArray());
        offset += sizeof(float);
        return data;
    }

}