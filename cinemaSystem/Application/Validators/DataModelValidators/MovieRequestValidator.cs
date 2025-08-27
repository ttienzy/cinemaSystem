using FluentValidation;
using Shared.Models.DataModels.MovieDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators.DataModelValidators
{
    public class MovieRequestValidator : AbstractValidator<MovieRequest>
    {
        public MovieRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Tiêu đề là bắt buộc")
                .Length(1, 200).WithMessage("Tiêu đề phải từ 1-200 ký tự")
                .Must(NotContainSpecialCharacters).WithMessage("Tiêu đề không được chứa ký tự đặc biệt");

            RuleFor(x => x.DurationMinutes)
                .GreaterThan(0).WithMessage("Thời lượng phải lớn hơn 0")
                .LessThanOrEqualTo(600).WithMessage("Thời lượng không được vượt quá 600 phút");

            RuleFor(x => x.ReleaseDate)
                .NotEmpty().WithMessage("Ngày phát hành là bắt buộc")
                .Must(BeValidReleaseDate).WithMessage("Ngày phát hành không hợp lệ");

            RuleFor(x => x.PosterUrl)
                .NotEmpty().WithMessage("URL poster là bắt buộc")
                .Must(BeValidUrl).WithMessage("URL poster không hợp lệ");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Mô tả không được quá 1000 ký tự")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Trạng thái không hợp lệ. Chỉ chấp nhận các giá trị: ComingSoon, Showing, Ended.");
        }

        private bool NotContainSpecialCharacters(string title)
        {
            return !title.Any(c => "!@#$%^&*()".Contains(c));
        }

        private bool BeValidReleaseDate(DateTime date)
        {
            return date >= new DateTime(1900, 1, 1) && date <= DateTime.Now.AddYears(10);
        }

        private bool BeValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var result)
                   && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }
    }
}
