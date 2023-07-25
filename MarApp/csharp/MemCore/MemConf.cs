using System.Diagnostics;
using YamlDotNet.Serialization;

namespace MemCore
{
    public class MemConfigParser
    {
        public string? ConfigString { get; set; }
        public string? GameVersion { get; set; }

        public Dictionary<string, MemPointer> Pointers {get; set;} = new Dictionary<string, MemPointer>();
        private Dictionary<string, GameVersion> GameVersions { get; set; } = new Dictionary<string, GameVersion>();
        private Dictionary<string, StateConfig> StateConfigs { get; set; } = new Dictionary<string, StateConfig>();
        private Dictionary<string, StateStructConfig> StructConfigs { get; set; } = new Dictionary<string, StateStructConfig>();

        public Dictionary<string, State> States { get; set; } = new Dictionary<string, State>();
        public Dictionary<string, StateStruct> Structs { get; set; } = new Dictionary<string, StateStruct>();

        public void Parse(string? confFile)
        {
            // Config Parser
            //    Takes a config file (path string) and parses the contents into a MemConfig object
            if (confFile != null)
                ConfigString = File.ReadAllText(confFile);
            else if (ConfigString == null)
                throw new Exception("No config file provided");
        
            var deserializer = new DeserializerBuilder().Build();
            var configDict = (Dictionary<object, object>?) deserializer.Deserialize(new StringReader(ConfigString));

            if (configDict == null)
                return;

            // Iterate over the deserialized object and dispatch to the appropriate parsers
            foreach (var confItem in configDict)
            {
                var str = confItem.Key.ToString();
                if (str == null)
                    continue;
                str = str.ToUpper();

                // Dispatch to the appropriate parser
                if (str == "GAME_VERSIONS")
                    GameVersions = ParseGameVersions(ToObjDict(confItem.Value));
                else if (str == "STATES")
                    StateConfigs = ParseStateConfigs(ToObjDict(confItem.Value));
                else if (str == "STRUCTS")
                    StructConfigs = ParseStructConfigs(ToObjDict(confItem.Value));

            }
        }
    }
}