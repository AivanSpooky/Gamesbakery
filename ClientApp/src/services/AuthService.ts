import axios from 'axios';
import { useAuthStore } from '@/stores/auth';

const api = axios.create({ baseURL: '/api/v2' });

api.interceptors.request.use((config) => {
  const authStore = useAuthStore();
  if (authStore.token) {
    config.headers.Authorization = `Bearer ${authStore.token}`;
  }
  return config;
});

export const AuthService = {
  async login(username: string, password: string) {
    const res = await api.post('/auth/login', { username, password });
    return res.data.item;  // Return the nested 'item' directly: { token, role, userId, sellerId }
  },
  async register(username: string, email: string, password: string, country: string) {
    const res = await api.post('/auth/register', { username, email, password, country });
    return res.data;
  },
  logout() {
    const authStore = useAuthStore();
    authStore.logout();
  },
};
