using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using tMod64Bot.Services.Commons;
using tMod64Bot.Services.Config;
using tMod64Bot.Services.Logging;

namespace tMod64Bot.Services.Tag
{
    public class TagService : ServiceBase
    {
        private readonly ConfigService _config;
        private readonly LoggingService _loggingService;
        
        public TagService(IServiceProvider services) : base(services)
        {
            _config = services.GetRequiredService<ConfigService>();
            _loggingService = services.GetRequiredService<LoggingService>();
        }

        public Task<TaskResult> AddTag(Config.Tag tag)
        {
            try
            {
                tag.Name = tag.Name.ToLower();
                
                if (_config.Config.Tags.Any(x => x.Name == tag.Name))
                    return Task.FromResult(TaskResult.FromError($"Tag {tag.Name} already exists"));
            
                _config.Config.Tags.Add(tag);
                _config.SaveData();
            }
            catch (Exception e)
            {
                _loggingService.Log(LogSeverity.Error, LogSource.Service, "Error while adding Tag", e);
                return Task.FromResult(TaskResult.FromError("Error while adding Tag"));
            }
            
            return Task.FromResult(TaskResult.FromSuccess());
        }
        
        public Task<TaskResult> RemoveTag(string tagName)
        {
            try
            {
                tagName = tagName.ToLower();
                
                if (_config.Config.Tags.All(x => x.Name != tagName))
                    return Task.FromResult(TaskResult.FromError($"Tag {tagName} doesn't exist"));

                _config.Config.Tags.Remove(_config.Config.Tags.Single(x => x.Name == tagName));
                _config.SaveData();
            }
            catch (Exception e)
            {
                _loggingService.Log(LogSeverity.Error, LogSource.Service, "Error while removing Tag", e);
                return Task.FromResult(TaskResult.FromError("Error while removing Tag"));
            }

            return Task.FromResult(TaskResult.FromSuccess());
        }

        public Task<TaskResult> EditTag(string tagName, string newContent)
        {
            try
            {
                tagName = tagName.ToLower();
                
                if (_config.Config.Tags.All(x => x.Name != tagName))
                    return Task.FromResult(TaskResult.FromError($"Tag {tagName} doesn't exist"));

                var tag = _config.Config.Tags.FirstOrDefault(x => x.Name == tagName);
                tag!.Content = newContent;
                _config.SaveData();
            }
            catch (Exception e)
            {
                _loggingService.Log(LogSeverity.Error, LogSource.Service, "Error while editing Tag", e);
                return Task.FromResult(TaskResult.FromError("Error while editing Tag"));
            }

            return Task.FromResult(TaskResult.FromSuccess());
        }

        public Task<Config.Tag> GetTag(string tagName)
        {
            try
            {
                tagName = tagName.ToLower();

                if (_config.Config.Tags.All(x => x.Name != tagName))
                    return null!;

                return Task.FromResult(_config.Config.Tags.FirstOrDefault(x => x.Name == tagName)!);
            }
            catch (Exception e)
            {
                _loggingService.Log(LogSeverity.Error, LogSource.Service, "Error while editing Tag", e);
                return null!;
            }
        }
    }
}