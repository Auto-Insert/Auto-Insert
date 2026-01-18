using AutoInsert.Core.Controllers;
using AutoInsert.Core.Services.Communication;
using AutoInsert.Core.Services.Control.StepHandlers;
using AutoInsert.Shared.Models;

namespace AutoInsert.Core.Services.Control;

public class StepFactory
{
    private readonly URController _urController;
    private readonly CoordinateService _coordinateService;
    private readonly UartService _uartService;
    private readonly StepperMotorService _stepperMotorService;
    private readonly ServoMotorService _servoMotorService;
    private readonly SolenoidActuatorService _solenoidActuatorService;
    private readonly LinearActuatorService _linearActuatorService;

    public StepFactory(
        URController urController,
        CoordinateService coordinateService,
        UartService uartService,
        StepperMotorService stepperMotorService,
        ServoMotorService servoMotorService,
        SolenoidActuatorService solenoidActuatorService,
        LinearActuatorService linearActuatorService)
    {
        _urController = urController;
        _coordinateService = coordinateService;
        _uartService = uartService;
        _stepperMotorService = stepperMotorService;
        _servoMotorService = servoMotorService;
        _solenoidActuatorService = solenoidActuatorService;
        _linearActuatorService = linearActuatorService;
    }

    public SequenceStep? CreateHandler(SequenceStep stepData)
    {
        switch (stepData.StepType)
        {
            case StepType.MoveURToPosition:
                var move = (MoveURToPosition)stepData;
                return new MoveURToPosition(
                    _urController,
                    _coordinateService,
                    move.TargetPosition!,
                    move.Speed,
                    move.Acceleration,
                    move.GripPart)
                {
                    Name = move.Name,
                    Description = move.Description
                };
            case StepType.WaitSeconds:
                var wait = (WaitSeconds)stepData;
                return new WaitSeconds(wait.Seconds)
                {
                    Name = wait.Name,
                    Description = wait.Description
                };
            case StepType.RotatePartToThreadHole:
                var rotate = (RotatePartToThreadHole)stepData;
                return new RotatePartToThreadHole(
                    _urController,
                    _coordinateService,
                    rotate.ScrewingPosition!,
                    rotate.Speed,
                    rotate.Acceleration)
                {
                    Name = rotate.Name,
                    Description = rotate.Description
                };
            case StepType.ScrewInThePlug:
                var screwIn = (ScrewInThePlug)stepData;
                return new ScrewInThePlug(
                    _uartService,
                    _servoMotorService,
                    _stepperMotorService,
                    _solenoidActuatorService,
                    _linearActuatorService,
                    screwIn.PlugType!)
                {
                    Name = screwIn.Name,
                    Description = screwIn.Description
                };
            case StepType.ResetRail:
                var reset = (ResetRail)stepData;
                return new ResetRail(
                    _uartService,
                    _stepperMotorService,
                    _linearActuatorService,
                    reset.Direction)
                {
                    Name = reset.Name,
                    Description = reset.Description
                };
            case StepType.AttachPlug:
                var attach = (AttachPlug)stepData;
                return new AttachPlug(
                    _uartService,
                    _stepperMotorService,
                    _linearActuatorService,
                    _solenoidActuatorService,
                    _servoMotorService)
                {
                    Name = attach.Name,
                    Description = attach.Description
                };
            case StepType.DispenseGlue:
                var dispense = (DispenseGlue)stepData;
                return new DispenseGlue(
                    _uartService,
                    _stepperMotorService,
                    _linearActuatorService)
                {
                    Name = dispense.Name,
                    Description = dispense.Description
                };
            case StepType.ScrewPlug:
                var screwPlug = (ScrewPlug)stepData;
                return new ScrewPlug(
                    _uartService,
                    _stepperMotorService,
                    _linearActuatorService)
                {
                    Name = screwPlug.Name,
                    Description = screwPlug.Description
                };
            case StepType.ChangeToolBit:
                var changeTool = (ChangeToolBit)stepData;
                return new ChangeToolBit(
                    _uartService,
                    _stepperMotorService,
                    _linearActuatorService)
                {
                    Name = changeTool.Name,
                    Description = changeTool.Description
                };
            case StepType.DetachTool:
                var detach = (DetachTool)stepData;
                return new DetachTool(
                    _uartService,
                    _stepperMotorService,
                    _linearActuatorService)
                {
                    Name = detach.Name,
                    Description = detach.Description
                };
            default:
                return null;
        }
    }
}