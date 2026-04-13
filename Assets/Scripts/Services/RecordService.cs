using System.Threading.Tasks;

public static class RecordService
{
    public static Task<int?> LoadRecordAsync()
    {
        return GameProgressSaver.LoadRecordAsync();
    }

    public static async Task<bool> TryUpdateRecordAsync(int currentValue)
    {
        int? previousValue = await GameProgressSaver.LoadRecordAsync();

        if (previousValue.HasValue && currentValue <= previousValue.Value)
        {
            return false;
        }

        await GameProgressSaver.SaveRecordAsync(currentValue);
        return true;
    }
}
