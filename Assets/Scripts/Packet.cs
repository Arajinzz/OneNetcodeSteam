using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Packet
{

    public enum PacketType : ushort
    {
        InstantiatePlayer,
        InputMessage,
        StateMessage,
        PlayerLeaving,
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

    public void InsertBool(bool data)
    {
        buffer.AddRange(BitConverter.GetBytes(data));
    }

    public bool PopBool()
    {
        bool data = BitConverter.ToBoolean(buffer.GetRange(offset, sizeof(bool)).ToArray());
        offset += sizeof(bool);
        return data;
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

    public void InsertUInt64(UInt64 data)
    {
        buffer.AddRange(BitConverter.GetBytes(data));
    }

    public UInt64 PopUInt64()
    {
        UInt64 data = BitConverter.ToUInt64(buffer.GetRange(offset, sizeof(UInt64)).ToArray());
        offset += sizeof(UInt64);
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

    public void InsertInput(Structs.Inputs inputs)
    {

        InsertBool(inputs.up);
        InsertBool(inputs.down);
        InsertBool(inputs.left);
        InsertBool(inputs.right);
        InsertBool(inputs.jump);
        InsertFloat(inputs.axisX);

    }

    public Structs.Inputs PopInput()
    {
        Structs.Inputs inputs;
        inputs.up = PopBool();
        inputs.down = PopBool();
        inputs.left = PopBool();
        inputs.right = PopBool();
        inputs.jump = PopBool();
        inputs.axisX = PopFloat();

        return inputs;

    }

    public void InsertInputMessage(Structs.InputMessage inputMsg)
    {

        InsertUInt32(inputMsg.tick_number);
        InsertInput(inputMsg.inputs);

    }

    public Structs.InputMessage PopInputMessage()
    {
        Structs.InputMessage inputMsg;
        inputMsg.tick_number = PopUInt32();
        inputMsg.inputs = PopInput();

        return inputMsg;
    }

    public void InsertStateMessage(Structs.StateMessage stateMsg)
    {

        InsertUInt32(stateMsg.tick_number);

        InsertFloat(stateMsg.position.x);
        InsertFloat(stateMsg.position.y);
        InsertFloat(stateMsg.position.z);

        InsertFloat(stateMsg.rotation.x);
        InsertFloat(stateMsg.rotation.y);
        InsertFloat(stateMsg.rotation.z);
        InsertFloat(stateMsg.rotation.w);

    }

    public Structs.StateMessage PopStateMessage()
    {
        Structs.StateMessage stateMsg;
        stateMsg.tick_number = PopUInt32();
        stateMsg.position = new Vector3(PopFloat(), PopFloat(), PopFloat());
        stateMsg.rotation = new Quaternion(PopFloat(), PopFloat(), PopFloat(), PopFloat());

        return stateMsg;
    }

}