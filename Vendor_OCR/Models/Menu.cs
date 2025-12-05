namespace Vendor_OCR.Models
{
    public class Menu
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Icon { get; set; }
        public int? ParentId { get; set; }
        public int? Role { get; set; }
        public int SortOrder { get; set; }
        public List<Menu> Children { get; set; } = new List<Menu>();
    }
}
