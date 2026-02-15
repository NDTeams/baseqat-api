using Microsoft.AspNetCore.Mvc;

namespace Baseqt.API.Helper
{
    public class isAllowedAttribute : TypeFilterAttribute
    {
        public isAllowedAttribute(string privlige, string permession/*, params string[] roles*/)
            : base(typeof(isAllowedFilter))
        {
            Arguments = new object[] { privlige, permession/*, roles*/ };
        }
    }
}
