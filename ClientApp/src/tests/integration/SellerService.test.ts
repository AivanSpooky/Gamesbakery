import { vi, describe, it, expect, beforeEach } from 'vitest';
import { SellerService } from '@/services/SellerService';
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
  useAuthStore: vi.fn(() => ({ userId: '123', sellerId: '456' })),
}));

const mockedAuthStore = useAuthStore as any;

describe('SellerService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockedAuthStore.mockReturnValue({ userId: '123', sellerId: '456' });
  });

  it('should fetch seller by ID', async () => {
    const mockSeller = { id: '456' };
    mockGet.mockResolvedValue({ data: { item: mockSeller } });

    const result = await SellerService.getById('456');

    expect(mockGet).toHaveBeenCalledWith('/sellers/456');
    expect(result).toEqual(mockSeller);
  });

  it('should fetch seller profile', async () => {
    const mockProfile = { id: '456' };
    mockGet.mockResolvedValue({ data: { item: mockProfile } });

    const result = await SellerService.getProfile();

    expect(mockGet).toHaveBeenCalledWith('/sellers/456');
    expect(result).toEqual(mockProfile);
  });

  it('should fetch order items for seller', async () => {
    const mockResponse = { data: { items: [] } };
    mockGet.mockResolvedValue(mockResponse);

    const result = await SellerService.getOrderItems({ page: 1, limit: 12 });

    expect(mockGet).toHaveBeenCalledWith('/order-items', { params: { sellerId: '456', page: 1, limit: 12 } });
    expect(result).toEqual(mockResponse.data);
  });

  it('should throw error on getOrderItems if no valid ID', async () => {
    mockedAuthStore.mockReturnValue({ userId: null, sellerId: null });

    await expect(SellerService.getOrderItems()).rejects.toThrow('No valid ID');
  });

  it('should create key for game', async () => {
    mockPost.mockResolvedValue({});

    await SellerService.createKey('game123', 'key123');

    expect(mockPost).toHaveBeenCalledWith('/sellers/456/order-items', { gameId: 'game123', key: 'key123' });
  });

  it('should throw error on createKey if no valid ID', async () => {
    mockedAuthStore.mockReturnValue({ userId: null, sellerId: null });

    await expect(SellerService.createKey('game123', 'key123')).rejects.toThrow('No valid seller or user ID available');
  });

  it('should fetch all sellers', async () => {
    const mockSellers = [{ id: '1' }];
    mockGet.mockResolvedValue({ data: { items: mockSellers } });
    const result = await SellerService.getAllSellers();
    expect(mockGet).toHaveBeenCalledWith('/sellers', { params: { page: 1, limit: 1000, getAll: true } });
    expect(result).toEqual(mockSellers);
  });

  it('should handle empty sellers default', async () => {
    mockGet.mockResolvedValue({ data: {} });

    const result = await SellerService.getAllSellers();

    expect(result).toEqual([]);
  });

  it('should reset rating', async () => {
    mockPost.mockResolvedValue({});

    await SellerService.resetRating('seller123');

    expect(mockPost).toHaveBeenCalledWith('/sellers/admin/seller123/reset-rating');
  });

  it('should handle fetch error in getById', async () => {
    const mockError = new Error('Network error');
    mockGet.mockRejectedValue(mockError);

    await expect(SellerService.getById('456')).rejects.toThrow('Network error');
  });
});
