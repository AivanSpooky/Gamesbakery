import axios from 'axios';
import { useAuthStore } from '@/stores/auth';
const api = axios.create({ baseURL: '/api/v2' });
export const UserService = {
  async getProfile() {
    const authStore = useAuthStore();
    const res = await api.get(`/users/${authStore.userId}`);
    return res.data.item;
  },
  async updateBalance(amount: number) {
    const authStore = useAuthStore();
    await api.patch(`/users/${authStore.userId}`, { balance: amount });
  },
  async getAllUsers(page = 1, limit = 1000, getAll = true) {
    const params = {
      page,
      limit,
      ...(getAll && { getAll: true })
    };
    const res = await api.get('/users/admin', { params });
    return res.data.items || res.data;
  },
  async getById(id: string) {
    const res = await api.get(`/users/${id}`);
    return res.data.item;
  },
  async banUser(id: string) {
    await api.post(`/users/admin/${id}/ban`);
  },
  async unbanUser(id: string) {
    await api.post(`/users/admin/${id}/unban`);
  },
};
