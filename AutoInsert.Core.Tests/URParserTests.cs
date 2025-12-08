using AutoInsert.Core.Utilities;
using AutoInsert.Core.Services.Communication;
using Xunit;

namespace AutoInsert.Core.Tests;

public class URParserTests
{
    private readonly URPackageParser _parser;

    public URParserTests()
    {
        _parser = new URPackageParser();
    }

    [Fact]
    public void ParseJointPositions_ValidData_ReturnsWaypoint()
    {
        // Arrange - Create a minimal valid package with joint data
        var data = new byte[5 + 4 + 1 + (6 * 41)]; // Header + size + type + 6 joints
        
        // Package header (5 bytes)
        data[0] = 0; data[1] = 0; data[2] = 0; data[3] = 0; data[4] = 0;
        
        // Sub-package size (4 bytes) = 5 (header) + 246 (6 joints * 41 bytes)
        int subPackageSize = 251;
        data[5] = (byte)(subPackageSize >> 24);
        data[6] = (byte)(subPackageSize >> 16);
        data[7] = (byte)(subPackageSize >> 8);
        data[8] = (byte)subPackageSize;
        
        // Package type
        data[9] = PackageTypes.JointData;
        
        // Joint data (first joint = 1.5 radians)
        int jointOffset = 10;
        byte[] jointValue = BitConverter.GetBytes(1.5);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(jointValue);
        Array.Copy(jointValue, 0, data, jointOffset, 8);

        // Act
        var result = _parser.ParseJointPositions(data);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1.5, result[0], 2);
    }

    [Fact]
    public void ParseJointPositions_InsufficientData_ReturnsNull()
    {
        // Arrange
        var data = new byte[3];

        // Act
        var result = _parser.ParseJointPositions(data);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ParseJointPositions_NoJointDataPackage_ReturnsNull()
    {
        // Arrange - Package with wrong type
        var data = new byte[20];
        data[5] = 0; data[6] = 0; data[7] = 0; data[8] = 15;
        data[9] = 99; // Wrong package type

        // Act
        var result = _parser.ParseJointPositions(data);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ParseToolData_ValidData_ReturnsToolData()
    {
        // Arrange
        var data = new byte[5 + 4 + 1 + 32]; // Header + size + type + tool data
        
        // Package header
        data[0] = 0; data[1] = 0; data[2] = 0; data[3] = 0; data[4] = 0;
        
        // Sub-package size = 5 + 32 = 37
        data[5] = 0; data[6] = 0; data[7] = 0; data[8] = 37;
        
        // Package type
        data[9] = PackageTypes.ToolData;
        
        // Tool data fields
        int offset = 10;
        data[offset] = 1; // AnalogInputRange0
        data[offset + 1] = 2; // AnalogInputRange1
        
        // AnalogInput0 (double = 3.5)
        byte[] analogInput0 = BitConverter.GetBytes(3.5);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(analogInput0);
        Array.Copy(analogInput0, 0, data, offset + 2, 8);
        
        // ToolVoltage48V (float = 48.0)
        byte[] voltage = BitConverter.GetBytes(48.0f);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(voltage);
        Array.Copy(voltage, 0, data, offset + 18, 4);

        // Act
        var result = _parser.ParseToolData(data);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.AnalogInputRange0);
        Assert.Equal(2, result.AnalogInputRange1);
        Assert.Equal(3.5, result.AnalogInput0, 2);
        Assert.Equal(48.0f, result.ToolVoltage48V, 1);
    }

    [Fact]
    public void ParseToolData_InsufficientData_ReturnsNull()
    {
        // Arrange
        var data = new byte[3];

        // Act
        var result = _parser.ParseToolData(data);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ParseToolData_NoToolDataPackage_ReturnsNull()
    {
        // Arrange
        var data = new byte[20];
        data[5] = 0; data[6] = 0; data[7] = 0; data[8] = 15;
        data[9] = 99; // Wrong package type

        // Act
        var result = _parser.ParseToolData(data);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ReadInt32BigEndian_ConvertsCorrectly()
    {
        // Arrange
        var data = new byte[] { 0x00, 0x00, 0x01, 0x00 }; // 256 in big-endian

        // Act
        var result = _parser.ReadInt32BigEndian(data, 0);

        // Assert
        Assert.Equal(256, result);
    }

    [Fact]
    public void ReadInt32BigEndian_LargeValue_ConvertsCorrectly()
    {
        // Arrange
        var data = new byte[] { 0x12, 0x34, 0x56, 0x78 }; // 305419896 in big-endian

        // Act
        var result = _parser.ReadInt32BigEndian(data, 0);

        // Assert
        Assert.Equal(305419896, result);
    }

    [Fact]
    public void ParseJointPositions_CorruptedPackageSize_ReturnsNull()
    {
        // Arrange - Package size larger than actual data
        var data = new byte[20];
        data[5] = 0xFF; data[6] = 0xFF; data[7] = 0xFF; data[8] = 0xFF;
        data[9] = PackageTypes.JointData;

        // Act
        var result = _parser.ParseJointPositions(data);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ParseToolData_CorruptedPackageSize_ReturnsNull()
    {
        // Arrange - Package size larger than actual data
        var data = new byte[20];
        data[5] = 0xFF; data[6] = 0xFF; data[7] = 0xFF; data[8] = 0xFF;
        data[9] = PackageTypes.ToolData;

        // Act
        var result = _parser.ParseToolData(data);

        // Assert
        Assert.Null(result);
    }
}