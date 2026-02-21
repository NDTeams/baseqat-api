using System;
using System.Collections.Generic;
using System.Text;

namespace Baseqat.EF.Models
{
    public class DeleteEntity : AuditableEntity
    {
        public bool? IsDeleted { get; set; }

        public string? DeletedBy { get; set; }

        public DateTime? DeletedAt { get; set; }
    }
}
