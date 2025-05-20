using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetService_Project.Models;
using PetService_Project_Api.DTO.WalkDTOs;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace PetService_Project_Api.Controllers
{
    [ApiController]
    [Route("api/employees")]
    public class EmployeesController : ControllerBase
    {
        private readonly dbPetService_ProjectContext _context;

        // 將字串轉換為數字（存入資料庫用）
        private static readonly Dictionary<string, int> _petTypeMapping = new Dictionary<string, int>
        {
            { "狗", 1 },
            { "貓", 2 },
            { "兔子", 4 },
        };

        private static readonly Dictionary<string, int> _petSizeMapping = new Dictionary<string, int>
        {
            { "小型", 1 },
            { "中型", 2 },
            { "大型", 4 }
        };

        // 將資料庫中的數值轉回字串（顯示在畫面上）
        private static readonly Dictionary<int, string> _petTypeReverseMapping = _petTypeMapping.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
        private static readonly Dictionary<int, string> _petSizeReverseMapping = _petSizeMapping.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        public EmployeesController(dbPetService_ProjectContext context)
        {
            _context = context;
        }
        //轉換 bitmask 成多選字串
        private List<string> ConvertPetTypeBitmaskToList(int bitmask)
        {
            var petTypes = new List<string>();
            foreach (var kv in _petTypeReverseMapping)
            {
                if ((bitmask & kv.Key) != 0)
                {
                    petTypes.Add(kv.Value);
                }
            }
            return petTypes;
        }
        private List<string> ConvertPetSizeBitmaskToList(int bitmask)
        {
            var sizes = new List<string>();
            foreach (var kv in _petSizeReverseMapping)
            {
                if ((bitmask & kv.Key) != 0)
                {
                    sizes.Add(kv.Value);
                }
            }
            return sizes;
        }


        // 取得所有員工資料
        [HttpGet]
        public async Task<IActionResult> GetEmployees()
        {
            // 先撈原始資料（int 型別）
            var employeesRawList = await _context.TEmployeeServices
                .Include(es => es.FEmployee)
                .Include(es => es.FDistrict)
                .Where(es => !es.FIsDelete)
                .Select(es => new
                {
                    es.FEmployeeId,
                    es.FEmployee.FName,
                    es.FDistrict.FDistrictName,
                    es.FAcceptPetType,
                    es.FPrice,
                    es.FEmployee.FImage
                }).ToListAsync();

            // 回傳轉換後的 DTO（字串顯示）
            var employeesList = employeesRawList.Select(es => new EmployeeListResponseDTO
            {
                EmployeeId = es.FEmployeeId,
                Name = es.FName,
                District = es.FDistrictName,
                PetTypes = ConvertPetTypeBitmaskToList(es.FAcceptPetType),
                Price = (int)(es.FPrice ?? 0),
                EmployeeImage = es.FImage
            }).ToList();

            return Ok(employeesList);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployeeDetail(int id)
        {
            // 先找 tEmployee_Service 主體
            var employeeService = await _context.TEmployeeServices
                .Include(es => es.FEmployee)
                .Include(es => es.FDistrict)
                .FirstOrDefaultAsync(es => es.FEmployeeId == id && !es.FIsDelete);

            if (employeeService == null)
            {
                return NotFound();
            }

            // 撈出多張工作照（tEmployee_Photo）
            var photoList = await _context.TEmployeePhotos
                .Where(p => p.FEmployeeId == id)
                .Select(p => p.FImagepath)
                .ToListAsync();

            //經緯度變數分離，方便閱讀
            var lat = (double)(employeeService.FLatitude ?? 0);
            var lng = (double)(employeeService.FLongitude ?? 0);

            var dto = new EmployeeDetailResponseDTO
            {
                Id = id,
                Name = employeeService.FEmployee.FName,
                District = employeeService.FDistrict.FDistrictName,
                Price = (int)(employeeService.FPrice ?? 0),

                PetTypes = ConvertPetTypeBitmaskToList(employeeService.FAcceptPetType),
                PetSizes = ConvertPetSizeBitmaskToList(employeeService.FPetSize),

                Distance = employeeService.FDistance ?? 0,
                DescriptionShort = employeeService.FDescriptionShort ?? "",
                Description = employeeService.FDescription ?? "",
                Carousel = photoList, // 多張圖片
                EmployeeImage = employeeService.FEmployee.FImage ?? "",
                Latitude = lat,
                Longitude = lng,
                Map = $"https://www.google.com/maps?q={lat},{lng}&hl=zh-TW&z=15&output=embed"
            };

            return Ok(dto);
        }


        // 篩選員工（地區、寵物類型）
        [HttpPost("filter")]
        public async Task<IActionResult> FilterEmployees([FromBody] EmployeeListRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var query = _context.TEmployeeServices
                .Include(es => es.FEmployee)
                .Include(es => es.FDistrict)
                .Where(es => !es.FIsDelete);

            // 篩選：地區
            if (!string.IsNullOrEmpty(request.District))
            {
                query = query.Where(es => es.FDistrict.FDistrictName == request.District);
            }

            // 篩選：寵物類型
            if (request.PetTypes != null && request.PetTypes.Any())
            {
                int petTypeBitmask = 0;
                foreach (var type in request.PetTypes)
                {
                    if (_petTypeMapping.TryGetValue(type, out int val))
            {
                        petTypeBitmask |= val;
                    }
            }

                query = query.Where(es => (es.FAcceptPetType & petTypeBitmask) != 0);
            }

            // 撈資料（原始 int 型別）
            var filteredRawList = await query.Select(es => new
            {
                es.FEmployeeId,
                es.FEmployee.FName,
                es.FDistrict.FDistrictName,
                es.FAcceptPetType,
                es.FPrice,
                es.FEmployee.FImage
            }).ToListAsync();

            // 轉為回傳 DTO（轉成字串）
            var filteredEmployeesList = filteredRawList.Select(es => new EmployeeListResponseDTO
            {
                EmployeeId = es.FEmployeeId,
                Name = es.FName,
                District = es.FDistrictName,
                PetTypes = ConvertPetTypeBitmaskToList(es.FAcceptPetType),
                Price = (int)(es.FPrice ?? 0),
                EmployeeImage = es.FImage
            }).ToList();

            return Ok(filteredEmployeesList);
        }

    }
}