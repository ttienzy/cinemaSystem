using Domain.Common;
using Domain.Entities.MovieAggregate.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.MovieAggregate
{
    public class Movie : BaseEntity, IAggregateRoot
    {
        public string Title { get; private set; }
        public int DurationMinutes { get; private set; }
        public RatingStatus Rating { get; private set; }
        public string Trailer { get; private set; }
        public DateTime ReleaseDate { get; private set; }
        public string Description { get; private set; }
        public string PosterUrl { get; private set; }
        public MovieStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }


        private readonly List<MovieCopyright> _copyrights;
        public IReadOnlyCollection<MovieCopyright> Copyrights => _copyrights.AsReadOnly();

        private readonly List<MovieCertification> _certifications;
        public IReadOnlyCollection<MovieCertification> Certifications => _certifications.AsReadOnly();

        private readonly List<MovieCastCrew> _castCrew;
        public IReadOnlyCollection<MovieCastCrew> CastCrew => _castCrew.AsReadOnly();

        private readonly List<MovieGenre> _movieGenres;
        public IReadOnlyCollection<MovieGenre> MovieGenres => _movieGenres.AsReadOnly();
        public Movie()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            _copyrights = new List<MovieCopyright>();
            _certifications = new List<MovieCertification>();
            _castCrew = new List<MovieCastCrew>();
            _movieGenres = new List<MovieGenre>();
        }

        public Movie(string title, int durationMinutes, DateTime releaseDate, MovieStatus movieStatus, string description, RatingStatus rating, string posterUrl, string trailer = "N/A")
        {
            Title = title;
            DurationMinutes = durationMinutes;
            ReleaseDate = releaseDate;
            Description = description;
            PosterUrl = posterUrl;
            Status = movieStatus;
            Rating = rating;
            Trailer = trailer;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            _copyrights = new List<MovieCopyright>();
            _certifications = new List<MovieCertification>();
            _castCrew = new List<MovieCastCrew>();
            _movieGenres = new List<MovieGenre>();
        }
        
        public void UpdateDetail(string title, int durationMinutes, DateTime releaseDate, string description, string posterUrl, RatingStatus rating, string trailer)
        {
            Title = title;
            DurationMinutes = durationMinutes;
            ReleaseDate = releaseDate;
            Description = description;
            PosterUrl = posterUrl;
            UpdatedAt = DateTime.UtcNow;
            Rating = rating;
            Trailer = trailer;
        }
        ///------------CastCrew--------------------------------------
        public MovieCastCrew? GetCastCrewById(Guid castCrewId)
        {
            return _castCrew.FirstOrDefault(c => c.Id == castCrewId);
        }
        public void AddCastCrew(string personName, string roleType)
        {
            var castCrew = new MovieCastCrew(this.Id, personName, roleType);
            _castCrew.Add(castCrew);
            //UpdatedAt = DateTime.UtcNow;
        }

        public void AddRangeCastCrew(List<MovieCastCrew> castCrews)
        {
            _castCrew.AddRange(castCrews);
            UpdatedAt = DateTime.UtcNow;
        }
        public bool UpdateCastCrew(Guid castCrewId,Guid movieId, string personName, string roleType)
        {
            var getCastCrew = GetCastCrewById(castCrewId);
            if (getCastCrew is null)
                return false;
            getCastCrew.UpdateDetails(movieId, personName, roleType);
            return true;
        }
        public void RemoveRangeCastCrew()
        {
            _castCrew.Clear();
            UpdatedAt = DateTime.UtcNow;
        }
        ///---------------Copyrights-------------------------------------
        public MovieCopyright? GetCopyrightById(Guid crId)
        {
            return _copyrights.FirstOrDefault(x => x.Id == crId);
        }
        public void AddRangeCopyrights(List<MovieCopyright> copyrights)
        {
            _copyrights.AddRange(copyrights);
            UpdatedAt = DateTime.UtcNow;
        }
        public void RemoveRangeCopyrights()
        {
            _copyrights.Clear();
            UpdatedAt = DateTime.UtcNow;
        }
        public bool UpdateCopyright(Guid crId, string distributorCompany, DateTime licenseStartDate, DateTime licenseEndDate, MovieCopyrightStatus status)
        {
            var copyRight = GetCopyrightById(crId);
            if (copyRight is null) return false;
            copyRight.UpdateDetail(distributorCompany, licenseStartDate, licenseEndDate, status);
            return true;
        }
        //-------------------Certifications------------------------
        public MovieCertification? GetCertificationById(Guid certId)
        {
            return _certifications.FirstOrDefault(x => x.Id == certId);
        }
        public void AddRangeCertifications(List<MovieCertification> certifications)
        {
            _certifications.AddRange(certifications);
            UpdatedAt = DateTime.UtcNow;
        }
        public bool UpdateCertification(Guid certId, string certBody, string rating, DateTime issueDate)
        {
            var cert = GetCertificationById(certId);
            if (cert == null) return false;
            cert.UpdateDetail(certBody, rating, issueDate);
            return true;
        }
        public void RemoveRangeCertifications()
        {
            _certifications.Clear();
            UpdatedAt = DateTime.UtcNow;
        }
        //------------------------Genres----------------------------------------
        public MovieGenre? GetMovieGenreById(Guid movieGenreId)
        {
            return _movieGenres.FirstOrDefault(x =>x.Id == movieGenreId);   
        }
        public void AddRangeGenres(List<MovieGenre> genres)
        {
            _movieGenres.AddRange(genres);
            UpdatedAt = DateTime.UtcNow;
        }
        public void RemoveRangeGenres()
        {
            _movieGenres.Clear();
            UpdatedAt = DateTime.UtcNow;
        }
        public bool UpdateMovieGenre(Guid mdId, Guid genreId)
        {
            var mg = GetMovieGenreById(mdId);
            if (mg == null) return false;
            mg.UpdateDetail(genreId);
            return true;
        }
    }
}
