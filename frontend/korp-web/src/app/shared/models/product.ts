export interface Product {
  id: string;
  code: string;
  description: string;
  stockQuantity: number;
}

export interface CreateProductRequest {
  code: string;
  description: string;
  stockQuantity: number;
}

export interface IncreaseStockRequest {
  items: IncreaseStockItemRequest[];
}

export interface IncreaseStockItemRequest {
  productId: string;
  quantity: number;
}

export interface IncreaseStockResponse {
  message: string;
  items: IncreaseStockItemResponse[];
}

export interface IncreaseStockItemResponse {
  productId: string;
  code: string;
  stockQuantity: number;
}