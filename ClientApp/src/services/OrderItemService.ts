import axios from 'axios';

const api = axios.create({ baseURL: '/api/v2' });

export const OrderItemService = {
  async getByGameId(gameId: string) {
    const res = await api.get('/order-items', { params: { gameId } });
    return res.data.items || []; // Use lowercase "items" and default to empty array
  },
  async getKeyById(itemId: string) {
    const res = await api.get(`/order-items/${itemId}/key`);
    return res.data.item; // Unwrap to return the key string directly
  },
  // Add other methods if needed
};
