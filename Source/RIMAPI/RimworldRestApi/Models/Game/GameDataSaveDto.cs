namespace RIMAPI.Models
{
    public class GameLoadRequestDto
    {
        public string FileName { get; set; }
        public bool CheckVersion { get; set; }
        public bool SkipModMismatch { get; set; }
    }

    public class GameSaveRequestDto
    {
        public string FileName { get; set; }
    }
}
