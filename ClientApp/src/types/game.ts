export interface GameOrderItem {
  id: string;
  sellerName: string;
  sellerId?: string;
  averageRating?: number;
}

export interface Game {
  id: string;
  isForSale: boolean;
  title: string;
  price: number;
  description?: string;
  releaseDate?: Date;
  originalPublisher?: string;
  categoryId?: string;
  averageRating?: number;
}
