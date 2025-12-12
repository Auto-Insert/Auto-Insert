namespace AutoInsert.Shared.Models
{
    public class MoveStatus(bool success, string message)
    {
        public bool Success { get; set; } = success;
        public string Message { get; set; } = message;
    }
}