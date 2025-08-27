using Application.Interfaces.Persistences;
using Application.Interfaces.Persistences.Repo;
using Domain.Entities.SharedAggregates;
using Shared.Common.Base;
using Shared.Models.DataModels.ClassificationDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Services
{
    public class SeatTypeService : ISeatTypeService
    {
        private readonly IRepository<SeatType> _seatTypeRepository;
        public SeatTypeService(IRepository<SeatType> seatTypeRepository)
        {
            _seatTypeRepository = seatTypeRepository;
        }
        public async Task<BaseResponse<SeatType>> CreateSeatTypeAsync(SeatTypeRequest request)
        {
            try
            {
                var seatType = new SeatType(request.TypeName, request.PriceMultiplier);
                await _seatTypeRepository.AddAsync(seatType);
                return BaseResponse<SeatType>.Success(seatType);
            }
            catch (Exception ex)
            {
                return BaseResponse<SeatType>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<object>> DeleteSeatTypeAsync(Guid seatTypeId)
        {
            try
            {
                var seatType = await _seatTypeRepository.GetByIdAsync(seatTypeId);
                if (seatType == null)
                {
                    return BaseResponse<object>.Failure(Error.NotFound("Seat type not found."));
                }
                await _seatTypeRepository.DeleteAsync(seatType);
                return BaseResponse<object>.Success();
            }
            catch (Exception ex)
            {
                return BaseResponse<object>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<SeatType>> GetSeatTypeByIdAsync(Guid seatTypeId)
        {
            try
            {
                var seatType = await _seatTypeRepository.GetByIdAsync(seatTypeId);
                if (seatType == null)
                {
                    return BaseResponse<SeatType>.Failure(Error.NotFound("Seat type not found."));
                }
                return BaseResponse<SeatType>.Success(seatType);
            }
            catch (Exception ex)
            {
                return BaseResponse<SeatType>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<IEnumerable<SeatType>>> GetSeatTypesAsync()
        {
            try
            {
                var seatType = await _seatTypeRepository.ListAsync();
                return BaseResponse<IEnumerable<SeatType>>.Success(seatType);
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<SeatType>>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<SeatType>> UpdateSeatTypeAsync(Guid seatTypeId, SeatTypeRequest request)
        {
            try
            {
                var seatType = await _seatTypeRepository.GetByIdAsync(seatTypeId);
                if (seatType == null)
                {
                    return BaseResponse<SeatType>.Failure(Error.NotFound("Seat type not found."));
                }
                seatType.UpdateSeatType(request.TypeName, request.PriceMultiplier);
                await _seatTypeRepository.UpdateAsync(seatType);
                return BaseResponse<SeatType>.Success(seatType);
            }
            catch (Exception ex)
            {
                return BaseResponse<SeatType>.Failure(Error.InternalServerError(ex.Message));
            }
        }
    }
}
