import axios from 'axios';
const api = axios.create({ baseURL: '/api/v2' });
export const CategoryService = {
  async getAll() {
    const res = await api.get('/categories');
    return res.data.items; // Paginated
  },
  // Add more if needed
};
