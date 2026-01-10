namespace RIMAPI.Models
{
    public class ImageDto
    {
        public string Result { get; set; }
        public string ImageBase64 { get; set; }
    }

    public class ImageUploadRequest
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public string Direction { get; set; }
        public string Offset { get; set; }
        public string Scale { get; set; }
        public string ThingType { get; set; }
        public string IsStackable { get; set; }
        public string MaskImage { get; set; }
        public int UpdateItemIndex { get; set; }
    }

    public class FactionIconImageDto
    {
        public ImageDto Image { get; set; }
        public string Color { get; set; }
    }
}
