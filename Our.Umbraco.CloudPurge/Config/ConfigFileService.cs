using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Xml.Serialization;
using Umbraco.Cms.Core.IO;
using static Umbraco.Cms.Core.Constants;

namespace Our.Umbraco.CloudPurge.Config
{
	internal class ConfigFileService : IConfigService
	{
		private readonly ILogger<ConfigFileService> _logger;
		private readonly string _configFilePath;

		public ConfigFileService(ILogger<ConfigFileService> logger, IWebHostEnvironment webHostEnvironment)
		{
			_logger = logger;
			_configFilePath = $"{webHostEnvironment.WebRootPath}/{SystemDirectories.Config}/CloudPurge.config";
		}

		private CloudPurgeConfig _config;

		public CloudPurgeConfig GetConfig()
		{
			if (_config != null)
				return _config;

			lock (_configFilePath)
			{
				if (_config != null)
					return _config;

				return _config = ReadConfigFromFile()
					?? new CloudPurgeConfig(false,
						new ContentFilterConfig(Array.Empty<string>(), Array.Empty<string>()),
						new CloudFlareConfig(false, "", "", ""));
			}
		}

		public void WriteConfig(CloudPurgeConfig config)
		{
			if (!Directory.Exists(SystemDirectories.Config))
				Directory.CreateDirectory(SystemDirectories.Config);

			var serializer = new XmlSerializer(typeof(CloudPurgeConfig));

			using (var file = File.Create(_configFilePath))
			{
				serializer.Serialize(file, config);
				file.Close();
			}

			_config = config;
		}

		private CloudPurgeConfig ReadConfigFromFile()
		{
			if (!File.Exists(_configFilePath))
				return null;

			var serializer = new XmlSerializer(typeof(CloudPurgeConfig));
			
			try
			{
                using var fileStream = File.OpenRead(_configFilePath);
                var config = (CloudPurgeConfig)serializer.Deserialize(fileStream);
                return config;
            }
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error reading CloudPurge config file @filePath", _configFilePath);
				return null;
			}
		}
	}
}
