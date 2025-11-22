import { vi, describe, it, expect, beforeEach } from 'vitest';
import { ReviewService } from '@/services/ReviewService';

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

describe('ReviewService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should fetch reviews by game ID', async () => {
    const mockReviews = [{ rating: 5 }];
    mockGet.mockResolvedValue({ data: { items: mockReviews } });

    const result = await ReviewService.getByGameId('game123');

    expect(mockGet).toHaveBeenCalledWith('/reviews/game', { params: { gameId: 'game123' } });
    expect(result).toEqual(mockReviews);
  });

  it('should fetch reviews by seller ID', async () => {
    const mockReviews = [{ rating: 4 }];
    mockGet.mockResolvedValue({ data: { items: mockReviews } });

    const result = await ReviewService.getBySellerId('seller123');

    expect(mockGet).toHaveBeenCalledWith('/reviews', { params: { sellerId: 'seller123' } });
    expect(result).toEqual(mockReviews);
  });

  it('should create a review', async () => {
    mockPost.mockResolvedValue({});

    await ReviewService.create('game123', 5, 'Great game');

    expect(mockPost).toHaveBeenCalledWith('/reviews', { gameId: 'game123', rating: 5, text: 'Great game' });
  });

  it('should handle empty reviews default', async () => {
    mockGet.mockResolvedValue({ data: {} });

    const result = await ReviewService.getByGameId('game123');

    expect(result).toEqual([]);
  });

  it('should handle create error', async () => {
    const mockError = new Error('Create failed');
    mockPost.mockRejectedValue(mockError);

    await expect(ReviewService.create('game123', 5, 'Text')).rejects.toThrow('Create failed');
  });
});
