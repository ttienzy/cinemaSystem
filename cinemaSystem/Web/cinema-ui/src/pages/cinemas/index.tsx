// Cinemas Page - Danh sách rạp chiếu phim
import { useNavigate } from 'react-router-dom';
import { Card, Row, Col, Typography, Input, Spin, Empty, Button, Tag, List } from 'antd';
import { EnvironmentOutlined, HomeOutlined, PhoneOutlined } from '@ant-design/icons';
import { useState } from 'react';
import { useCinemas } from '../../features/cinemas/hooks/useCinemas';

const { Title, Text, Paragraph } = Typography;
const { Search } = Input;

const CinemasPage = () => {
    const navigate = useNavigate();
    const [searchKeyword, setSearchKeyword] = useState('');
    const { data: cinemasData, isLoading } = useCinemas();

    const cinemas = cinemasData?.items || [];

    const filteredCinemas = cinemas.filter((cinema) =>
        cinema.name.toLowerCase().includes(searchKeyword.toLowerCase()) ||
        cinema.address.toLowerCase().includes(searchKeyword.toLowerCase()) ||
        cinema.city?.toLowerCase().includes(searchKeyword.toLowerCase())
    );

    const handleCinemaClick = (cinemaId: string) => {
        navigate(`/movies?cinemaId=${cinemaId}`);
    };

    if (isLoading) {
        return (
            <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '400px' }}>
                <Spin size="large" />
            </div>
        );
    }

    return (
        <div style={{ padding: '24px', background: '#141414', minHeight: '100vh' }}>
            <Title level={2} style={{ color: '#fff', marginBottom: '24px' }}>
                Danh Sách Rạp Chiếu Phim
            </Title>

            <Search
                placeholder="Tìm kiếm rạp..."
                onChange={(e) => setSearchKeyword(e.target.value)}
                style={{ marginBottom: '24px', maxWidth: '400px' }}
                size="large"
            />

            {filteredCinemas.length === 0 ? (
                <Empty description="Không tìm thấy rạp nào" />
            ) : (
                <List
                    grid={{ gutter: 16, xs: 1, sm: 1, md: 2, lg: 3, xl: 4 }}
                    dataSource={filteredCinemas}
                    renderItem={(cinema) => (
                        <List.Item>
                            <Card
                                hoverable
                                onClick={() => handleCinemaClick(cinema.id)}
                                style={{
                                    background: '#1f1f1f',
                                    borderColor: '#333',
                                    height: '100%'
                                }}
                                cover={
                                    <div
                                        style={{
                                            height: '150px',
                                            background: cinema.imageUrl
                                                ? `url(${cinema.imageUrl}) center/cover`
                                                : '#333',
                                            display: 'flex',
                                            alignItems: 'center',
                                            justifyContent: 'center'
                                        }}
                                    >
                                        {!cinema.imageUrl && <HomeOutlined style={{ fontSize: 48, color: '#666' }} />}
                                    </div>
                                }
                            >
                                <Card.Meta
                                    title={<Text style={{ color: '#fff' }}>{cinema.name}</Text>}
                                    description={
                                        <div>
                                            <Paragraph
                                                style={{ color: '#999', marginBottom: '8px' }}
                                                ellipsis={{ rows: 2 }}
                                            >
                                                <EnvironmentOutlined /> {cinema.address}
                                                {cinema.district && `, ${cinema.district}`}
                                                {cinema.city && `, ${cinema.city}`}
                                            </Paragraph>

                                            <div style={{ display: 'flex', gap: '8px', flexWrap: 'wrap' }}>
                                                <Tag color="blue">{cinema.totalScreens} Phòng chiếu</Tag>
                                                {!cinema.isActive && <Tag color="red">Tạm đóng</Tag>}
                                            </div>

                                            {cinema.phone && (
                                                <Text type="secondary" style={{ fontSize: '12px', display: 'block', marginTop: '8px' }}>
                                                    <PhoneOutlined /> {cinema.phone}
                                                </Text>
                                            )}
                                        </div>
                                    }
                                />
                            </Card>
                        </List.Item>
                    )}
                />
            )}
        </div>
    );
};

export default CinemasPage;
