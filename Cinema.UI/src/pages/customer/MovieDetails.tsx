import React from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Button, Tag, Divider, Empty, Skeleton } from 'antd';
import { PlayCircleOutlined, ClockCircleOutlined, CalendarOutlined } from '@ant-design/icons';
import { movieApi } from '../../features/movies/api/movieApi';
import { showtimeApi, type Showtime } from '../../features/showtimes/api/showtimeApi';
import dayjs from '../../utils/dayjs';
import { toLocalDateTime } from '../../utils/dateTime';

const MovieDetails: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  // Lấy chi tiết phim
  const { data: movieRes, isLoading: isLoadingMovie } = useQuery({
    queryKey: ['movie', id],
    queryFn: () => movieApi.getMovieById(id!),
    enabled: !!id,
  });

  // Lấy danh sách lịch chiếu của phim
  const { data: showtimesRes, isLoading: isLoadingShowtimes } = useQuery({
    queryKey: ['showtimes', id],
    queryFn: () => showtimeApi.getShowtimesByMovie(id!),
    enabled: !!id,
  });

  if (isLoadingMovie || isLoadingShowtimes) {
    return (
      <div className="movie-details-container" style={{ padding: '80px 10%' }}>
        <Skeleton active avatar={{ shape: 'square', size: 300 }} paragraph={{ rows: 6 }} />
        <Divider />
        <Skeleton active title={false} paragraph={{ rows: 4 }} />
      </div>
    );
  }

  if (!movieRes?.data) return <Empty description="Không tìm thấy phim" style={{ marginTop: 100 }} />;

  const movie = movieRes.data;
  const showtimes = showtimesRes?.data || [];

  // Group suất chiếu theo ngày -> rạp -> phòng chiếu
  const groupedShowtimes = showtimes.reduce((acc, curr) => {
    // Convert UTC to local time
    const localStartTime = toLocalDateTime(curr.startTime);
    const dateKey = localStartTime.format('YYYY-MM-DD');
    const cinemaKey = curr.cinemaName;
    const hallKey = curr.cinemaHallName;

    if (!acc[dateKey]) acc[dateKey] = {};
    if (!acc[dateKey][cinemaKey]) acc[dateKey][cinemaKey] = {};
    if (!acc[dateKey][cinemaKey][hallKey]) acc[dateKey][cinemaKey][hallKey] = [];

    acc[dateKey][cinemaKey][hallKey].push(curr);
    return acc;
  }, {} as Record<string, Record<string, Record<string, Showtime[]>>>);

  // Sort dates
  const sortedDates = Object.keys(groupedShowtimes).sort();

  return (
    <div className="movie-details-container">
      {/* Hero Section */}
      <div className="details-hero">
        <div className="details-backdrop" style={{ backgroundImage: `url(${movie.posterUrl || 'https://via.placeholder.com/1200x500'})` }}>
          <div className="backdrop-overlay"></div>
        </div>

        <div className="details-content">
          <div className="details-poster">
            <img src={movie.posterUrl || 'https://via.placeholder.com/300x450'} alt={movie.title} />
            <Button type="primary" icon={<PlayCircleOutlined />} size="large" block className="trailer-btn">
              Xem Trailer
            </Button>
          </div>
          <div className="details-info">
            <h1 className="details-title">{movie.title}</h1>
            <div className="details-meta">
              <Tag color="gold">{movie.language || '2D Phụ đề'}</Tag>
              <span><ClockCircleOutlined /> {movie.duration} phút</span>
              <span><CalendarOutlined /> Khởi chiếu: {toLocalDateTime(movie.releaseDate).format('DD/MM/YYYY')}</span>
            </div>
            <div className="details-desc">
              <h3>Nội dung phim</h3>
              <p>{movie.description || 'Chưa có thông tin mô tả.'}</p>
            </div>
          </div>
        </div>
      </div>

      {/* Lịch chiếu */}
      <div className="showtimes-section">
        <h2>Lịch Chiếu</h2>
        <Divider />

        {sortedDates.length === 0 ? (
          <Empty description="Hiện chưa có suất chiếu nào cho phim này" />
        ) : (
          sortedDates.map(dateKey => {
            const dateShowtimes = groupedShowtimes[dateKey];
            const displayDate = dayjs(dateKey);
            const isToday = displayDate.isSame(dayjs(), 'day');
            const isTomorrow = displayDate.isSame(dayjs().add(1, 'day'), 'day');

            let dateLabel = displayDate.format('dddd, DD/MM/YYYY');
            if (isToday) dateLabel = `Hôm nay, ${displayDate.format('DD/MM/YYYY')}`;
            if (isTomorrow) dateLabel = `Ngày mai, ${displayDate.format('DD/MM/YYYY')}`;

            return (
              <div key={dateKey} className="date-group">
                <h3 className="showtime-date">
                  <CalendarOutlined /> {dateLabel}
                </h3>

                {Object.entries(dateShowtimes).map(([cinemaName, halls]) => (
                  <div key={cinemaName} className="cinema-group">
                    <h4 className="cinema-name">Rạp {cinemaName}</h4>

                    {Object.entries(halls).map(([hallName, times]) => (
                      <div key={hallName} className="hall-group">
                        <div className="hall-name">Phòng {hallName}</div>
                        <div className="showtime-tags">
                          {times.map(t => {
                            const localStart = toLocalDateTime(t.startTime);

                            return (
                              <Button
                                key={t.id}
                                className="showtime-btn"
                                onClick={() => navigate(`/booking/${t.id}`)}
                              >
                                {localStart.format('HH:mm')}
                              </Button>
                            );
                          })}
                        </div>
                      </div>
                    ))}
                  </div>
                ))}
              </div>
            );
          })
        )}
      </div>
    </div>
  );
};

export default MovieDetails;
