using Newtonsoft.Json;
using System.IO;

namespace C3PO.Net
{
    public class GlobalSettings
	{
        // Define the location of the config file
        const string _path = "./config/global.json";
        static GlobalSettings _instance = new GlobalSettings();

        // Load the config file into memory
		public static void Load()
		{
			if (!File.Exists(_path))
				throw new FileNotFoundException($"{_path} is missing.");
			_instance = JsonConvert.DeserializeObject<GlobalSettings>(File.ReadAllText(_path));

		}

        // Write to the config file from memory (not yet used)
		public static void Save()
		{
			using (var stream = new FileStream(_path, FileMode.Create, FileAccess.Write, FileShare.None))
			using (var writer = new StreamWriter(stream))
				writer.Write(JsonConvert.SerializeObject(_instance, Formatting.Indented));
		}

		//Discord settings
		public class DiscordSettings
		{
			[JsonProperty("username")]
			public string Email;
			[JsonProperty("password")]
			public string Password;
		}
		[JsonProperty("discord")]
		DiscordSettings _discord = new DiscordSettings();
		public static DiscordSettings Discord => _instance._discord;

		//Users settings
		public class UserSettings
		{
			[JsonProperty("dev")]
			public ulong DevId;
		}
		[JsonProperty("users")]
		UserSettings _users = new UserSettings();
		public static UserSettings Users => _instance._users;
	}
}
