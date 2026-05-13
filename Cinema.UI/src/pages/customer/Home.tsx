import React from 'react';
import { Carousel, Button, Spin, Tabs } from 'antd';
import { useQuery } from '@tanstack/react-query';
import { movieApi, type Movie } from '../../features/movies/api/movieApi';
import dayjs from '../../utils/dayjs';
import { toLocalDateTime } from '../../utils/dateTime';
import { useNavigate } from 'react-router-dom';

const { TabPane } = Tabs;

const Home: React.FC = () => {
  const navigate = useNavigate();
  const { data: apiResponse, isLoading } = useQuery({
    queryKey: ['storefront-movies'],
    queryFn: () => movieApi.getMovies(1, 100),
  });

  const movies = apiResponse?.data?.items || [];
  const now = dayjs();

  // Phân loại Đang chiếu / Sắp chiếu dựa vào ReleaseDate
  const nowShowing = movies.filter(m => toLocalDateTime(m.releaseDate).isBefore(now) || toLocalDateTime(m.releaseDate).isSame(now, 'day'));
  const comingSoon = movies.filter(m => toLocalDateTime(m.releaseDate).isAfter(now));

  const banners = nowShowing.slice(0, 3); // Lấy 3 phim đang chiếu làm banner

  if (isLoading) return <div className="global-spinner"><Spin size="large" /></div>;

  const renderMovieGrid = (movieList: Movie[]) => (
    <div className="movie-grid">
      {movieList.map(movie => (
        <div key={movie.id} className="movie-card" onClick={() => navigate(`/movies/${movie.id}`)}>
          <div className="movie-poster">
            <img src={movie.posterUrl || 'https://via.placeholder.com/300x450?text=No+Poster'} alt={movie.title} />
            <div className="movie-overlay">
              <Button type="primary" shape="round" size="large">Mua Vé Ngay</Button>
            </div>
          </div>
          <div className="movie-info">
            <h3 className="movie-title">{movie.title}</h3>
            <p className="movie-meta">
              <span>{movie.language || 'Phụ đề'}</span> • <span>{movie.duration} phút</span>
            </p>
            <p className="movie-date">Khởi chiếu: {toLocalDateTime(movie.releaseDate).format('DD/MM/YYYY')}</p>
          </div>
        </div>
      ))}
    </div>
  );

  return (
    <div className="storefront-container">
      {/* Banner Carousel */}
      <Carousel autoplay effect="fade" className="hero-carousel">
        {banners.map(movie => (
          <div key={`banner-${movie.id}`} className="hero-slide">
            <div
              className="hero-background"
              style={{ backgroundImage: `url(${movie.posterUrl || 'https://via.placeholder.com/1200x500'})` }}
            />
            <div className="hero-content">
              <h1 className="hero-title">{movie.title}</h1>
              <p className="hero-desc">{movie.description?.substring(0, 150)}...</p>
              <Button type="primary" size="large" onClick={() => navigate(`/movies/${movie.id}`)}>
                Đặt Vé Ngay
              </Button>
            </div>
          </div>
        ))}
      </Carousel>

      {/* Tabs Phim */}
      <div className="movies-section">
        <Tabs defaultActiveKey="1" centered size="large" className="custom-tabs">
          <TabPane tab="Phim Đang Chiếu" key="1">
            {renderMovieGrid(nowShowing)}
          </TabPane>
          <TabPane tab="Phim Sắp Chiếu" key="2">
            {renderMovieGrid(comingSoon)}
          </TabPane>
        </Tabs>
      </div>
    </div>
  );
};

export default Home;
