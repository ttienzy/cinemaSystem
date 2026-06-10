export interface ApiError {
  code: string;
  message: string;
  field?: string;
}

export interface ApiResponse<T = unknown> {
  data: T;
  success: boolean;
  message: string;
  statusCode: number;
  errors: ApiError[] | null;
  traceId: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
