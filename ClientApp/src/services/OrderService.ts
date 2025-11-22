import axios from 'axios';
import { useAuthStore } from '@/stores/auth';

const api = axios.create({ baseURL: '/api/v2' });

export const OrderService = {
  // OrderService
  async getAll(params = { page: 1, limit: 12 }) {
    const authStore = useAuthStore();
    const res = await api.get(`/users/${authStore.userId}/orders`, { params });
    return res.data;
  },
  async getById(id: string) {
    const res = await api.get(`/orders/${id}`);
    return res.data.item;
  },
  async checkout(cartItemIds: string[]) {
    const authStore = useAuthStore();
    const res = await api.post(`/users/${authStore.userId}/orders`, { cartItemIds });
    return res.data.item.orderId;
  },
};
