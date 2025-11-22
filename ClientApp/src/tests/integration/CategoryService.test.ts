import { vi, describe, it, expect, beforeEach } from 'vitest';
import { CategoryService } from '@/services/CategoryService';

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

describe('CategoryService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should fetch all categories successfully', async () => {
    const mockCategories = [{ id: '1', name: 'Action' }];
    mockGet.mockResolvedValue({ data: { items: mockCategories } });

    const result = await CategoryService.getAll();

    expect(mockGet).toHaveBeenCalledWith('/categories');
    expect(result).toEqual(mockCategories);
  });

  it('should handle fetch failure', async () => {
    const mockError = new Error('Network error');
    mockGet.mockRejectedValue(mockError);

    await expect(CategoryService.getAll()).rejects.toThrow('Network error');
  });
});
