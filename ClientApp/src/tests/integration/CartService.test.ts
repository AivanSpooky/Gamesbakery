import { vi, describe, it, expect, beforeEach } from 'vitest';
import { CartService } from '@/services/CartService';
import { useAuthStore } from '@/stores/auth';

const { mockGet, mockPost, mockDelete } = vi.hoisted(() => {
  return {
    mockGet: vi.fn(),
    mockPost: vi.fn(),
    mockDelete: vi.fn(),
  };
});

vi.mock('axios', () => ({
  default: {
    create: vi.fn(() => ({
      get: mockGet,
      post: mockPost,
      delete: mockDelete,
    })),
  },
}));

vi.mock('@/stores/auth', () => ({
  useAuthStore: vi.fn(() => ({ userId: '123' })),
}));

const mockedAuthStore = useAuthStore as any;

describe('CartService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockedAuthStore.mockReturnValue({ userId: '123' });
  });

  it('should fetch cart items successfully', async () => {
    const mockItems = [{ id: '1', gamePrice: 10 }];
    mockGet.mockResolvedValue({ data: { items: mockItems } });

    const result = await CartService.getItems();

    expect(mockGet).toHaveBeenCalledWith('/users/123/cart-items');
    expect(result).toEqual(mockItems);
  });

  it('should handle fetch failure', async () => {
    const mockError = new Error('Network error');
    mockGet.mockRejectedValue(mockError);

    await expect(CartService.getItems()).rejects.toThrow('Network error');
  });

  it('should add item to cart', async () => {
    mockPost.mockResolvedValue({});

    await CartService.addItem('item123');

    expect(mockPost).toHaveBeenCalledWith('/users/123/cart-items', { orderItemId: 'item123' });
  });

  it('should remove item from cart', async () => {
    mockDelete.mockResolvedValue({});

    await CartService.removeItem('item123');

    expect(mockDelete).toHaveBeenCalledWith('/users/123/cart-items/item123');
  });

  it('should clear cart', async () => {
    mockDelete.mockResolvedValue({});

    await CartService.clear();

    expect(mockDelete).toHaveBeenCalledWith('/users/123/cart-items');
  });

  it('should calculate total correctly', () => {
    const items = [{ gamePrice: 10 }, { gamePrice: 20 }];
    expect(CartService.getTotal(items)).toBe(30);
  });

  it('should return 0 for empty cart total', () => {
    expect(CartService.getTotal([])).toBe(0);
  });
});
