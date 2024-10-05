using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ManaSpline
{
    public class RecordingFileWriter : IDisposable
    {
        private readonly string _filePath;
        private readonly SemaphoreSlim _fileSemaphore = new SemaphoreSlim(1, 1);
        private readonly ConcurrentQueue<string> _writeQueue = new ConcurrentQueue<string>();
        public Task fileWriterTask;
        private bool _isRunning = true;

        public bool IsWriting = false;

        public RecordingFileWriter(string filePath)
        {
            _filePath = filePath;
            _isRunning = true;
            fileWriterTask = Task.Run(() => ProcessWriteQueue());

            // Backup the old file if it exists
            string oldFile = _filePath + ".old";
            if (File.Exists(oldFile))
                File.Replace(_filePath, oldFile, null);
        }

        // Enqueue a new record to be written
        public async Task EnqueueRecordAsync(string json)
        {
            _writeQueue.Enqueue(json);
            await Task.CompletedTask;
        }

        // Process the write queue and ensure records are written in order
        private async Task ProcessWriteQueue()
        {
            while (_isRunning || !_writeQueue.IsEmpty)
            {
                IsWriting = true;
                if (_writeQueue.TryDequeue(out var json))
                {
                    await WriteToFileAsync(json);
                }
                await Task.Delay(10);
            }
        }

        // Actual method to write the record to the file
        private async Task WriteToFileAsync(string json)
        {
            try
            {
                await _fileSemaphore.WaitAsync();
                await File.AppendAllTextAsync(_filePath, json + Environment.NewLine);
                ManaSpline.AppendLog(json);
            }
            finally
            {
                _fileSemaphore.Release();
            }
        }

        public static string ToJson(object record) => JsonConvert.SerializeObject(record, Formatting.None);

        //
        public void Stop()
        {
            _isRunning = false;
            Task.Run(() => CompleteAsync());
        }

        // Method to signal that no more records are being enqueued
        public async Task CompleteAsync()
        {
            _isRunning = false;
            await fileWriterTask;
            IsWriting = false;
        }

        // Ensure the writer is disposed and any remaining records are flushed
        public void Dispose()
        {
            fileWriterTask?.Wait();
            _fileSemaphore?.Dispose();
        }
    }

    public class RecordPostProcessor
    {
        private readonly string _filePath;

        public RecordPostProcessor(string filePath)
        {
            _filePath = filePath;
        }

        public void Process()
        {
            var lines = File.ReadAllLines(_filePath);
            var sorted = new SortedDictionary<double, Dictionary<string, object>>();

            ManaSpline.AppendLog("Processing File...");
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var obj = JsonConvert.DeserializeObject<Dictionary<string, object>>(line);
                    var igt = (double)obj["IGT"];

                    if (!sorted.TryAdd(igt, obj))
                    {
                        foreach (var kvp in obj)
                        {
                            sorted[igt][kvp.Key] = kvp.Value;
                        }
                    }
                }
            }

            ManaSpline.AppendLog("Writing Processed File...");

            string sortedFile = _filePath + ".sorted";

            using (StreamWriter sw = File.CreateText(sortedFile))
            {
                foreach (var obj in sorted)
                {
                    sw.WriteLine(RecordingFileWriter.ToJson(obj.Value));
                }
            }

            File.Replace(sortedFile, _filePath, _filePath + ".backup");

            ManaSpline.AppendLog("Complete!");
        }
    }
}