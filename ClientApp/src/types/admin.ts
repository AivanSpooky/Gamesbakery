export interface AdminEntity {
  id: string;
  username: string;
  role: string;
  isBlocked: boolean;
  email?: string;
  avgRating?: number;
  type: 'User' | 'Seller';
}
