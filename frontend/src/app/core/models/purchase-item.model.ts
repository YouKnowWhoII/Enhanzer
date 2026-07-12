export interface PurchaseItem {
  item: string;
  batch: string;
  standardCost: number;
  standardPrice: number;
  quantity: number;
  discount: number;
  totalCost: number;
  totalSelling: number;
}

export interface PurchaseBillSaveRequest {
  items: PurchaseItem[];
}

export interface PurchaseBillSaveResponse {
  id: number;
  createdAtUtc: string;
  totalItems: number;
  totalQuantity: number;
  totalCost: number;
  totalSelling: number;
}
