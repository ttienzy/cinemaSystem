using Ardalis.Specification;
using Domain.Entities.InventoryAggregate;
using Shared.Models.DataModels.InventoryDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Specifications.InventorySpec
{
    public class GetConcessionByCinemaIdSpecification : Specification<InventoryItem, InventoryResponse>
    {
        public GetConcessionByCinemaIdSpecification(Guid cinemaId)
        {
            Query.AsNoTracking()
                .Where(i => i.CinemaId == cinemaId)
                .Select(e => new InventoryResponse
                {
                    CurrentStock = e.CurrentStock,
                    Id = e.Id,
                    Image = e.ImageUrl,
                    ItemCategory = e.ItemCategory,
                    ItemName = e.ItemName,
                    UnitPrice = e.UnitPrice,
                });
        }
    }
}
