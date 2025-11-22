import axios from 'axios';
const api = axios.create({ baseURL: '/api/v2' });
export const GameService = {
  async getAll(genre?: string, minPrice?: number, maxPrice?: number, page = 1, limit = 2) {
    const params: any = { page, limit };
    if (genre) params.genre = genre;
    if (minPrice !== undefined) params.minPrice = minPrice;
    if (maxPrice !== undefined) params.maxPrice = maxPrice;
    const res = await api.get('/games', { params });
    return res.data.items; // Full PaginatedResponse
  },
  async getById(id: string, includeOrderItems = false) {
    const res = await api.get(`/games/${id}`, { params: { includeOrderItems } });
    return res.data.item; // SingleResponse.item
  },
  async create(categoryId: string, title: string, price: number, releaseDate: string, description: string, publisher: string) {
    const res = await api.post('/games', { categoryId, title, price, releaseDate, description, originalPublisher: publisher });
    return res.data.item;
  },
};
