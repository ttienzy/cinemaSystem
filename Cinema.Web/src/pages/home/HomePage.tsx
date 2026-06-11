import { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Button, Card, Col, Empty, Image, Row, Skeleton, Space, Tabs, Tag } from 'antd';
import { CalendarOutlined, ClockCircleOutlined, PlayCircleOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import dayjs from 'dayjs';
import { movieApi, type Movie } from '../../features/movies/movieApi';
import { formatDate } from '../../shared/utils/format';

function isShowing(movie: Movie): boolean {
  return dayjs(movie.releaseDate).isBefore(dayjs()) || dayjs(movie.releaseDate).isSame(dayjs(), 'day');
}

function MovieGrid({ movies }: { movies: Movie[] }) {
  const navigate = useNavigate();

  if (movies.length === 0) return <Empty description="No movies found" />;

  return (
    <Row gutter={[18, 18]}>
      {movies.map((movie) => (
        <Col xs={24} sm={12} md={8} lg={6} key={movie.id}>
          <Card
            hoverable
            className="movie-card"
            cover={
              <div className="poster-frame">
                {movie.posterUrl ? (
                  <Image src={movie.posterUrl} alt={movie.title} preview={false} />
                ) : (
                  <div className="poster-empty">No poster</div>
                )}
              </div>
            }
            onClick={() => navigate(`/movies/${movie.id}`)}
          >
            <Space direction="vertical" size={8} style={{ width: '100%' }}>
              <strong className="movie-title">{movie.title}</strong>
              <Space wrap size={8}>
                <Tag icon={<ClockCircleOutlined />}>{movie.duration} min</Tag>
                <Tag icon={<CalendarOutlined />}>{formatDate(movie.releaseDate)}</Tag>
              </Space>
              <Button type="primary" block>
                View showtimes
              </Button>
            </Space>
          </Card>
        </Col>
      ))}
    </Row>
  );
}

export default function HomePage() {
  const navigate = useNavigate();
  const moviesQuery = useQuery({
    queryKey: ['customer-movies'],
    queryFn: () => movieApi.getMovies(1, 100),
  });

  const movies = moviesQuery.data?.data.items ?? [];
  const showingMovies = useMemo(() => movies.filter(isShowing), [movies]);
  const comingSoonMovies = useMemo(() => movies.filter((movie) => !isShowing(movie)), [movies]);
  const featuredMovie = showingMovies[0] ?? movies[0];

  if (moviesQuery.isLoading) {
    return (
      <main className="page-shell">
        <Skeleton active paragraph={{ rows: 8 }} />
      </main>
    );
  }

  return (
    <main className="home-shell">
      <section
        className="home-hero"
        style={featuredMovie?.posterUrl ? { backgroundImage: `linear-gradient(90deg, rgba(16, 24, 40, 0.94), rgba(16, 24, 40, 0.48)), url(${featuredMovie.posterUrl})` } : undefined}
      >
        <div>
          <h1>{featuredMovie?.title ?? 'Cinema Web'}</h1>
          <p>{featuredMovie?.description ?? 'Find movies, choose seats, and book tickets online.'}</p>
          <Space>
            <Button type="primary" size="large" icon={<PlayCircleOutlined />} onClick={() => featuredMovie && navigate(`/movies/${featuredMovie.id}`)}>
              Book now
            </Button>
            <Button size="large" onClick={() => navigate('/movies')}>
              Browse movies
            </Button>
          </Space>
        </div>
      </section>

      <section className="content-section">
        <Tabs
          defaultActiveKey="showing"
          items={[
            {
              key: 'showing',
              label: 'Now showing',
              children: <MovieGrid movies={showingMovies} />,
            },
            {
              key: 'coming',
              label: 'Coming soon',
              children: <MovieGrid movies={comingSoonMovies} />,
            },
          ]}
        />
      </section>
    </main>
  );
}
