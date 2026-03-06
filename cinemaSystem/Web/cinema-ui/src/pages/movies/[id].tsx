// Movie Detail Page - Chi tiết phim và đặt vé
import { useParams, useNavigate } from 'react-router-dom';
import { useState } from 'react';
import {
    Card,
    Row,
    Col,
    Typography,
    Button,
    Tag,
    Tabs,
    DatePicker,
    List,
    Spin
} from 'antd';
import {
    PlayCircleOutlined,
    CalendarOutlined,
    ClockCircleOutlined,
    StarFilled
} from '@ant-design/icons';
import { useMovie } from '../../features/movies/hooks/useMovies';
import { useShowtimesByMovie } from '../../features/showtime/hooks/useShowtimes';
import { useCinemas } from '../../features/cinemas/hooks/useCinemas';
import type { Showtime } from '../../features/showtime/types/showtime.types';
import dayjs from 'dayjs';

const { Title, Paragraph, Text } = Typography;

const MovieDetailPage = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const [selectedDate, setSelectedDate] = useState<string>(dayjs().format('YYYY-MM-DD'));

    // Fetch movie details
    const { data: movie, isLoading: movieLoading, error: movieError } = useMovie(id!);

    // Fetch showtimes for selected date
    const { data: showtimesData, isLoading: showtimesLoading } = useShowtimesByMovie(id!, selectedDate);

    // Fetch cinemas
    const { data: cinemasData } = useCinemas();

    const handleBookTicket = (showtimeId: string) => {
        navigate(`/booking?showtimeId=${showtimeId}`);
    };

    const handleDateChange = (date: dayjs.Dayjs | null) => {
        if (date) {
            setSelectedDate(date.format('YYYY-MM-DD'));
        }
    };

    if (movieLoading) {
        return (
            <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '400px' }}>
                <Spin size="large" />
            </div>
        );
    }

    if (movieError || !movie) {
        return (
            <div style={{ textAlign: 'center', padding: '50px' }}>
                <Title level={4}>Không tìm thấy phim</Title>
                <Button type="primary" onClick={() => navigate('/movies')}>
                    Quay lại danh sách phim
                </Button>
            </div>
        );
    }

    const showtimes: Showtime[] = showtimesData?.items ? showtimesData.items : [];
    const cinemas = cinemasData?.items || [];

    // Group showtimes by cinema
    const showtimesByCinema = showtimes.reduce((acc: Record<string, Showtime[]>, showtime) => {
        const cinemaId = showtime.cinemaId;
        if (!acc[cinemaId]) {
            acc[cinemaId] = [];
        }
        acc[cinemaId].push(showtime);
        return acc;
    }, {});

    return (
        <div style={{ padding: '24px', background: '#141414', minHeight: '100vh' }}>
            {/* Movie Hero Section */}
            <div
                style={{
                    position: 'relative',
                    padding: '40px 0',
                    background: `linear-gradient(to bottom, transparent, #141414), url(${movie.backdropUrl || movie.posterUrl})`,
                    backgroundSize: 'cover',
                    backgroundPosition: 'center',
                    marginBottom: '24px'
                }}
            >
                <Row gutter={[32, 24]} align="middle">
                    <Col xs={24} md={8} lg={6}>
                        <Card
                            hoverable
                            cover={<img alt={movie.title} src={movie.posterUrl} style={{ borderRadius: '8px' }} />}
                        >
                        </Card>
                    </Col>
                    <Col xs={24} md={16} lg={18}>
                        <div style={{ color: '#fff' }}>
                            <Title level={1} style={{ color: '#fff', marginBottom: '8px' }}>
                                {movie.title}
                            </Title>
                            {movie.originalTitle && (
                                <Text style={{ color: '#999' }}>{movie.originalTitle}</Text>
                            )}

                            <div style={{ marginTop: '16px', display: 'flex', gap: '8px', flexWrap: 'wrap' }}>
                                {movie.genres?.map((genre) => (
                                    <Tag key={genre.id} color="red">{genre.name}</Tag>
                                ))}
                                {movie.rating && <Tag color="orange">{movie.rating}</Tag>}
                            </div>

                            <div style={{ marginTop: '16px', display: 'flex', gap: '24px', flexWrap: 'wrap' }}>
                                <div>
                                    <ClockCircleOutlined /> <Text style={{ color: '#ccc' }}> {movie.duration} phút</Text>
                                </div>
                                <div>
                                    <CalendarOutlined /> <Text style={{ color: '#ccc' }}> {dayjs(movie.releaseDate).format('DD/MM/YYYY')}</Text>
                                </div>
                                {movie.averageRating && (
                                    <div>
                                        <StarFilled style={{ color: '#faad14' }} /> <Text style={{ color: '#ccc' }}> {movie.averageRating.toFixed(1)}/5 ({movie.totalReviews} đánh giá)</Text>
                                    </div>
                                )}
                            </div>

                            <Paragraph style={{ color: '#ccc', marginTop: '16px', maxWidth: '600px' }}>
                                {movie.synopsis}
                            </Paragraph>

                            <div style={{ marginTop: '16px' }}>
                                <Text strong style={{ color: '#fff' }}>Đạo diễn: </Text>
                                <Text style={{ color: '#ccc' }}>{movie.director}</Text>
                            </div>

                            <div style={{ marginTop: '8px' }}>
                                <Text strong style={{ color: '#fff' }}>Diễn viên: </Text>
                                <Text style={{ color: '#ccc' }}>{movie.cast?.join(', ')}</Text>
                            </div>

                            <div style={{ marginTop: '24px', display: 'flex', gap: '16px' }}>
                                {movie.trailerUrl && (
                                    <Button
                                        type="primary"
                                        icon={<PlayCircleOutlined />}
                                        size="large"
                                        onClick={() => window.open(movie.trailerUrl, '_blank')}
                                    >
                                        Xem Trailer
                                    </Button>
                                )}
                                <Button
                                    type="primary"
                                    danger
                                    icon={<CalendarOutlined />}
                                    size="large"
                                    onClick={() => document.getElementById('showtimes')?.scrollIntoView({ behavior: 'smooth' })}
                                >
                                    Đặt Vé Ngay
                                </Button>
                            </div>
                        </div>
                    </Col>
                </Row>
            </div>

            {/* Showtimes Section */}
            <div id="showtimes" style={{ padding: '24px 0' }}>
                <Title level={2} style={{ color: '#fff', marginBottom: '24px' }}>
                    Lịch Chiếu
                </Title>

                <div style={{ marginBottom: '24px' }}>
                    <DatePicker
                        onChange={handleDateChange}
                        value={dayjs(selectedDate)}
                        disabledDate={(current) => current && current < dayjs().startOf('day')}
                        style={{ width: '200px' }}
                    />
                </div>

                {showtimesLoading ? (
                    <Spin size="large" />
                ) : showtimes.length === 0 ? (
                    <div style={{ textAlign: 'center', padding: '40px' }}>
                        <Text style={{ color: '#999' }}>Không có lịch chiếu cho ngày này</Text>
                    </div>
                ) : (
                    <Tabs
                        defaultActiveKey={cinemas[0]?.id}
                        items={cinemas.map((cinema) => ({
                            key: cinema.id,
                            label: cinema.name,
                            children: (
                                <List
                                    grid={{ gutter: 16, xs: 1, sm: 2, md: 3, lg: 4 }}
                                    dataSource={showtimesByCinema[cinema.id] || []}
                                    renderItem={(showtime) => (
                                        <List.Item>
                                            <Card
                                                hoverable
                                                style={{ background: '#1f1f1f', borderColor: '#333' }}
                                                actions={[
                                                    <Button
                                                        type="primary"
                                                        danger
                                                        onClick={() => handleBookTicket(showtime.id)}
                                                    >
                                                        Đặt Vé
                                                    </Button>
                                                ]}
                                            >
                                                <Card.Meta
                                                    title={<Text style={{ color: '#fff' }}>{showtime.screenName}</Text>}
                                                    description={
                                                        <div>
                                                            <Text style={{ color: '#ccc' }}>
                                                                {dayjs(showtime.startTime).format('HH:mm')} - {dayjs(showtime.endTime).format('HH:mm')}
                                                            </Text>
                                                            <br />
                                                            <Text style={{ color: '#ff4d4f' }}>
                                                                {showtime.basePrice.toLocaleString('vi-VN')}đ
                                                            </Text>
                                                            <br />
                                                            <Text type="secondary" style={{ fontSize: '12px' }}>
                                                                Còn {showtime.availableSeats} ghế
                                                            </Text>
                                                        </div>
                                                    }
                                                />
                                            </Card>
                                        </List.Item>
                                    )}
                                />
                            ),
                        }))}
                    />
                )}
            </div>
        </div>
    );
};

export default MovieDetailPage;
