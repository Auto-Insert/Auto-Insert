using AutoInsert.Core.Services.Control;
using AutoInsert.Core.Services.Data;
using AutoInsert.Shared.Models;
using Autoinsert.Shared.Models;

namespace AutoInsert.Core.Controllers;

public class CalibrationController
{
    private readonly URController _urController;
    private readonly CoordinateService _coordinateService;
    private readonly StorageService _storageService;

    public CalibrationController(URController urController, CoordinateService coordinateService, StorageService storageService)
    {
        _urController = urController;
        _coordinateService = coordinateService;
        _storageService = storageService;
    }
    public async Task<Waypoint?> GetCurrentPositionAsync(string name)
    {
        return await _urController.GetWaypointFromCurrentPositionsAsync(name);
    }
    public (bool IsValid, string? ErrorMessage) ValidateCalibrationPoint(Waypoint? waypoint,Waypoint? referencePoint = null,double minDistanceMm = 10.0)
    {
        if (waypoint?.CartesianPositions == null)
            return (false, "Waypoint has no position data");

        if (referencePoint?.CartesianPositions != null)
        {
            double distance = Math.Sqrt(
                Math.Pow(waypoint.CartesianPositions.X - referencePoint.CartesianPositions.X, 2) +
                Math.Pow(waypoint.CartesianPositions.Y - referencePoint.CartesianPositions.Y, 2) +
                Math.Pow(waypoint.CartesianPositions.Z - referencePoint.CartesianPositions.Z, 2)) * 1000;

            if (distance < minDistanceMm)
                return (false, $"Point is too close to reference ({distance:F1}mm). Minimum distance: {minDistanceMm}mm");
        }

        return (true, null);
    }
    public double CalculateDistance(Waypoint point1, Waypoint point2)
    {
        if (point1?.CartesianPositions == null || point2?.CartesianPositions == null)
            return 0;

        return Math.Sqrt(
            Math.Pow(point1.CartesianPositions.X - point2.CartesianPositions.X, 2) +
            Math.Pow(point1.CartesianPositions.Y - point2.CartesianPositions.Y, 2) +
            Math.Pow(point1.CartesianPositions.Z - point2.CartesianPositions.Z, 2)) * 1000;
    }
    public (bool Success, string? ErrorMessage) CalibrateFromWaypoints(Waypoint zeroPoint,Waypoint xAxisPoint,Waypoint yAxisPoint)
    {
        try
        {
            // Validate all points have data
            var zeroValidation = ValidateCalibrationPoint(zeroPoint);
            if (!zeroValidation.IsValid)
                return (false, $"zero point invalid: {zeroValidation.ErrorMessage}");

            var xValidation = ValidateCalibrationPoint(xAxisPoint, zeroPoint);
            if (!xValidation.IsValid)
                return (false, $"X-axis point invalid: {xValidation.ErrorMessage}");

            var yValidation = ValidateCalibrationPoint(yAxisPoint, zeroPoint);
            if (!yValidation.IsValid)
                return (false, $"Y-axis point invalid: {yValidation.ErrorMessage}");

            // Check that X and Y points are not collinear
            double xyDistance = CalculateDistance(xAxisPoint, yAxisPoint);
            if (xyDistance < 10.0)
                return (false, "X-axis and Y-axis points are too close together");

            // Create calibration data
            var calibrationData = new CalibrationData(zeroPoint, xAxisPoint, yAxisPoint);

            // Apply to coordinate service
            _coordinateService.SetCalibrationData(calibrationData);

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
    public async Task<bool> SaveCalibrationAsync()
    {
        if (_coordinateService.Calibration == null)
            return false;

        AppConfiguration config = await _storageService.LoadConfigAsync();
        config.CalibrationData = _coordinateService.Calibration;
        config.LastCalibrationTime = DateTime.Now;
        config.IsCalibrated = true;

        return await _storageService.SaveConfigAsync(config);
    }
    public async Task<(bool Success, string? ErrorMessage)> LoadCalibrationAsync()
    {
        try
        {
            var config = await _storageService.LoadConfigAsync();

            if (config.CalibrationData == null)
                return (false, "No saved calibration found");

            _coordinateService.SetCalibrationData(config.CalibrationData);

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
    public async Task<bool> HasSavedCalibrationAsync()
    {
        var config = await _storageService.LoadConfigAsync();
        return config.IsCalibrated && config.CalibrationData != null;
    }
    public CalibrationData? GetCalibrationInfo()
    {
        if (_coordinateService.Calibration == null)
            return null;
        return _coordinateService.Calibration;
    }
}