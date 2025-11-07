namespace CineTicket.DTOs.KhuyenMai
{

    public class KhuyenMaiCodeCreateDto
    {
        public int KhuyenMaiId { get; set; }
        public string? Code { get; set; }          // nếu null và Count>0 thì auto-generate
        public int? Count { get; set; }            // số lượng generate
        public string? Prefix { get; set; }        // tiền tố (vd: CINE)
        public string? AssignedToUserId { get; set; } // mã đích danh (optional)
    }
}