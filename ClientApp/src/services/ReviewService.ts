import axios from 'axios';

const api = axios.create({ baseURL: '/api/v2' });

export const ReviewService = {
  async getByGameId(gameId: string) {
    const res = await api.get('/reviews/game', { params: { gameId } });
    return res.data.items || []; // Lowercase, default empty
  },
  async getBySellerId(sellerId: string) {
    const res = await api.get('/reviews', { params: { sellerId } });
    return res.data.items || [];
  },
  async create(gameId: string, rating: number, text: string) {
    await api.post('/reviews', { gameId, rating, text });
  },
};
