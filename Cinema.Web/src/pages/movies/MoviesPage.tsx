import { useEffect, useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Button, Card, Col, Empty, Image, Input, Row, Select, Space, Tag } from 'antd';
import { SearchOutlined } from '@ant-design/icons';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { movieApi } from '../../features/movies/movieApi';
import { formatDate } from '../../shared/utils/format';

const searchSuggestions = [
  'space adventure',
  'family drama',
  'light horror',
  'animated weekend',
  'emotional romance',
];

export default function MoviesPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [search, setSearch] = useState(() => searchParams.get('query') ?? '');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [status, setStatus] = useState<string | undefined>();

  useEffect(() => {
    setSearch(searchParams.get('query') ?? '');
  }, [searchParams]);

  useEffect(() => {
    const timeout = window.setTimeout(() => {
      setDebouncedSearch(search.trim());
    }, 350);

    return () => window.clearTimeout(timeout);
  }, [search]);

  const moviesQuery = useQuery({
    queryKey: ['customer-movies'],
    queryFn: () => movieApi.getMovies(1, 100),
    enabled: debouncedSearch.length === 0,
  });

  const searchQuery = useQuery({
    queryKey: ['customer-movie-search', debouncedSearch],
    queryFn: () => movieApi.searchMovies(debouncedSearch, 1, 100),
    enabled: debouncedSearch.length > 0,
  });

  const movies = useMemo(() => {
    const source = debouncedSearch
      ? (searchQuery.data?.data.items ?? [])
      : (moviesQuery.data?.data.items ?? []);

    return source.filter((movie) => {
      const matchStatus = !status || movie.status === status;
      return matchStatus;
    });
  }, [debouncedSearch, moviesQuery.data?.data.items, searchQuery.data?.data.items, status]);

  const isLoading = debouncedSearch ? searchQuery.isLoading : moviesQuery.isLoading;
  const searchType = searchQuery.data?.data.searchType;
  const hasSemanticResult = debouncedSearch.length > 0 && searchType === 'Semantic';

  return (
    <main className="page-shell">
      <Space direction="vertical" size={20} style={{ width: '100%' }}>
        <div>
          <h1 className="page-title">Movies</h1>
          <div className="page-subtitle">Browse movies and select a showtime.</div>
        </div>

        <Card>
          <div className="movie-search-toolbar">
            <Input
              allowClear
              prefix={<SearchOutlined />}
              placeholder="What kind of movie do you want?"
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              style={{ width: 340, maxWidth: '100%' }}
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
            {hasSemanticResult && <Tag color="blue">Semantic</Tag>}
          </div>

          <Space wrap className="movie-search-chips">
            {searchSuggestions.map((suggestion) => (
              <Button key={suggestion} size="small" onClick={() => setSearch(suggestion)}>
                {suggestion}
              </Button>
            ))}
          </Space>

          {movies.length === 0 && !isLoading ? (
            <Empty />
          ) : (
            <Row gutter={[18, 18]}>
              {movies.map((movie) => (
                <Col xs={24} sm={12} md={8} lg={6} key={movie.id}>
                  <Card
                    hoverable
                    loading={isLoading}
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
