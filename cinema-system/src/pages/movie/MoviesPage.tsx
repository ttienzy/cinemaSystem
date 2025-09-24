import React, { useState, useMemo, useEffect } from 'react';
import { Search, Filter, Calendar, Star, Clock, Shield } from 'lucide-react';
import type { MovieSection } from '../../types/movie.types';
import { useAppDispatch, useAppSelector } from '../../hooks/redux';
import { getMoviesSection } from '../../store/slices/movieSlice';
import { useNavigate } from 'react-router-dom';


const MoviesPage: React.FC = () => {

    const [searchTerm, setSearchTerm] = useState('');
    const [statusFilter, setStatusFilter] = useState<'All' | 'ComingSoon' | 'Showing'>('All');
    const [sortBy, setSortBy] = useState<'title' | 'releaseDate' | 'ageRating' | 'duration'>('releaseDate');
    const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('desc');

    const currentDate = new Date('2025-09-15');

    const { moviesSection, loading, error } = useAppSelector(state => state.movie);
    const dispatch = useAppDispatch();
    const navigate = useNavigate();

    useEffect(() => {
        dispatch(getMoviesSection());
    }, [dispatch]);

    const getStatus = (movie: MovieSection) => {
        return new Date(movie.releaseDate) <= currentDate ? 'Showing' : 'ComingSoon';
    };

    // Filtered and sorted movies
    const filteredMovies = useMemo(() => {
        let filtered = moviesSection.filter(movie => {
            const matchesSearch = movie.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
                movie.description.toLowerCase().includes(searchTerm.toLowerCase());
            const matchesStatus = statusFilter === 'All' || getStatus(movie) === statusFilter;
            return matchesSearch && matchesStatus;
        });

        // Sort movies
        filtered.sort((a, b) => {
            let aValue: any, bValue: any;

            switch (sortBy) {
                case 'title':
                    aValue = a.title.toLowerCase();
                    bValue = b.title.toLowerCase();
                    break;
                case 'releaseDate':
                    aValue = new Date(a.releaseDate);
                    bValue = new Date(b.releaseDate);
                    break;
                case 'ageRating':
                    aValue = a.ageRating.toLowerCase();
                    bValue = b.ageRating.toLowerCase();
                    break;
                case 'duration':
                    aValue = a.durationMinutes;
                    bValue = b.durationMinutes;
                    break;
                default:
                    return 0;
            }

            if (aValue < bValue) return sortOrder === 'asc' ? -1 : 1;
            if (aValue > bValue) return sortOrder === 'asc' ? 1 : -1;
            return 0;
        });

        return filtered;
    }, [moviesSection, searchTerm, statusFilter, sortBy, sortOrder]);

    const showingMovies = filteredMovies.filter(movie => getStatus(movie) === 'Showing');
    const comingSoonMovies = filteredMovies.filter(movie => getStatus(movie) === 'ComingSoon');

    const formatDuration = (minutes: number) => {
        const hours = Math.floor(minutes / 60);
        const mins = minutes % 60;
        return `${hours}h ${mins}m`;
    };

    const formatDate = (dateString: string) => {
        return new Date(dateString).toLocaleDateString('vi-VN');
    };

    const getStatusBadge = (status: string) => {
        const baseClasses = "px-3 py-1 rounded-full text-sm font-medium";
        if (status === 'Showing') {
            return `${baseClasses} bg-green-100 text-green-800`;
        } else {
            return `${baseClasses} bg-blue-100 text-blue-800`;
        }
    };

    const getStatusText = (status: string) => {
        return status === 'ComingSoon' ? 'Sắp Chiếu' : 'Đang Chiếu';
    };

    return (
        <div className="min-h-screen bg-gray-50 py-8 px-4">
            <div className="max-w-7xl mx-auto">
                {/* Header */}
                <div className="mb-8">
                    <h1 className="text-3xl font-bold text-gray-900 mb-2">Quản Lý Phim</h1>
                    <p className="text-gray-600">Danh sách các bộ phim đang chiếu và sắp chiếu</p>
                </div>

                {/* Filters and Search */}
                <div className="bg-white rounded-lg shadow-sm p-6 mb-8">
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                        {/* Search */}
                        <div className="relative">
                            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-4 h-4" />
                            <input
                                type="text"
                                placeholder="Tìm kiếm phim..."
                                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                                value={searchTerm}
                                onChange={(e) => setSearchTerm(e.target.value)}
                            />
                        </div>

                        {/* Status Filter */}
                        <div className="relative">
                            <Filter className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-4 h-4" />
                            <select
                                title='Tất cả trạng thái'
                                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 appearance-none bg-white"
                                value={statusFilter}
                                onChange={(e) => setStatusFilter(e.target.value as any)}
                            >
                                <option value="All">Tất cả trạng thái</option>
                                <option value="Showing">Đang chiếu</option>
                                <option value="ComingSoon">Sắp chiếu</option>
                            </select>
                        </div>

                        {/* Sort By */}
                        <div>
                            <select
                                title='Sắp xếp theo'
                                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 appearance-none bg-white"
                                value={sortBy}
                                onChange={(e) => setSortBy(e.target.value as any)}
                            >
                                <option value="releaseDate">Ngày phát hành</option>
                                <option value="title">Tên phim</option>
                                <option value="ageRating">Phân loại tuổi</option>
                                <option value="duration">Thời lượng</option>
                            </select>
                        </div>

                        {/* Sort Order */}
                        <div>
                            <select
                                title='Sắp xếp theo'
                                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 appearance-none bg-white"
                                value={sortOrder}
                                onChange={(e) => setSortOrder(e.target.value as any)}
                            >
                                <option value="desc">Giảm dần</option>
                                <option value="asc">Tăng dần</option>
                            </select>
                        </div>
                    </div>

                    {/* Results count */}
                    <div className="mt-4 text-sm text-gray-600">
                        Hiển thị {filteredMovies.length} trong tổng số {moviesSection.length} phim
                    </div>
                </div>

                {/* Movies Sections */}
                <div className="space-y-12">
                    {(statusFilter === 'All' || statusFilter === 'Showing') && (
                        <div>
                            <h2 className="text-2xl font-bold text-gray-900 mb-4">Đang Chiếu</h2>
                            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
                                {showingMovies.map((movie) => (
                                    <div
                                        key={movie.movieId}
                                        className="bg-white rounded-xl shadow-md overflow-hidden hover:shadow-xl transition-shadow duration-300 cursor-pointer"
                                        onClick={() => navigate(`/movies/detail/${movie.movieId}`)}>
                                        {/* Poster */}
                                        <div className="relative">
                                            <img
                                                src={movie.posterUrl}
                                                alt={movie.title}
                                                className="w-full h-72 object-cover"
                                            />
                                            <div className="absolute top-4 right-4">
                                                <span className={getStatusBadge(getStatus(movie))}>
                                                    {getStatusText(getStatus(movie))}
                                                </span>
                                            </div>
                                        </div>

                                        {/* Movie Info */}
                                        <div className="p-5">
                                            <h3 className="text-xl font-semibold text-gray-900 mb-2 line-clamp-2">
                                                {movie.title}
                                            </h3>

                                            <p className="text-gray-600 text-sm mb-4 line-clamp-3">
                                                {movie.description}
                                            </p>

                                            <div className="space-y-2 text-sm text-gray-500">
                                                {/* Release Date */}
                                                <div className="flex items-center">
                                                    <Calendar className="w-4 h-4 mr-2" />
                                                    {formatDate(movie.releaseDate)}
                                                </div>

                                                {/* Duration */}
                                                <div className="flex items-center">
                                                    <Clock className="w-4 h-4 mr-2" />
                                                    {formatDuration(movie.durationMinutes)}
                                                </div>

                                                {/* Age Rating */}
                                                {movie.ageRating && (
                                                    <div className="flex items-center">
                                                        <Shield className="w-4 h-4 mr-2 text-blue-500" />
                                                        {movie.ageRating}
                                                    </div>
                                                )}
                                            </div>

                                            {/* Details Button */}
                                            {/* <div className="mt-4">
                                                <button
                                                    className="w-full bg-blue-500 text-white py-2 rounded-lg hover:bg-blue-600 transition-colors"
                                                    onClick={() => alert(`Chi tiết cho phim: ${movie.title}`)}
                                                >
                                                    Chi tiết
                                                </button>
                                            </div> */}
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>
                    )}

                    {(statusFilter === 'All' || statusFilter === 'ComingSoon') && (
                        <div>
                            <h2 className="text-2xl font-bold text-gray-900 mb-4">Sắp Chiếu</h2>
                            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
                                {comingSoonMovies.map((movie) => (
                                    <div
                                        key={movie.movieId}
                                        className="bg-white rounded-xl shadow-md overflow-hidden hover:shadow-xl transition-shadow duration-300 cursor-pointer"
                                        onClick={() => navigate(`/movies/detail/${movie.movieId}`)}
                                    >
                                        {/* Poster */}
                                        <div className="relative">
                                            <img
                                                src={movie.posterUrl}
                                                alt={movie.title}
                                                className="w-full h-72 object-cover"
                                            />
                                            <div className="absolute top-4 right-4">
                                                <span className={getStatusBadge(getStatus(movie))}>
                                                    {getStatusText(getStatus(movie))}
                                                </span>
                                            </div>
                                        </div>

                                        {/* Movie Info */}
                                        <div className="p-5">
                                            <h3 className="text-xl font-semibold text-gray-900 mb-2 line-clamp-2">
                                                {movie.title}
                                            </h3>

                                            <p className="text-gray-600 text-sm mb-4 line-clamp-3">
                                                {movie.description}
                                            </p>

                                            <div className="space-y-2 text-sm text-gray-500">
                                                {/* Release Date */}
                                                <div className="flex items-center">
                                                    <Calendar className="w-4 h-4 mr-2" />
                                                    {formatDate(movie.releaseDate)}
                                                </div>

                                                {/* Duration */}
                                                <div className="flex items-center">
                                                    <Clock className="w-4 h-4 mr-2" />
                                                    {formatDuration(movie.durationMinutes)}
                                                </div>

                                                {/* Age Rating */}
                                                {movie.ageRating && (
                                                    <div className="flex items-center">
                                                        <Shield className="w-4 h-4 mr-2 text-blue-500" />
                                                        {movie.ageRating}
                                                    </div>
                                                )}
                                            </div>

                                            {/* Details Button */}
                                            {/* <div className="mt-4">
                                                <button
                                                    className="w-full bg-blue-500 text-white py-2 rounded-lg hover:bg-blue-600 transition-colors"
                                                    onClick={() => alert(`Chi tiết cho phim: ${movie.title}`)}
                                                >
                                                    Chi tiết
                                                </button>
                                            </div> */}
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>
                    )}
                </div>

                {/* Empty State */}
                {filteredMovies.length === 0 && (
                    <div className="text-center py-12">
                        <div className="text-gray-500 text-lg mb-2">Không tìm thấy phim nào</div>
                        <div className="text-gray-400 text-sm">Thử thay đổi từ khóa tìm kiếm hoặc bộ lọc</div>
                    </div>
                )}
            </div>
        </div>
    );
};

export default MoviesPage;