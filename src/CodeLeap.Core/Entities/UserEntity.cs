using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeLeap.Core.Entities
{
    public class UserEntity : BaseEntity
    {
        public required string password { get; set; }
        public required string Username { get; set; }

    }
}
