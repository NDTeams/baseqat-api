using Baseqat.EF.DATA;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Baseqt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseCategoryController : ControllerBase
    {
        private readonly IDataUnit _unitOfWork;

        public CourseCategoryController(IDataUnit unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


    }
}
