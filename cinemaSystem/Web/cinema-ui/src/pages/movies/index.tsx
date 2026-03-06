// Movies List Page
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
    Row, Col, Card, Input, Select,
    Pagination, Skeleton, Typography, Space, Tag
} from 'antd';
import { SearchOutlined, FilterOutlined } from '@ant-design/icons';
import { useMovies, useGenres } from '../../features/movies/hooks/useMovies';

const { Title, Paragraph } = Typography;
const { Option } = Select;

interface Movie {
    id: string;
    title: string;
    posterUrl: string;
    duration: number;
    averageRating?: number;
    genres: { name: string }[];
    status: string;
}

function MovieListPage() {
    const navigate = useNavigate();
    const [pageIndex, setPageIndex] = useState(1);
    const [pageSize, setPageSize] = useState(12);
    const [search, setSearch] = useState('');
    const [genreId, setGenreId] = useState<string | undefined>();

    const { data, isLoading, error } = useMovies({
        pageIndex,
        pageSize,
        keyword: search || undefined,
        genreId,
    });

    // Fetch genres for filter dropdown
    const { data: genresData } = useGenres();

    const movies: Movie[] = data?.items || [];
    const totalCount = data?.totalCount || 0;

    return (
        <div style={{ padding: '24px', background: '#141414', minHeight: '100vh' }}>
            <Title level={2} style={{ color: '#fff', marginBottom: 24 }}>
                Danh sách phim
            </Title>

            {/* Search and Filter */}
            <Space style={{ marginBottom: 24 }} wrap>
                <Input
                    placeholder="Tìm kiếm phim..."
                    prefix={<SearchOutlined />}
                    value={search}
                    onChange={(e) => setSearch(e.target.value)}
                    style={{ width: 300 }}
                    allowClear
                />
                <Select
                    placeholder="Thể loại"
                    style={{ width: 200 }}
                    allowClear
                    onChange={setGenreId}
                    suffixIcon={<FilterOutlined />}
                    loading={!genresData}
                >
                    {genresData?.map((genre) => (
                        <Option key={genre.id} value={genre.id}>
                            {genre.name}
                        </Option>
                    ))}
                </Select>
            </Space>

            {/* Movies Grid */}
            <Row gutter={[24, 24]}>
                {isLoading ? (
                    Array.from({ length: 8 }).map((_, i) => (
                        <Col xs={24} sm={12} md={8} lg={6} key={i}>
                            <Skeleton active avatar paragraph={{ rows: 4 }} />
                        </Col>
                    ))
                ) : error ? (
                    <Col span={24}>
                        <Paragraph style={{ color: '#ff4d4f' }}>
                            Có lỗi khi tải danh sách phim. Vui lòng thử lại.
                        </Paragraph>
                    </Col>
                ) : movies.length === 0 ? (
                    <Col span={24}>
                        <Paragraph style={{ color: '#999' }}>
                            Không tìm thấy phim nào.
                        </Paragraph>
                    </Col>
                ) : (
                    movies.map((movie) => (
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
                                style={{
                                    background: '#1f1f1f',
                                    borderRadius: 8,
                                    overflow: 'hidden'
                                }}
                                bodyStyle={{ padding: 12 }}
                            >
                                <Card.Meta
                                    title={<span style={{ color: '#fff' }}>{movie.title}</span>}
                                    description={
                                        <div>
                                            <p style={{ margin: 0, color: '#a3a3a3', fontSize: 12 }}>
                                                {movie.duration} phút
                                            </p>
                                            <Space wrap style={{ marginTop: 4 }}>
                                                {movie.genres?.slice(0, 2).map((g) => (
                                                    <Tag key={g.name} color="blue">{g.name}</Tag>
                                                ))}
                                            </Space>
                                            <p style={{ margin: '8px 0 0', color: '#faad14', fontSize: 14 }}>
                                                ★ {movie.averageRating ? movie.averageRating.toFixed(1) : 'N/A'}/10
                                            </p>
                                        </div>
                                    }
                                />
                            </Card>
                        </Col>
                    ))
                )}
            </Row>

            {/* Pagination */}
            {totalCount > 0 && (
                <div style={{ marginTop: 32, textAlign: 'center' }}>
                    <Pagination
                        current={pageIndex}
                        pageSize={pageSize}
                        total={totalCount}
                        onChange={(page, size) => {
                            setPageIndex(page);
                            setPageSize(size);
                        }}
                        showSizeChanger
                        showTotal={(total) => `Tổng ${total} phim`}
                    />
                </div>
            )}
        </div>
    );
}

export default MovieListPage;
