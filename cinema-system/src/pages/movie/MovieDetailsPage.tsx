import React, { useState, useEffect } from 'react';
import { Star, Calendar, Clock, Users, Award, Shield, Eye } from 'lucide-react';
import type { MovieDetailsData } from '../../types/movie.types';
import { useAppSelector, useAppDispatch } from '../../hooks/redux';
import { getMovieDetails } from '../../store/slices/movieSlice';
import { useParams } from 'react-router-dom';


const MovieDetailsPage: React.FC = () => {
    const { loading, movieDetail } = useAppSelector((state) => state.movie);
    console.log(movieDetail);
    const dispatch = useAppDispatch();
    const { id: movieId } = useParams<{ id: string }>();
    if (movieId === undefined) {
        return (
            <div className="min-h-screen bg-gray-900 flex items-center justify-center">
                <div className="text-white text-xl">The movie ID is undefined</div>
            </div>
        );
    }
    // Mock data - replace with actual API call
    useEffect(() => {
        // Simulate API call
        setTimeout(() => {
            dispatch(getMovieDetails(movieId));
        }, 1000);
    }, []);

    const formatDate = (dateString: string) => {
        return new Date(dateString).toLocaleDateString('vi-VN', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
    };

    const formatDuration = (minutes: number) => {
        const hours = Math.floor(minutes / 60);
        const mins = minutes % 60;
        return `${hours}h ${mins}m`;
    };

    if (loading) {
        return (
            <div className="min-h-screen bg-gray-900 flex items-center justify-center">
                <div className="text-white text-xl">Đang tải thông tin phim...</div>
            </div>
        );
    }

    if (!movieDetail) {
        return (
            <div className="min-h-screen bg-gray-900 flex items-center justify-center">
                <div className="text-white text-xl">Không tìm thấy thông tin phim</div>
            </div>
        );
    }

    const { movie, castCrews, certifications, genres, copyrights } = movieDetail;

    const directors = castCrews.filter(cc => cc.roleType === 'Director');
    const actors = castCrews.filter(cc => cc.roleType === 'Actor');
    const writers = castCrews.filter(cc => cc.roleType === 'Writer');

    return (
        <div className="min-h-screen bg-gray-900 text-white">
            {/* Hero Section */}
            <div className="relative h-96 bg-gradient-to-t from-gray-900 to-transparent">
                <div className="absolute inset-0 bg-black bg-opacity-50"></div>
                <div className="relative container mx-auto px-4 h-full flex items-end pb-8">
                    <div className="flex flex-col md:flex-row gap-6 w-full">
                        <img
                            src={movie.posterUrl || 'https://img.freepik.com/free-vector/bird-colorful-logo-gradient-vector_343694-1365.jpg?semt=ais_incoming&w=740&q=80'}
                            alt={movie.title}
                            className="w-48 h-72 object-cover rounded-lg shadow-2xl"
                        />
                        <div className="flex-1">
                            <h1 className="text-4xl md:text-6xl font-bold mb-4">{movie.title}</h1>
                            <div className="flex flex-wrap gap-4 mb-4">
                                <div className="flex items-center gap-2">
                                    <Calendar className="w-5 h-5" />
                                    <span>{formatDate(movie.releaseDate)}</span>
                                </div>
                                <div className="flex items-center gap-2">
                                    <Clock className="w-5 h-5" />
                                    <span>{formatDuration(movie.durationMinutes)}</span>
                                </div>
                                <div className="flex items-center gap-2">
                                    <Eye className="w-5 h-5" />
                                    <span className="px-2 py-1 bg-green-600 rounded text-sm">{movie.status}</span>
                                </div>
                            </div>
                            <div className="flex flex-wrap gap-2 mb-4">
                                {genres.map(genre => (
                                    <span key={genre.id} className="px-3 py-1 bg-blue-600 rounded-full text-sm">
                                        {genre.genreName}
                                    </span>
                                ))}
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div className="container mx-auto px-4 py-8">
                <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
                    {/* Main Content */}
                    <div className="lg:col-span-2 space-y-8">
                        {/* Description */}
                        <section>
                            <h2 className="text-2xl font-bold mb-4">Tóm tắt nội dung</h2>
                            <p className="text-gray-300 leading-relaxed">{movie.description}</p>
                        </section>

                        {/* Cast & Crew */}
                        <section>
                            <h2 className="text-2xl font-bold mb-6">Diễn viên và Đoàn làm phim</h2>

                            {/* Directors */}
                            {directors.length > 0 && (
                                <div className="mb-6">
                                    <h3 className="text-xl font-semibold mb-3 flex items-center gap-2">
                                        <Users className="w-5 h-5" />
                                        Đạo diễn
                                    </h3>
                                    <div className="flex flex-wrap gap-3">
                                        {directors.map(director => (
                                            <span key={director.id} className="px-4 py-2 bg-purple-600 rounded-lg">
                                                {director.personName}
                                            </span>
                                        ))}
                                    </div>
                                </div>
                            )}

                            {/* Writers */}
                            {writers.length > 0 && (
                                <div className="mb-6">
                                    <h3 className="text-xl font-semibold mb-3">Biên kịch</h3>
                                    <div className="flex flex-wrap gap-3">
                                        {writers.map(writer => (
                                            <span key={writer.id} className="px-4 py-2 bg-orange-600 rounded-lg">
                                                {writer.personName}
                                            </span>
                                        ))}
                                    </div>
                                </div>
                            )}

                            {/* Actors */}
                            {actors.length > 0 && (
                                <div>
                                    <h3 className="text-xl font-semibold mb-3">Diễn viên chính</h3>
                                    <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
                                        {actors.map(actor => (
                                            <div key={actor.id} className="px-4 py-2 bg-gray-700 rounded-lg text-center">
                                                {actor.personName}
                                            </div>
                                        ))}
                                    </div>
                                </div>
                            )}
                        </section>
                    </div>

                    {/* Sidebar */}
                    <div className="space-y-6">
                        {/* Certifications */}
                        <section className="bg-gray-800 rounded-lg p-6">
                            <h3 className="text-xl font-bold mb-4 flex items-center gap-2">
                                <Award className="w-5 h-5" />
                                Xếp hạng độ tuổi
                            </h3>
                            <div className="space-y-3">
                                {certifications.map(cert => (
                                    <div key={cert.id} className="flex justify-between items-center">
                                        <span className="font-semibold">{cert.certificationBody}</span>
                                        <span className="px-3 py-1 bg-yellow-600 rounded font-bold">
                                            {cert.rating}
                                        </span>
                                    </div>
                                ))}
                            </div>
                        </section>

                        {/* Copyright Info */}
                        <section className="bg-gray-800 rounded-lg p-6">
                            <h3 className="text-xl font-bold mb-4 flex items-center gap-2">
                                <Shield className="w-5 h-5" />
                                Thông tin bản quyền
                            </h3>
                            <div className="space-y-4">
                                {copyrights.map(copyright => (
                                    <div key={copyright.id} className="space-y-2">
                                        <div>
                                            <span className="text-gray-400">Nhà phân phối:</span>
                                            <div className="font-semibold">{copyright.distributorCompany}</div>
                                        </div>
                                        <div>
                                            <span className="text-gray-400">Thời hạn:</span>
                                            <div className="text-sm">
                                                {formatDate(copyright.licenseStartDate)} - {formatDate(copyright.licenseEndDate)}
                                            </div>
                                        </div>
                                        <div>
                                            <span className={`px-2 py-1 rounded text-xs ${copyright.status === 'Active' ? 'bg-green-600' : 'bg-red-600'
                                                }`}>
                                                {copyright.status}
                                            </span>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </section>

                        {/* Movie Info */}
                        <section className="bg-gray-800 rounded-lg p-6">
                            <h3 className="text-xl font-bold mb-4">Thông tin phim</h3>
                            <div className="space-y-3 text-sm">
                                <div>
                                    <span className="text-gray-400">Thời lượng:</span>
                                    <span className="ml-2">{formatDuration(movie.durationMinutes)}</span>
                                </div>
                                <div>
                                    <span className="text-gray-400">Ngày tạo:</span>
                                    <span className="ml-2">{formatDate(movie.createdAt)}</span>
                                </div>
                            </div>
                        </section>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default MovieDetailsPage;