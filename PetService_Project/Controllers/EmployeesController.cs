using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetService_Project.Models;
using PetService_Project_Api.DTO.WalkDTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetService_Project_Api.Controllers
{
    [ApiController]
    [Route("api/employees")]
    public class EmployeesController : ControllerBase
    {
        private readonly dbPetService_ProjectContext _context;

        // 定義寵物類型和尺寸的對應關係
        private static readonly Dictionary<string, int> _petTypeMapping = new Dictionary<string, int>
        {
            { "Dog", 1 },
            { "Cat", 2 },
            { "rabbit", 3 },
          
        };

        private static readonly Dictionary<string, int> _petSizeMapping = new Dictionary<string, int>
        {
            { "Small", 1 },
            { "Medium", 2 },
            { "Large", 3 }
           
        };

        // 反向映射，用於在 Select 中將整數轉換回字串
        private static readonly Dictionary<int, string> _petTypeReverseMapping = _petTypeMapping.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
        private static readonly Dictionary<int, string> _petSizeReverseMapping = _petSizeMapping.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        public EmployeesController(dbPetService_ProjectContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeListResponseDTO>>> GetEmployees([FromQuery] EmployeeListRequestDTO request)
        {
            var query = _context.TEmployeeServices
                .Include(es => es.FEmployee)
                .Include(es => es.FDistrict)
                .Where(es => !es.FIsDelete);

            if (!string.IsNullOrEmpty(request.District))
            {
                query = query.Where(es => es.FDistrict.FDistrictName == request.District);
            }

            if (!string.IsNullOrEmpty(request.PetType) && _petTypeMapping.TryGetValue(request.PetType, out int petTypeValue))
            {
                query = query.Where(es => es.FAcceptPetType == petTypeValue);
            }

            if (!string.IsNullOrEmpty(request.PetSize) && _petSizeMapping.TryGetValue(request.PetSize, out int petSizeValue))
            {
                query = query.Where(es => es.FPetSize == petSizeValue);
            }

            if (request.Price.HasValue)
            {
                query = query.Where(es => es.FPrice.HasValue && es.FPrice <= request.Price.Value);
            }

            var employeesList = await query.Select(es => new EmployeeListResponseDTO
            {
                EmployeeId = es.FEmployeeId,
                Name = es.FEmployee.FName,
                District = es.FDistrict.FDistrictName,
                PetType = _petTypeReverseMapping.GetValueOrDefault(es.FAcceptPetType), // 使用反向映射
                PetSize = _petSizeReverseMapping.GetValueOrDefault(es.FPetSize),     // 使用反向映射
                Price = (int)(es.FPrice.HasValue ? es.FPrice.Value : 0),
                EmployeeImage = es.FEmployee.FImage
            }).ToListAsync();

            return Ok(employeesList);
        }
    }
}