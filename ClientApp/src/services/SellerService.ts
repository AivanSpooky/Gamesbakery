import axios from 'axios';
import { useAuthStore } from '@/stores/auth';
const api = axios.create({ baseURL: '/api/v2' });
export const SellerService = {
  async getById(id: string) {
    const res = await api.get(`/sellers/${id}`);
    return res.data.item;
  },
  async getProfile() {
    const authStore = useAuthStore();
    const res = await api.get(`/sellers/${authStore.sellerId || authStore.userId}`);
    return res.data.item;
  },
  async getOrderItems(params = { page: 1, limit: 12 }) {
    const authStore = useAuthStore();
    const effectiveId = authStore.sellerId || authStore.userId;
    if (!effectiveId) throw new Error('No valid ID');
    const res = await api.get('/order-items', { params: { sellerId: effectiveId, ...params } });
    return res.data;
  },
  async createKey(gameId: string, key: string) {
    const authStore = useAuthStore();
    const effectiveId = authStore.sellerId || authStore.userId;
    if (!effectiveId) {
      throw new Error('No valid seller or user ID available');
    }
    await api.post(`/sellers/${effectiveId}/order-items`, { gameId, key });
  },
  async getAllSellers(page = 1, limit = 1000, getAll = true) {
    const params = {
      page,
      limit,
      ...(getAll && { getAll: true })
    };
    const res = await api.get('/sellers', { params });
    return res.data.items || [];
  },
  async resetRating(id: string) {
    await api.post(`/sellers/admin/${id}/reset-rating`);
  },
};
