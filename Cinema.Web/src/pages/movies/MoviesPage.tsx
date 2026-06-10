import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Button, Card, Col, Empty, Image, Input, Row, Select, Space, Tag } from 'antd';
import { SearchOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { movieApi } from '../../features/movies/movieApi';
import { formatDate } from '../../shared/utils/format';

export default function MoviesPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState<string | undefined>();

  const moviesQuery = useQuery({
    queryKey: ['customer-movies'],
    queryFn: () => movieApi.getMovies(1, 100),
  });

  const movies = useMemo(() => {
    const source = moviesQuery.data?.data.items ?? [];
    return source.filter((movie) => {
      const matchSearch = movie.title.toLowerCase().includes(search.trim().toLowerCase());
      const matchStatus = !status || movie.status === status;
      return matchSearch && matchStatus;
    });
  }, [moviesQuery.data?.data.items, search, status]);

  return (
    <main className="page-shell">
      <Space direction="vertical" size={20} style={{ width: '100%' }}>
        <div>
          <h1 className="page-title">Movies</h1>
          <div className="page-subtitle">Browse movies and select a showtime.</div>
        </div>

        <Card>
          <Space wrap style={{ marginBottom: 18 }}>
            <Input
              allowClear
              prefix={<SearchOutlined />}
              placeholder="Search movie"
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              style={{ width: 260 }}
            />
            <Select
              allowClear
              placeholder="Status"
              value={status}
              onChange={setStatus}
              options={[
                { label: 'Showing', value: 'Showing' },
                { label: 'Coming soon', value: 'ComingSoon' },
                { label: 'Archived', value: 'Archived' },
              ]}
              style={{ width: 180 }}
            />
          </Space>

          {movies.length === 0 && !moviesQuery.isLoading ? (
            <Empty />
          ) : (
            <Row gutter={[18, 18]}>
              {movies.map((movie) => (
                <Col xs={24} sm={12} md={8} lg={6} key={movie.id}>
                  <Card
                    hoverable
                    loading={moviesQuery.isLoading}
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
                      <Tag>{movie.status}</Tag>
                      <span>{movie.duration} min</span>
                      <span>{formatDate(movie.releaseDate)}</span>
                      <Button type="primary" block>
                        Showtimes
                      </Button>
                    </Space>
                  </Card>
                </Col>
              ))}
            </Row>
          )}
        </Card>
      </Space>
    </main>
  );
}
