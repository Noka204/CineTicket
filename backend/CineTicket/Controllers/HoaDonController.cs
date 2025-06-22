//using AutoMapper;
//using CineTicket.DTOs.HoaDon;
//using CineTicket.Models;
//using CineTicket.Services.Interfaces;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace CineTicket.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class HoaDonController : ControllerBase
//    {
//        private readonly IHoaDonService _hoaDonService;
//        private readonly IMapper _mapper;

//        public HoaDonController(IHoaDonService service, IMapper mapper)
//        {
//            _hoaDonService = service;
//            _mapper = mapper;
//        }

//        [HttpGet("get-all")]
//        public async Task<IActionResult> GetAll()
//        {
//            var data = await _hoaDonService.GetAllAsync();
//            var mapped = _mapper.Map<IEnumerable<HoaDonDTO>>(data);
//            return Ok(new { status = true, message = "Lấy danh sách hóa đơn thành công", data = mapped });
//        }

//        [HttpGet("get/{id}")]
//        public async Task<IActionResult> GetById(int id)
//        {
//            var hd = await _hoaDonService.GetByIdAsync(id);
//            if (hd == null)
//                return NotFound(new { status = false, message = "Không tìm thấy hóa đơn" });

//            var mapped = _mapper.Map<HoaDonDTO>(hd);
//            return Ok(new { status = true, message = "Lấy hóa đơn thành công", data = mapped });
//        }

//        [HttpPost("create")]
//        //[Authorize(Roles = "Admin,Emloyee")]
//        public async Task<IActionResult> Create([FromBody] CreateHoaDonDTO request)
//        {
//            var model = _mapper.Map<HoaDon>(request);
//            var created = await _hoaDonService.CreateAsync(model);
//            return Ok(new { status = true, message = "Tạo hóa đơn thành công", data = created });
//        }

//        [HttpPut("update/{id}")]
//        //[Authorize(Roles = "Admin")]
//        public async Task<IActionResult> Update(int id, [FromBody] UpdateHoaDonDTO request)
//        {
//            var model = _mapper.Map<HoaDon>(request);
//            model.MaHd = id;
//            var success = await _hoaDonService.UpdateAsync(model);
//            return success
//                ? Ok(new { status = true, message = "Cập nhật thành công" })
//                : NotFound(new { status = false, message = "Không tìm thấy hóa đơn" });
//        }

//        [HttpDelete("delete/{id}")]
//        //[Authorize(Roles = "Admin")]
//        public async Task<IActionResult> Delete(int id)
//        {
//            var success = await _hoaDonService.DeleteAsync(id);
//            return success
//                ? Ok(new { status = true, message = "Xóa thành công" })
//                : NotFound(new { status = false, message = "Không tìm thấy hóa đơn" });
//        }
//    }
//}
