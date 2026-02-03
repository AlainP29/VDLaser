namespace VDLaser.Core.Grbl.Services
{
    /// <summary>
    /// Buffer circulaire RX pour conserver les derniers messages GRBL reçus.
    /// Sert à reconstruire le contexte (ok / error).
    /// </summary>
    internal sealed class GrblRxRingBuffer
    {
        private readonly string[] _buffer;
        private int _index;
        private readonly object _sync = new();

        public GrblRxRingBuffer(int capacity = 16)
        {
            _buffer = new string[capacity];
        }

        public void Push(string line)
        {
            lock (_sync)
            {
                _buffer[_index] = line;
                _index = (_index + 1) % _buffer.Length;
            }
        }

        public IReadOnlyList<string> Snapshot()
        {
            lock (_sync)
            {
                return _buffer
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .ToList();
            }
        }

        public bool ErrorAfterOk()
        {
            var snap = Snapshot();

            int lastOkIndex = -1;
            int lastErrorIndex = -1;

            for (int i = snap.Count - 1; i >= 0; i--)
            {
                if (lastErrorIndex == -1 && snap[i].StartsWith("error:"))
                    lastErrorIndex = i;

                if (lastOkIndex == -1 && snap[i] == "ok")
                    lastOkIndex = i;

                if (lastOkIndex != -1 && lastErrorIndex != -1)
                    break;
            }

            return lastOkIndex >= 0 && lastErrorIndex > lastOkIndex;
        }


        public int? LastErrorCode()
        {
            var snap = Snapshot();

            for (int i = snap.Count - 1; i >= 0; i--)
            {
                var line = snap[i];
                if (line.StartsWith("error:") &&
                    int.TryParse(line.Split(':')[1], out int code))
                {
                    return code;
                }
            }

            return null;
        }
    }
}
