using Ardalis.Specification;
using Domain.Entities.InventoryAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Specifications.InventorySpec
{
    public class ListInventoriesSpecification : Specification<InventoryItem>
    {
        public ListInventoriesSpecification(List<Guid> inventoryIds)
        {
            Query.AsNoTracking()
                .Where(i => inventoryIds.Contains(i.Id));
        }
    }
}
