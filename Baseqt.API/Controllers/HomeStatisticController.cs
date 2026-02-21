using Baseqat.CORE.DTOs;
using Baseqat.CORE.Response;
using Baseqat.EF.Consts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Baseqt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class HomeStatisticController : ControllerBase
    {
        private static readonly List<HomeStatisticDto> _statistics = new()
        {
            new HomeStatisticDto { Title = "عميل موثوق", Number = "350+" },
            new HomeStatisticDto { Title = "ورش وجلسات شهرية", Number = "28" },
            new HomeStatisticDto { Title = "خبير وشريك", Number = "42" },
            new HomeStatisticDto { Title = "معدل الرضا", Number = "98%" },
            new HomeStatisticDto { Title = "دولة عملنا بها", Number = "15" }
        };

        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            var result = _statistics
                .Select(x => new HomeStatisticDto { Title = x.Title, Number = x.Number })
                .ToList();

            return Ok(ApiBaseResponse<List<HomeStatisticDto>>.Success(result, ResponseMessages.DataRetrieved));
        }

        [HttpGet("GetByTitle/{title}")]
        public IActionResult GetByTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return BadRequest(ApiBaseResponse<string>.Fail(ResponseMessages.InvalidData));

            var item = _statistics.FirstOrDefault(x =>
                x.Title.Equals(title.Trim(), StringComparison.OrdinalIgnoreCase));

            if (item == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var dto = new HomeStatisticDto { Title = item.Title, Number = item.Number };
            return Ok(ApiBaseResponse<HomeStatisticDto>.Success(dto, ResponseMessages.DataRetrieved));
        }

        [HttpPost("Add")]
        public IActionResult Add([FromBody] HomeStatisticDto model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Title) || string.IsNullOrWhiteSpace(model.Number))
                return BadRequest(ApiBaseResponse<string>.Fail(ResponseMessages.InvalidData));

            var exists = _statistics.Any(x =>
                x.Title.Equals(model.Title.Trim(), StringComparison.OrdinalIgnoreCase));

            if (exists)
                return BadRequest(ApiBaseResponse<string>.Fail("العنوان موجود مسبقًا"));

            var entity = new HomeStatisticDto
            {
                Title = model.Title.Trim(),
                Number = model.Number.Trim()
            };

            _statistics.Add(entity);

            return Ok(ApiBaseResponse<HomeStatisticDto>.Success(entity, ResponseMessages.DataSaved));
        }

        [HttpPut("Update/{title}")]
        public IActionResult Update(string title, [FromBody] HomeStatisticDto model)
        {
            if (string.IsNullOrWhiteSpace(title) || model == null)
                return BadRequest(ApiBaseResponse<string>.Fail(ResponseMessages.InvalidData));

            var entity = _statistics.FirstOrDefault(x =>
                x.Title.Equals(title.Trim(), StringComparison.OrdinalIgnoreCase));

            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            if (!string.IsNullOrWhiteSpace(model.Title))
                entity.Title = model.Title.Trim();

            if (!string.IsNullOrWhiteSpace(model.Number))
                entity.Number = model.Number.Trim();

            return Ok(ApiBaseResponse<HomeStatisticDto>.Success(entity, ResponseMessages.DataUpdated));
        }

        [HttpDelete("Delete/{title}")]
        public IActionResult Delete(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return BadRequest(ApiBaseResponse<string>.Fail(ResponseMessages.InvalidData));

            var entity = _statistics.FirstOrDefault(x =>
                x.Title.Equals(title.Trim(), StringComparison.OrdinalIgnoreCase));

            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            _statistics.Remove(entity);

            return Ok(ApiBaseResponse<string>.Success(string.Empty, ResponseMessages.DataDeleted));
        }
    }
}
