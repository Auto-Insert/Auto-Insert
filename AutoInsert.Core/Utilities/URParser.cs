using AutoInsert.Shared.Models;
using AutoInsert.Core.Services.Communication;

namespace AutoInsert.Core.Utilities;

public class URPackageParser
{
    public Waypoint? ParseJointPositions(byte[] data)
    {
        if (data.Length < 5)
        {
            return null;
        }

        int offset = 5;

        while (offset < data.Length - 5)
        {
            if (offset + 4 > data.Length)
                break;
                
            int subPackageSize = ReadInt32BigEndian(data, offset);
            offset += 4;

            if (offset >= data.Length)
                break;

            byte packageType = data[offset];
            offset++;

            int dataSize = subPackageSize - 5;
            
            if (dataSize < 0 || offset + dataSize > data.Length)
            {
                break;
            }

            if (packageType == PackageTypes.JointData)
            {
                var waypoint = new Waypoint();
                
                for (int i = 0; i < 6; i++)
                {
                    int jointOffset = offset + (i * 41);
                    
                    if (jointOffset + 8 > data.Length)
                    {
                        return null;
                    }
                    
                    waypoint.JointPositions[i] = ReadDoubleBigEndian(data, jointOffset);
                }
                return waypoint;
            }

            offset += dataSize;
        }
        return null;
    }

    public ToolData? ParseToolData(byte[] data)
    {
        if (data.Length < 5)
        {
            return null;
        }
        int offset = 5;

        while (offset < data.Length - 5)
        {
            if (offset + 4 > data.Length)
                break;
                
            int subPackageSize = ReadInt32BigEndian(data, offset);
            offset += 4;

            if (offset >= data.Length)
                break;

            byte packageType = data[offset];
            offset++;

            int dataSize = subPackageSize - 5;
            
            if (dataSize < 0 || offset + dataSize > data.Length)
                break;

            if (packageType == PackageTypes.ToolData)
            {
                if (offset + 32 > data.Length)
                {
                    return null;
                }
                
                return new ToolData
                {
                    AnalogInputRange0 = data[offset],
                    AnalogInputRange1 = data[offset + 1],
                    AnalogInput0 = ReadDoubleBigEndian(data, offset + 2),
                    AnalogInput1 = ReadDoubleBigEndian(data, offset + 10),
                    ToolVoltage48V = ReadFloatBigEndian(data, offset + 18),
                    ToolOutputVoltage = data[offset + 22],
                    ToolCurrent = ReadFloatBigEndian(data, offset + 23),
                    ToolTemperature = ReadFloatBigEndian(data, offset + 27),
                    ToolMode = data[offset + 31]
                };
            }

            offset += dataSize;
        }
        return null;
    }

    public int ReadInt32BigEndian(byte[] data, int offset)
    {
        if (BitConverter.IsLittleEndian)
        {
            return (data[offset] << 24) | 
                   (data[offset + 1] << 16) | 
                   (data[offset + 2] << 8) | 
                   data[offset + 3];
        }
        return BitConverter.ToInt32(data, offset);
    }

    private double ReadDoubleBigEndian(byte[] data, int offset)
    {
        if (BitConverter.IsLittleEndian)
        {
            byte[] reversed = new byte[8];
            for (int i = 0; i < 8; i++)
                reversed[7 - i] = data[offset + i];
            return BitConverter.ToDouble(reversed, 0);
        }
        return BitConverter.ToDouble(data, offset);
    }

    private float ReadFloatBigEndian(byte[] data, int offset)
    {
        if (BitConverter.IsLittleEndian)
        {
            byte[] reversed = new byte[4];
            for (int i = 0; i < 4; i++)
                reversed[3 - i] = data[offset + i];
            return BitConverter.ToSingle(reversed, 0);
        }
        return BitConverter.ToSingle(data, offset);
    }
}