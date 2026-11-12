export interface PaginationResponse {
  pageNumber: number;
  totalPages: number;
  totalCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface PagedResult<T> {
  items: T[];
  pagination: PaginationResponse;
}

export interface PaginationRequest {
  pageNumber: number;
  pageSize: number;
}

// ⚠️ الغلاف العام الذي يضيفه ResponseWrapperMiddleware لكل ردود الـ API (كل الـ features تحتاجه)
export interface ApiEnvelope<T> {
  data: T;
  statusCode: number;
  isSuccess: boolean;
}