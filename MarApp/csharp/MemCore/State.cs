using System.Diagnostics;

namespace MemCore
{
    public class StateConfig
    {
        public string Name { get; set; }
        public string? TypeStr { get; set; }
        public int BaseOffset { get; set; } = 0x0;
        public int[]? Levels { get; set; }

        public int ValueOffset { get; set; }
        public object? Default { get; set; }

        public StateConfig (string name, string? typeStr, int baseOffset = 0x0, int[]? levels = null, int valueOffset = 0x0, object? defaultValue=null)
        {
            Name = name;
            TypeStr = typeStr;
            BaseOffset = baseOffset;
            Levels = levels;
            ValueOffset = valueOffset;
            Default = defaultValue;
        }

        public State Build (Process process) => new State(this, process);
    }

    public class State : StateConfig
    {
        private Process Process { get; set; }
        private MemPointer Pointer { get; set; }
    }
}