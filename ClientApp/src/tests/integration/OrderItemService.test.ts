import { vi, describe, it, expect, beforeEach } from 'vitest';
import { OrderItemService } from '@/services/OrderItemService';

const { mockGet } = vi.hoisted(() => {
  return {
    mockGet: vi.fn(),
  };
});

vi.mock('axios', () => ({
  default: {
    create: vi.fn(() => ({
      get: mockGet,
    })),
  },
}));

describe('OrderItemService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should fetch order items by game ID', async () => {
    const mockItems = [{ id: '1' }];
    mockGet.mockResolvedValue({ data: { items: mockItems } });

    const result = await OrderItemService.getByGameId('game123');

    expect(mockGet).toHaveBeenCalledWith('/order-items', { params: { gameId: 'game123' } });
    expect(result).toEqual(mockItems);
  });

  it('should fetch key by ID', async () => {
    const mockKey = 'key123';
    mockGet.mockResolvedValue({ data: { item: mockKey } });

    const result = await OrderItemService.getKeyById('item123');

    expect(mockGet).toHaveBeenCalledWith('/order-items/item123/key');
    expect(result).toEqual(mockKey);
  });

  it('should handle empty items default', async () => {
    mockGet.mockResolvedValue({ data: {} });

    const result = await OrderItemService.getByGameId('game123');

    expect(result).toEqual([]);
  });

  it('should handle fetch error', async () => {
    const mockError = new Error('Network error');
    mockGet.mockRejectedValue(mockError);

    await expect(OrderItemService.getByGameId('game123')).rejects.toThrow('Network error');
  });
});
