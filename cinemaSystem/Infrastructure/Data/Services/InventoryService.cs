using Application.Interfaces.Persistences;
using Application.Interfaces.Persistences.Repo;
using Application.Specifications.InventorySpec;
using Domain.Entities.CinemaAggreagte;
using Domain.Entities.InventoryAggregate;
using Shared.Common.Base;
using Shared.Models.DataModels.InventoryDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IRepository<InventoryItem> _inventoryRepository;
        public InventoryService(IRepository<InventoryItem> repository)
        {
            _inventoryRepository = repository;
        }

        public async Task<BaseResponse<IEnumerable<InventoryResponse>>> AddInventoryAsync(IEnumerable<InventoryRequest> requests)
        {
            try
            {
                var listItems = requests.Select(e => new InventoryItem(e.CinemaId, e.ItemName, e.ItemCategory, e.CurrentStock, e.MinimumStock, e.UnitPrice, e.UnitPrice, e.ImageUrl));
                await _inventoryRepository.AddRangeAsync(listItems);
                var response = listItems.Select(e => new InventoryResponse
                {
                    Id = e.Id,
                    CurrentStock = e.CurrentStock,
                    Image = e.ImageUrl,
                    ItemCategory = e.ItemCategory,
                    ItemName = e.ItemName,
                    UnitPrice = e.UnitPrice,
                });
                return BaseResponse<IEnumerable<InventoryResponse>>.Success(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<InventoryResponse>>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<IEnumerable<InventoryResponse>>> GetConcessionsAsync(Guid cinemaId)
        {
            try
            {
                var results = new GetConcessionByCinemaIdSpecification(cinemaId);
                return BaseResponse<IEnumerable<InventoryResponse>>.Success(await _inventoryRepository.ListAsync(results));
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<InventoryResponse>>.Failure(Error.InternalServerError(ex.Message));
            }
        }
    }
}
