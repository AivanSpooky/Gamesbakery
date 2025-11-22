import axios from 'axios';
import { useAuthStore } from '@/stores/auth';
const api = axios.create({ baseURL: '/api/v2' });
export const CartService = {
  async getItems() {
    const authStore = useAuthStore();
    const res = await api.get(`/users/${authStore.userId}/cart-items`);
    return res.data.items; // PaginatedResponse, use .items
  },
  async addItem(orderItemId: string) {
    const authStore = useAuthStore();
    await api.post(`/users/${authStore.userId}/cart-items`, { orderItemId });
  },
  async removeItem(orderItemId: string) {
    const authStore = useAuthStore();
    await api.delete(`/users/${authStore.userId}/cart-items/${orderItemId}`);
  },
  async clear() {
    const authStore = useAuthStore();
    await api.delete(`/users/${authStore.userId}/cart-items`);
  },
  getTotal(items: any[]) {
    return items.reduce((sum, item) => sum + item.gamePrice, 0); // Updated field name
  },
};
