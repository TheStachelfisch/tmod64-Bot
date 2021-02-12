namespace tMod64Bot.Services.Commons
{
    public interface ITaskResult
    {
        string? ErrorReason { get; }
        bool IsSuccess { get; }
    }
}