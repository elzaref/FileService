namespace FileService.Model
{
    public class FileMetadata
    {
        public Guid Id { get; set; }
        public string Filename { get; set; }
        public string FilePath { get; set; }
        public string ContentType { get; set; }
        public long Size { get; set; }
    }
}
