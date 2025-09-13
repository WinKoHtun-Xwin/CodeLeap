using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeLeap.Core.Entities
{
    public abstract class BaseEntity
    {
    public required string Id { get; set; }
    public required string CreatedBy { get; set; }  
    public required string UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    }   
}
