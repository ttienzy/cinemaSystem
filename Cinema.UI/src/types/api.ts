// ============================================================
// Standard Response Wrapper - Kiểu dữ liệu chung cho mọi API
// ============================================================
// Mọi API trong hệ thống Microservices CinemaSystem đều trả về
// cấu trúc đồng nhất này. Đây là "hợp đồng" (contract) giữa
// Backend và Frontend.
//
// Ví dụ response từ Backend:
// {
//   "data": { ... },
//   "success": true,
//   "message": "Login successful",
//   "statusCode": 200,
//   "errors": null,
//   "traceId": "0HNL7I57EGSK2:00000004"
// }
// ============================================================

/**
 * Standard Response Wrapper — Lớp vỏ bọc tiêu chuẩn.
 * Tất cả API đều trả về dạng này. TypeScript đảm bảo
 * type-safety cho phần `data` bên trong.
 */
export interface ApiResponse<T = unknown> {
  data: T;
  success: boolean;
  message: string;
  statusCode: number;
  errors: ApiError[] | null;
  traceId: string;
}

/**
 * Chi tiết lỗi validation hoặc business logic.
 */
export interface ApiError {
  code: string;
  message: string;
  field?: string;
}

/**
 * Response cho các API có phân trang.
 */
export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

/**
 * Helper type: Lấy phần data bên trong ApiResponse.
 * Dùng khi cần chỉ kiểu data, ví dụ:
 *   type MovieList = Unwrap<typeof movieApi.getMovies>;
 *   // → PaginatedResponse<Movie>
 */
export type Unwrap<T> = T extends ApiResponse<infer U> ? U : never;
