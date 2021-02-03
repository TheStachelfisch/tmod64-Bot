using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using tMod64Bot.Services.Logging;

namespace tMod64Bot.Services.Config
{
    public class ConfigService : ServiceBase
    {
        private readonly string _configPath = $"{ServiceConstants.DATA_DIR}ServerConfig.json";
        private readonly LoggingService _log;
        private Config _config;

        public Config Config
        {
            get => _config;
            set => _config = value;
        }

        public ConfigService(IServiceProvider services) : base(services)
        {
            _log = Services.GetRequiredService<LoggingService>();

            if (!File.Exists(_configPath))
                FirstTimeSetup().GetAwaiter().GetResult();
            else
                SetupData().GetAwaiter().GetResult();
        }

        private Task SetupData()
        {
            Stopwatch sw = Stopwatch.StartNew();
            string text = File.ReadAllText(_configPath);
            _config = JsonConvert.DeserializeObject<Config>(text);
            
            sw.Stop();
            _log.Log(LogSeverity.Info, LogSource.Service, $"Loaded Config Data in {sw.ElapsedMilliseconds}ms");
            
            return Task.CompletedTask;
        }

        public void SaveData()
        {
            try
            {
                File.WriteAllText(_configPath, JsonConvert.SerializeObject(_config, Formatting.Indented));
            }
            catch (Exception e)
            {
                _log.Log(LogSeverity.Error, LogSource.Service, "Failed to save Config Data", e);
                return;
            }

            _log.Log(LogSeverity.Info, LogSource.Service, "Successfully Saved Data");
        }

        public void SaveData(string json)
        {
            try
            {
                File.WriteAllText(_configPath, json);
            }
            catch (Exception e)
            {
                _log.Log(LogSeverity.Error, LogSource.Service, "Failed to save Config Data", e);
                return;
            }

            _log.Log(LogSeverity.Info, LogSource.Service, "Successfully Saved Data");
        }

        public void UpdateData()
        {
            String json = File.ReadAllText(_configPath);
            Config = JsonConvert.DeserializeObject<Config>(json);
        }

        public bool ChangeKey(string key, string value)
        {
            var json = File.ReadAllText(_configPath);
            var jObjects = JObject.Parse(json);

            if (!jObjects.ContainsKey(key))
                return false;


            if (ulong.TryParse(value, out ulong ulongResult) && jObjects[key].Type == JTokenType.Integer)
                jObjects[key] = ulongResult;
            else if(jObjects[key].Type == JTokenType.String)
                jObjects[key] = value;
            else
                throw new TypeMismatchException($"Expected Value Type '{jObjects[key].Type}'");

            SaveData(jObjects.ToString());
            UpdateData();
            return true;
        }

        private Task FirstTimeSetup()
        {
            try
            {
                _config = new Config
                {
                    BannedWords = new HashSet<string>(),
                    MutedUsers = new Dictionary<ulong, ulong>(),
                    BannedWordsExemptChannel = new HashSet<ulong>(),
                    BotPrefix = "64!"
                };
            
                string json = JsonConvert.SerializeObject(_config, Formatting.Indented);
            
                File.WriteAllText(_configPath, json);
            }
            catch (Exception e)
            {
                _log.Log(LogSeverity.Critical, LogSource.Service, "Failed completing first-time Config setup");
                throw;
            }
            
            _log.Log(LogSeverity.Info,LogSource.Service, "Completed first-time Setup");

            return Task.CompletedTask;
        }
    }
}