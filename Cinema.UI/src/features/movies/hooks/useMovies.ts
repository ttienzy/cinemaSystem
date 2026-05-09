import { useQuery } from '@tanstack/react-query';
import { movieApi } from '../api/movieApi';

export const useMovies = (pageNumber: number, pageSize: number) => {
  return useQuery({
    queryKey: ['movies', pageNumber, pageSize],
    queryFn: () => movieApi.getMovies(pageNumber, pageSize),
  });
};
