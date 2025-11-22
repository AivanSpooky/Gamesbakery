import { vi, describe, it, expect, beforeEach } from 'vitest';
import { UserService } from '@/services/UserService';
import { useAuthStore } from '@/stores/auth';

const { mockGet, mockPatch, mockPost } = vi.hoisted(() => {
  return {
    mockGet: vi.fn(),
    mockPatch: vi.fn(),
    mockPost: vi.fn(),
  };
});

vi.mock('axios', () => ({
  default: {
    create: vi.fn(() => ({
      get: mockGet,
      patch: mockPatch,
      post: mockPost,
    })),
  },
}));

vi.mock('@/stores/auth', () => ({
  useAuthStore: vi.fn(() => ({ userId: '123' })),
}));

const mockedAuthStore = useAuthStore as any;

describe('UserService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockedAuthStore.mockReturnValue({ userId: '123' });
  });

  it('should fetch user profile', async () => {
    const mockProfile = { id: '123' };
    mockGet.mockResolvedValue({ data: { item: mockProfile } });

    const result = await UserService.getProfile();

    expect(mockGet).toHaveBeenCalledWith('/users/123');
    expect(result).toEqual(mockProfile);
  });

  it('should update balance', async () => {
    mockPatch.mockResolvedValue({});

    await UserService.updateBalance(100);

    expect(mockPatch).toHaveBeenCalledWith('/users/123', { balance: 100 });
  });

  it('should fetch all users', async () => {
    const mockUsers = [{ id: '1' }];
    mockGet.mockResolvedValue({ data: { items: mockUsers } });
    const result = await UserService.getAllUsers();
    expect(mockGet).toHaveBeenCalledWith('/users/admin', { params: { page: 1, limit: 1000, getAll: true } });
    expect(result).toEqual(mockUsers);
  });

  it('should fetch user by ID', async () => {
    const mockUser = { id: '456' };
    mockGet.mockResolvedValue({ data: { item: mockUser } });

    const result = await UserService.getById('456');

    expect(mockGet).toHaveBeenCalledWith('/users/456');
    expect(result).toEqual(mockUser);
  });

  it('should ban user', async () => {
    mockPost.mockResolvedValue({});

    await UserService.banUser('456');

    expect(mockPost).toHaveBeenCalledWith('/users/admin/456/ban');
  });

  it('should unban user', async () => {
    mockPost.mockResolvedValue({});

    await UserService.unbanUser('456');

    expect(mockPost).toHaveBeenCalledWith('/users/admin/456/unban');
  });

  it('should handle update error', async () => {
    const mockError = new Error('Update failed');
    mockPatch.mockRejectedValue(mockError);

    await expect(UserService.updateBalance(100)).rejects.toThrow('Update failed');
  });
});
