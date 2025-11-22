import axios from 'axios';
import { useAuthStore } from '@/stores/auth';
const api = axios.create({ baseURL: '/api/v2' });
export const GiftService = {
  async getAll(type = 'all') {
    const authStore = useAuthStore();
    const res = await api.get(`/users/${authStore.userId}/gifts`, { params: { type } });
    return res.data.items; // Paginated
  },
  async create(recipientId: string, orderItemId: string) {
    const authStore = useAuthStore();
    await api.post(`/users/${authStore.userId}/gifts`, { recipientId, orderItemId });
  },
};
