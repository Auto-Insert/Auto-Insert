using Autoinsert.Shared.Models;
using AutoInsert.Shared.Models;

namespace AutoInsert.Core.Services.Control;

public class CoordinateService
{
    public CalibrationData? Calibration { get; private set; }
    
    // Transformation matrix from local to global coordinates
    private double[,]? _transformationMatrix;
    private double[]? _origin;
    private bool _isCalibrated = false;

    public void SetCalibrationData(CalibrationData calibrationData)
    {
        Calibration = calibrationData;
        ComputeTransformationMatrix();
        _isCalibrated = true;
    }

    private void ComputeTransformationMatrix()
    {
        if (Calibration?.ZeroPoint?.CartesianPositions == null ||
            Calibration?.XAxisPoint?.CartesianPositions == null ||
            Calibration?.YAxisPoint?.CartesianPositions == null)
        {
            throw new InvalidOperationException("Calibration data is incomplete");
        }

        // Origin of the local coordinate system
        _origin = new double[]
        {
            Calibration.ZeroPoint.CartesianPositions.X,
            Calibration.ZeroPoint.CartesianPositions.Y,
            Calibration.ZeroPoint.CartesianPositions.Z
        };

        // X-axis vector (from origin to X-axis point)
        double[] xVector = new double[]
        {
            Calibration.XAxisPoint.CartesianPositions.X - _origin[0],
            Calibration.XAxisPoint.CartesianPositions.Y - _origin[1],
            Calibration.XAxisPoint.CartesianPositions.Z - _origin[2]
        };

        // Y-axis vector (from origin to Y-axis point)
        double[] yVector = new double[]
        {
            Calibration.YAxisPoint.CartesianPositions.X - _origin[0],
            Calibration.YAxisPoint.CartesianPositions.Y - _origin[1],
            Calibration.YAxisPoint.CartesianPositions.Z - _origin[2]
        };

        // Normalize the X-axis
        double xLength = Math.Sqrt(xVector[0] * xVector[0] + xVector[1] * xVector[1] + xVector[2] * xVector[2]);
        xVector[0] /= xLength;
        xVector[1] /= xLength;
        xVector[2] /= xLength;

        // Compute Z-axis as cross product of X and Y vectors
        double[] zVector = new double[]
        {
            xVector[1] * yVector[2] - xVector[2] * yVector[1],
            xVector[2] * yVector[0] - xVector[0] * yVector[2],
            xVector[0] * yVector[1] - xVector[1] * yVector[0]
        };

        // Normalize Z-axis
        double zLength = Math.Sqrt(zVector[0] * zVector[0] + zVector[1] * zVector[1] + zVector[2] * zVector[2]);
        zVector[0] /= zLength;
        zVector[1] /= zLength;
        zVector[2] /= zLength;

        // Recompute Y-axis as cross product of Z and X to ensure orthogonality
        yVector[0] = zVector[1] * xVector[2] - zVector[2] * xVector[1];
        yVector[1] = zVector[2] * xVector[0] - zVector[0] * xVector[2];
        yVector[2] = zVector[0] * xVector[1] - zVector[1] * xVector[0];

        // Create transformation matrix (local to global)
        // Each column represents the direction of local axis in global coordinates
        _transformationMatrix = new double[3, 3]
        {
            { xVector[0], yVector[0], zVector[0] },
            { xVector[1], yVector[1], zVector[1] },
            { xVector[2], yVector[2], zVector[2] }
        };
    }

    public CartesianPositions LocalToGlobal(double localX, double localY, double localZ)
    {
        if (!_isCalibrated || _transformationMatrix == null || _origin == null)
        {
            throw new InvalidOperationException("Coordinate system is not calibrated");
        }

        // Apply transformation: global = origin + matrix * local
        double globalX = _origin[0] + 
            _transformationMatrix[0, 0] * localX + 
            _transformationMatrix[0, 1] * localY + 
            _transformationMatrix[0, 2] * localZ;

        double globalY = _origin[1] + 
            _transformationMatrix[1, 0] * localX + 
            _transformationMatrix[1, 1] * localY + 
            _transformationMatrix[1, 2] * localZ;

        double globalZ = _origin[2] + 
            _transformationMatrix[2, 0] * localX + 
            _transformationMatrix[2, 1] * localY + 
            _transformationMatrix[2, 2] * localZ;

        // Keep orientation from the zero point
        return new CartesianPositions
        {
            X = globalX,
            Y = globalY,
            Z = globalZ,
            Rx = Calibration!.ZeroPoint.CartesianPositions!.Rx,
            Ry = Calibration.ZeroPoint.CartesianPositions.Ry,
            Rz = Calibration.ZeroPoint.CartesianPositions.Rz,
            TCPOffsetX = Calibration.ZeroPoint.CartesianPositions.TCPOffsetX,
            TCPOffsetY = Calibration.ZeroPoint.CartesianPositions.TCPOffsetY,
            TCPOffsetZ = Calibration.ZeroPoint.CartesianPositions.TCPOffsetZ,
            TCPOffsetRx = Calibration.ZeroPoint.CartesianPositions.TCPOffsetRx,
            TCPOffsetRy = Calibration.ZeroPoint.CartesianPositions.TCPOffsetRy,
            TCPOffsetRz = Calibration.ZeroPoint.CartesianPositions.TCPOffsetRz
        };
    }

    public (double X, double Y, double Z) GlobalToLocal(CartesianPositions globalPosition)
    {
        if (!_isCalibrated || _transformationMatrix == null || _origin == null)
        {
            throw new InvalidOperationException("Coordinate system is not calibrated");
        }

        // Translate to origin
        double dx = globalPosition.X - _origin[0];
        double dy = globalPosition.Y - _origin[1];
        double dz = globalPosition.Z - _origin[2];

        // Apply inverse transformation (transpose of rotation matrix)
        double localX = _transformationMatrix[0, 0] * dx + 
                        _transformationMatrix[1, 0] * dy + 
                        _transformationMatrix[2, 0] * dz;

        double localY = _transformationMatrix[0, 1] * dx + 
                        _transformationMatrix[1, 1] * dy + 
                        _transformationMatrix[2, 1] * dz;

        double localZ = _transformationMatrix[0, 2] * dx + 
                        _transformationMatrix[1, 2] * dy + 
                        _transformationMatrix[2, 2] * dz;

        return (localX, localY, localZ);
    }

    public Waypoint CreateWaypointAtLocal(double localX, double localY, double localZ, string name)
    {
        var globalPosition = LocalToGlobal(localX, localY, localZ);
        
        return new Waypoint
        {
            Name = name,
            CartesianPositions = globalPosition,
            // Joint positions would need to be computed by inverse kinematics
            JointPositions = null
        };
    }

    public bool IsCalibrated => _isCalibrated;
}