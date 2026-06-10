import { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Button, Card, Col, Empty, Image, Row, Skeleton, Space, Tag } from 'antd';
import { CalendarOutlined, ClockCircleOutlined, EnvironmentOutlined } from '@ant-design/icons';
import { useNavigate, useParams } from 'react-router-dom';
import dayjs from 'dayjs';
import { movieApi } from '../../features/movies/movieApi';
import { showtimeApi, type Showtime } from '../../features/showtimes/showtimeApi';
import { formatDate, formatMoney } from '../../shared/utils/format';

type GroupedShowtimes = Record<string, Record<string, Record<string, Showtime[]>>>;

function groupShowtimes(showtimes: Showtime[]): GroupedShowtimes {
  return showtimes.reduce<GroupedShowtimes>((acc, showtime) => {
    const dateKey = dayjs(showtime.startTime).format('YYYY-MM-DD');
    const cinemaName = showtime.cinemaName || 'Cinema';
    const hallName = showtime.cinemaHallName || 'Hall';

    acc[dateKey] ??= {};
    acc[dateKey][cinemaName] ??= {};
    acc[dateKey][cinemaName][hallName] ??= [];
    acc[dateKey][cinemaName][hallName].push(showtime);
    return acc;
  }, {});
}

export default function MovieDetailPage() {
  const { movieId } = useParams<{ movieId: string }>();
  const navigate = useNavigate();

  const movieQuery = useQuery({
    queryKey: ['customer-movie', movieId],
    queryFn: () => movieApi.getMovieById(movieId!),
    enabled: !!movieId,
  });

  const showtimesQuery = useQuery({
    queryKey: ['customer-showtimes', movieId],
    queryFn: () => showtimeApi.getShowtimesByMovie(movieId!),
    enabled: !!movieId,
  });

  const movie = movieQuery.data?.data;
  const showtimes = (showtimesQuery.data?.data ?? []).filter((showtime) => dayjs(showtime.startTime).isAfter(dayjs()));
  const grouped = useMemo(() => groupShowtimes(showtimes), [showtimes]);
  const dateKeys = Object.keys(grouped).sort();

  if (movieQuery.isLoading) {
    return (
      <main className="page-shell">
        <Skeleton active paragraph={{ rows: 8 }} />
      </main>
    );
  }

  if (!movie) {
    return (
      <main className="page-shell">
        <Empty description="Movie not found" />
      </main>
    );
  }

  return (
    <main>
      <section className="movie-detail-hero">
        <Row gutter={[28, 28]} align="middle">
          <Col xs={24} md={7}>
            <div className="detail-poster">
              {movie.posterUrl ? <Image src={movie.posterUrl} alt={movie.title} preview={false} /> : <div>No poster</div>}
            </div>
          </Col>
          <Col xs={24} md={17}>
            <Space direction="vertical" size={14}>
              <h1>{movie.title}</h1>
              <Space wrap>
                <Tag icon={<ClockCircleOutlined />}>{movie.duration} min</Tag>
                <Tag icon={<CalendarOutlined />}>{formatDate(movie.releaseDate)}</Tag>
                {movie.language && <Tag>{movie.language}</Tag>}
              </Space>
              <p>{movie.description || 'No description available.'}</p>
            </Space>
          </Col>
        </Row>
      </section>

      <section className="content-section">
        <h2>Showtimes</h2>
        {dateKeys.length === 0 ? (
          <Card>
            <Empty description="No upcoming showtimes" />
          </Card>
        ) : (
          <Space direction="vertical" size={16} style={{ width: '100%' }}>
            {dateKeys.map((dateKey) => (
              <Card key={dateKey} title={dayjs(dateKey).format('dddd, DD/MM/YYYY')}>
                <Space direction="vertical" size={14} style={{ width: '100%' }}>
                  {Object.entries(grouped[dateKey]).map(([cinemaName, halls]) => (
                    <div key={cinemaName}>
                      <h3>
                        <EnvironmentOutlined /> {cinemaName}
                      </h3>
                      {Object.entries(halls).map(([hallName, items]) => (
                        <div key={hallName} className="showtime-group">
                          <span>{hallName}</span>
                          <Space wrap>
                            {items
                              .sort((left, right) => dayjs(left.startTime).valueOf() - dayjs(right.startTime).valueOf())
                              .map((showtime) => (
                                <Button key={showtime.id} onClick={() => navigate(`/booking/${showtime.id}`)}>
                                  {dayjs(showtime.startTime).format('HH:mm')} - {formatMoney(showtime.price)}
                                </Button>
                              ))}
                          </Space>
                        </div>
                      ))}
                    </div>
                  ))}
                </Space>
              </Card>
            ))}
          </Space>
        )}
      </section>
    </main>
  );
}
