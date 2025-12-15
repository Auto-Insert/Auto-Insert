using AutoInsert.Shared.Models;

namespace AutoInsert.Core.Services.Control.StepHandlers
{
    public class WaitSeconds : SequenceStep
    {
        public int Seconds { get; set; }

        public WaitSeconds(int seconds)
        {
            StepType = StepType.WaitSeconds;
            Seconds = seconds;
        }

        public override async Task ExecuteAsync()
        {
            try
            {
                StartStep();

                await Task.Delay(Seconds * 1000);

                CompleteStep(true, null);
            }
            catch (Exception ex)
            {
                CompleteStep(false, ex.Message);
            }
        }
    }
}