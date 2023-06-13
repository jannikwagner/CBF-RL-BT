
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public interface IStorageManager
{
    void AddStatistic(CompositeEpisodeStatistic statistic);
    void StoreStatistics();
}

public class StorageManager : IStorageManager
{
    private string folderPath;
    private int checkpointInterval;
    private List<CompositeEpisodeStatistic> statistics = new List<CompositeEpisodeStatistic>();

    public StorageManager(string folderPath, int checkpointInterval = 1)
    {
        this.folderPath = folderPath;
        this.checkpointInterval = checkpointInterval;

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
    }

    public void AddStatistic(CompositeEpisodeStatistic statistic)
    {
        statistics.Add(statistic);

        if (statistics.Count % checkpointInterval == 0)
        {
            StoreStatistics();
        }
    }

    public void StoreStatistics()
    {
        var filePath = folderPath + "/statistics.json";
        using (StreamWriter file = File.CreateText(filePath))
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(file, statistics);
        }
    }
}

public class SmallFilesStorageManager : IStorageManager
{

    private string folderPath;
    private List<CompositeEpisodeStatistic> compositeEpisodeStatistics = new List<CompositeEpisodeStatistic>();

    public SmallFilesStorageManager(string folderPath)
    {
        this.folderPath = folderPath;

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
    }

    public void AddStatistic(CompositeEpisodeStatistic statistic)
    {
        // write to file
        string statisticJson = Newtonsoft.Json.JsonConvert.SerializeObject(statistic);

        var filePath = folderPath + "/compositeEpisode" + statistic.compositeEpisodeNumber + ".json";
        using (StreamWriter file = File.CreateText(filePath))
        {
            file.Write(statisticJson);
        }
    }

    public void StoreStatistics()
    {
        // do nothing
    }
}
