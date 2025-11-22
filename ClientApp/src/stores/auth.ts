import { defineStore } from 'pinia';

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: localStorage.getItem('jwtToken') || null,
    role: localStorage.getItem('role') || '',
    username: localStorage.getItem('username') || '',
    userId: localStorage.getItem('userId') || '',
    sellerId: localStorage.getItem('sellerId') || null,
  }),
  actions: {
    login(token: string, role: string, username: string, userId: string, sellerId: string | null = null) {
      this.token = token;
      this.role = role;
      this.username = username;
      this.userId = userId;
      this.sellerId = sellerId;
      localStorage.setItem('sellerId', sellerId || '');
      localStorage.setItem('jwtToken', token);
      localStorage.setItem('role', role);
      localStorage.setItem('username', username);
      localStorage.setItem('userId', userId);
    },
    logout() {
      this.token = null;
      this.role = '';
      this.username = '';
      this.userId = '';
      this.sellerId = '';
      localStorage.clear();
    },
  },
  getters: {
    isAuthenticated: (state) => !!state.token,
  },
});
