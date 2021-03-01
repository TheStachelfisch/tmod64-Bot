using System.Threading.Tasks;
using Discord.Commands;

namespace tMod64Bot.Services.Commons
{
    public class TaskResult : ITaskResult
    {
        public string? ErrorReason { get; }
        public bool IsSuccess { get; }

        public static TaskResult FromSuccess(ITaskResult result) => new TaskResult(true, null);
        public static TaskResult FromSuccess() => new TaskResult(true, null);
        public static TaskResult FromError(ITaskResult result) => new TaskResult(false, result.ErrorReason);
        public static TaskResult FromError(string errorReason) => new TaskResult(false, errorReason);

        protected TaskResult(bool isSuccess, string? errorReason)
        {
            IsSuccess = isSuccess;
            ErrorReason = errorReason;
        }
    }
}