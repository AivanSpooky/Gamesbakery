import { vi, describe, it, expect, beforeEach } from 'vitest';
import { OrderService } from '@/services/OrderService';
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

describe('OrderService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockedAuthStore.mockReturnValue({ userId: '123' });
  });

  it('should fetch all orders', async () => {
    const mockResponse = { data: { items: [], total: 0 } };
    mockGet.mockResolvedValue(mockResponse);

    const result = await OrderService.getAll({ page: 1, limit: 12 });

    expect(mockGet).toHaveBeenCalledWith('/users/123/orders', { params: { page: 1, limit: 12 } });
    expect(result).toEqual(mockResponse.data);
  });

  it('should fetch order by ID', async () => {
    const mockOrder = { id: 'order1' };
    mockGet.mockResolvedValue({ data: { item: mockOrder } });

    const result = await OrderService.getById('order1');

    expect(mockGet).toHaveBeenCalledWith('/orders/order1');
    expect(result).toEqual(mockOrder);
  });

  it('should checkout cart', async () => {
    const mockOrderId = 'newOrder';
    mockPost.mockResolvedValue({ data: { item: { orderId: mockOrderId } } });

    const result = await OrderService.checkout(['item1', 'item2']);

    expect(mockPost).toHaveBeenCalledWith('/users/123/orders', { cartItemIds: ['item1', 'item2'] });
    expect(result).toEqual(mockOrderId);
  });

  it('should handle checkout error', async () => {
    const mockError = new Error('Checkout failed');
    mockPost.mockRejectedValue(mockError);

    await expect(OrderService.checkout([])).rejects.toThrow('Checkout failed');
  });
});
