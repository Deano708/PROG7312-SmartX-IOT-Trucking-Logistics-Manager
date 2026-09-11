namespace SmartX.Shared.Services;

using SmartX.Shared.Models;

public class TelemetryBatchProcessor
{
    // Flattens sequential historical batches from raw arrays into List<T>
    public List<TelemetryPacket<T>> ProcessJaggedBatch<T>(T[][] rawBatches, string[] macAddresses) where T : struct
    {
        int totalReadings = 0;
        for (int i = 0; i < rawBatches.Length; i++) totalReadings += rawBatches[i].Length;

        // Pre-allocate capacity to eliminate internal array resizing overhead
        var resultList = new List<TelemetryPacket<T>>(totalReadings);

        for (int batchIndex = 0; batchIndex < rawBatches.Length; batchIndex++)
        {
            string mac = macAddresses[batchIndex % macAddresses.Length];
            for (int readingIndex = 0; readingIndex < rawBatches[batchIndex].Length; readingIndex++)
            {
                resultList.Add(new TelemetryPacket<T>(mac, rawBatches[batchIndex][readingIndex]));
            }
        }

        return resultList;
    }
}