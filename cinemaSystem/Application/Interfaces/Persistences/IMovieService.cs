using Shared.Common.Base;
using Shared.Common.Paging;
using Shared.Models.DataModels.CinemaDtos;
using Shared.Models.DataModels.MovieDtos;
using Shared.Models.DataModels.StatisticDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Persistences
{
    public interface IMovieService
    {
        Task<BaseResponse<IEnumerable<MovieSectionResponse>>> GetMoviesSectionAsync();
        Task<BaseResponse<PaginatedList<MovieResponse>>> GetMoviesAsync(MovieQueryParameters parameters);
        Task<BaseResponse<MovieDetailsResponse>> GetMovieByIdAsync(Guid movieId);
        Task<BaseResponse<IEnumerable<MovieComingSoonResponse>>> GetMovieComingSoonAsync();
        Task<BaseResponse<HighlightStat>> GetHighlightStatAsync();
        Task<BaseResponse<IEnumerable<MovieResponse>>> GetMovieListAsync();
        Task<BaseResponse<MovieDetailsResponse>> CreateMovieAsync(MovieRequest request);
        Task<BaseResponse<IEnumerable<MovieCastCrewResponse>>> AddCastCrewToMovie(Guid movieId, IEnumerable<MovieCastCrewRequest> requests);
        Task<BaseResponse<IEnumerable<MovieCertificationResponse>>> AddCertificationsToMovie(Guid movieId, IEnumerable<MovieCertificationRequest> requests);
        Task<BaseResponse<IEnumerable<MovieCopyrightResponse>>> AddCopyrightsToMovie(Guid movieId, IEnumerable<MovieCopyrightRequest> requests);
        Task<BaseResponse<IEnumerable<MovieGenreResponse>>> AddGenreToMovie(Guid movieId, IEnumerable<MovieGenreRequest> requests);

        Task<BaseResponse<MovieDetailsResponse>> UpdateMovieAsync(Guid movieId, MovieRequest request);
        Task<BaseResponse<MovieCastCrewResponse>> UpdateCastCrewForMovie(Guid movieId, Guid castCrewId, MovieCastCrewRequest request);
        Task<BaseResponse<MovieCertificationResponse>> UpdateCertificationsForMovie(Guid movieId, Guid certId, MovieCertificationRequest request);
        Task<BaseResponse<MovieCopyrightResponse>> UpdateCopyrightsForMovie(Guid movieId, Guid copyrightId, MovieCopyrightRequest request);
        Task<BaseResponse<MovieGenreResponse>> UpdateGenreForMovie(Guid movieId, Guid mGenreId, MovieGenreRequest request);

        Task<BaseResponse<object>> DeleteMovieAsync(Guid movieId);
        Task<BaseResponse<object>> DeleteCastCrewForMovie(Guid movieId, IEnumerable<Guid> castCrewIds);
        Task<BaseResponse<object>> DeleteCertificationsForMovie(Guid movieId, IEnumerable<Guid> certificationIds);
        Task<BaseResponse<object>> DeleteCopyrightsForMovie(Guid movieId, IEnumerable<Guid> copyrightIds);
        Task<BaseResponse<object>> DeleteGenreForMovie(Guid movieId, IEnumerable<Guid> genreIds);
    }
}
