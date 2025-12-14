using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CodeLeap.Core.Entities
{
    public class ProductEntity : BaseEntity
    {
        [CodeLeap.Core.Attributes.Searchable]
        public required string Name { get; set; }
        public required decimal Price { get; set; }
        [CodeLeap.Core.Attributes.Searchable]
        public required string Description { get; set; }
        public required int Stock { get; set; }

        public required string ImageUrl { get; set; }
    }
}
