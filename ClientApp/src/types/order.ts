export interface OrderItem {
  id: string;
  gameTitle?: string;
  sellerName?: string;
  key?: string;
  gamePrice?: number;
  status?: string;
  averageRating?: number;
  totalAmount?: number;
  orderDate?: string;
}
export interface Order {
  id: string;
  status: string;
  orderDate: string;
  totalPrice: number;
  items: OrderItem[];
}
