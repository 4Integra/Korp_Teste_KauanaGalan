export interface Invoice {
  id: string;
  number: number;
  status: string;
  createdAt: string;
  items: InvoiceItem[];
}

export interface InvoiceItem {
  id: string;
  productId: string;
  quantity: number;
}

export interface CreateInvoiceRequest {
  items: CreateInvoiceItemRequest[];
}

export interface CreateInvoiceItemRequest {
  productId: string;
  quantity: number;
}