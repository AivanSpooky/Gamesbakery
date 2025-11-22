import { vi, describe, it, expect, beforeEach } from 'vitest';
import { GameService } from '@/services/GameService';

const { mockGet, mockPost } = vi.hoisted(() => {
  return {
    mockGet: vi.fn(),
    mockPost: vi.fn(),
  };
});

vi.mock('axios', () => ({
  default: {
    create: vi.fn(() => ({
      get: mockGet,
      post: mockPost,
    })),
  },
}));

describe('GameService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should fetch all games with filters', async () => {
    const mockResponse = { data: { items: [], totalCount: 0 } };
    mockGet.mockResolvedValue(mockResponse);
    const result = await GameService.getAll('action', 10, 50, 2, 10);
    expect(mockGet).toHaveBeenCalledWith('/games', {
      params: { genre: 'action', minPrice: 10, maxPrice: 50, page: 2, limit: 10 },
    });
    expect(result).toEqual(mockResponse.data);
  });

  it('should fetch game by ID', async () => {
    const mockGame = { id: '1', title: 'Game' };
    mockGet.mockResolvedValue({ data: { item: mockGame } });

    const result = await GameService.getById('1', true);

    expect(mockGet).toHaveBeenCalledWith('/games/1', { params: { includeOrderItems: true } });
    expect(result).toEqual(mockGame);
  });

  it('should create a new game', async () => {
    const mockCreated = { id: 'new' };
    mockPost.mockResolvedValue({ data: { item: mockCreated } });

    const result = await GameService.create('cat1', 'New Game', 20, '2023-01-01', 'Desc', 'Pub');

    expect(mockPost).toHaveBeenCalledWith('/games', {
      categoryId: 'cat1',
      title: 'New Game',
      price: 20,
      releaseDate: '2023-01-01',
      description: 'Desc',
      originalPublisher: 'Pub',
    });
    expect(result).toEqual(mockCreated);
  });

  it('should handle fetch error', async () => {
    const mockError = new Error('Network error');
    mockGet.mockRejectedValue(mockError);

    await expect(GameService.getAll()).rejects.toThrow('Network error');
  });
});
