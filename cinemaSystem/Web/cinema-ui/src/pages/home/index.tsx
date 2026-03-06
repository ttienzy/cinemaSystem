import { useQuery } from '@tanstack/react-query';
import { Carousel, Card, Row, Col, Tabs, Skeleton, Typography } from 'antd';
import { useNavigate } from 'react-router-dom';
import api from '../../shared/api/axios.instance';
import { Endpoints } from '../../shared/api/endpoints';

const { Title, Paragraph } = Typography;

interface MovieSummary {
    id: string;
    title: string;
    posterUrl: string;
    duration: number;
    rating: number;
    genreNames: string[];
}

function HomePage() {
    const navigate = useNavigate();

    const { data: nowShowing, isLoading: loadingNowShowing } = useQuery({
        queryKey: ['movies', 'now-showing'],
        queryFn: async () => {
            const response = await api.get<MovieSummary[]>(Endpoints.MOVIES.NOW_SHOWING);
            return response.data;
        },
    });

    const { data: comingSoon, isLoading: loadingComingSoon } = useQuery({
        queryKey: ['movies', 'coming-soon'],
        queryFn: async () => {
            const response = await api.get<MovieSummary[]>(Endpoints.MOVIES.COMING_SOON);
            return response.data;
        },
    });

    const tabItems = [
        {
            key: 'now-showing',
            label: 'Phim đang chiếu',
            children: (
                <Row gutter={[24, 24]}>
                    {loadingNowShowing ? (
                        Array.from({ length: 8 }).map((_, i) => (
                            <Col xs={24} sm={12} md={8} lg={6} key={i}>
                                <Skeleton active avatar paragraph={{ rows: 4 }} />
                            </Col>
                        ))
                    ) : (
                        nowShowing?.map((movie) => (
                            <Col xs={24} sm={12} md={8} lg={6} key={movie.id}>
                                <Card
                                    hoverable
                                    cover={
                                        <div style={{ position: 'relative', paddingTop: '150%' }}>
                                            <img
                                                src={movie.posterUrl || 'https://via.placeholder.com/300x450'}
                                                alt={movie.title}
                                                style={{
                                                    position: 'absolute',
                                                    top: 0,
                                                    left: 0,
                                                    width: '100%',
                                                    height: '100%',
                                                    objectFit: 'cover',
                                                }}
                                            />
                                        </div>
                                    }
                                    onClick={() => navigate(`/movies/${movie.id}`)}
                                    style={{ borderRadius: 8, overflow: 'hidden' }}
                                >
                                    <Card.Meta
                                        title={movie.title}
                                        description={
                                            <div>
                                                <p style={{ margin: 0, color: '#a3a3a3' }}>
                                                    {movie.duration} phút • {movie.genreNames?.join(', ')}
                                                </p>
                                                <p style={{ margin: '8px 0 0', color: '#faad14' }}>
                                                    ★ {movie.rating}/10
                                                </p>
                                            </div>
                                        }
                                    />
                                </Card>
                            </Col>
                        ))
                    )}
                </Row>
            ),
        },
        {
            key: 'coming-soon',
            label: 'Phim sắp chiếu',
            children: (
                <Row gutter={[24, 24]}>
                    {loadingComingSoon ? (
                        Array.from({ length: 8 }).map((_, i) => (
                            <Col xs={24} sm={12} md={8} lg={6} key={i}>
                                <Skeleton active avatar paragraph={{ rows: 4 }} />
                            </Col>
                        ))
                    ) : (
                        comingSoon?.map((movie) => (
                            <Col xs={24} sm={12} md={8} lg={6} key={movie.id}>
                                <Card
                                    hoverable
                                    cover={
                                        <div style={{ position: 'relative', paddingTop: '150%' }}>
                                            <img
                                                src={movie.posterUrl || 'https://via.placeholder.com/300x450'}
                                                alt={movie.title}
                                                style={{
                                                    position: 'absolute',
                                                    top: 0,
                                                    left: 0,
                                                    width: '100%',
                                                    height: '100%',
                                                    objectFit: 'cover',
                                                }}
                                            />
                                        </div>
                                    }
                                    onClick={() => navigate(`/movies/${movie.id}`)}
                                    style={{ borderRadius: 8, overflow: 'hidden' }}
                                >
                                    <Card.Meta
                                        title={movie.title}
                                        description={
                                            <div>
                                                <p style={{ margin: 0, color: '#a3a3a3' }}>
                                                    {movie.duration} phút • {movie.genreNames?.join(', ')}
                                                </p>
                                                <p style={{ margin: '8px 0 0', color: '#faad14' }}>
                                                    ★ {movie.rating}/10
                                                </p>
                                            </div>
                                        }
                                    />
                                </Card>
                            </Col>
                        ))
                    )}
                </Row>
            ),
        },
    ];

    return (
        <div>
            <div style={{ marginBottom: 32 }}>
                <Carousel autoplay dots={{ className: 'carousel-dots' }}>
                    <div>
                        <div style={{
                            height: 400,
                            background: 'linear-gradient(to right, #141414, #0a0a0a), url(https://via.placeholder.com/1920x400)',
                            backgroundSize: 'cover',
                            backgroundPosition: 'center',
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            borderRadius: 12,
                        }}>
                            <div style={{ textAlign: 'center', padding: 32 }}>
                                <Title level={1} style={{ color: '#fff', marginBottom: 16 }}>
                                    Chào mừng đến với Cinema
                                </Title>
                                <Paragraph style={{ color: '#a3a3a3', fontSize: 18 }}>
                                    Đặt vé xem phim dễ dàng, nhanh chóng
                                </Paragraph>
                            </div>
                        </div>
                    </div>
                </Carousel>
            </div>

            <Title level={2} style={{ color: '#fff', marginBottom: 24 }}>
                Lịch chiếu phim
            </Title>

            <Tabs defaultActiveKey="now-showing" items={tabItems} />
        </div>
    );
}

export default HomePage;
