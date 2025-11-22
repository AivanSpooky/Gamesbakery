import { vi, describe, it, expect, beforeEach } from 'vitest';
import { GiftService } from '@/services/GiftService';
import { useAuthStore } from '@/stores/auth';

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

vi.mock('@/stores/auth', () => ({
  useAuthStore: vi.fn(() => ({ userId: '123' })),
}));

const mockedAuthStore = useAuthStore as any;

describe('GiftService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockedAuthStore.mockReturnValue({ userId: '123' });
  });

  it('should fetch gifts successfully', async () => {
    const mockGifts = [{ id: '1' }];
    mockGet.mockResolvedValue({ data: { items: mockGifts } });

    const result = await GiftService.getAll('all');

    expect(mockGet).toHaveBeenCalledWith('/users/123/gifts', { params: { type: 'all' } });
    expect(result).toEqual(mockGifts);
  });

  it('should create a gift', async () => {
    mockPost.mockResolvedValue({});

    await GiftService.create('recip123', 'item123');

    expect(mockPost).toHaveBeenCalledWith('/users/123/gifts', { recipientId: 'recip123', orderItemId: 'item123' });
  });

  it('should handle fetch failure', async () => {
    const mockError = new Error('Network error');
    mockGet.mockRejectedValue(mockError);

    await expect(GiftService.getAll()).rejects.toThrow('Network error');
  });
});
