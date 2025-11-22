import { vi, describe, it, expect, beforeEach } from 'vitest';
import { AuthService } from '@/services/AuthService';
import { useAuthStore } from '@/stores/auth';

const { mockPost, mockDelete, mockGet, mockPut, mockRequestUse } = vi.hoisted(() => {
  return {
    mockPost: vi.fn(),
    mockDelete: vi.fn(),
    mockGet: vi.fn(),
    mockPut: vi.fn(),
    mockRequestUse: vi.fn(),
  };
});

vi.mock('axios', () => ({
  default: {
    create: vi.fn(() => ({
      interceptors: {
        request: { use: mockRequestUse, eject: vi.fn() },
        response: { use: vi.fn(), eject: vi.fn() }
      },
      get: mockGet,
      post: mockPost,
      delete: mockDelete,
      put: mockPut,
    })),
  },
}));

vi.mock('@/stores/auth', () => ({
  useAuthStore: vi.fn(() => ({ token: null, logout: vi.fn(), userId: '123', sellerId: '456' })),
}));

// Capture the registered interceptor callback after module import
const interceptorFn = mockRequestUse.mock.calls[0][0];

const mockedAuthStore = useAuthStore as any;

describe('AuthService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockedAuthStore.mockReturnValue({ token: null, logout: vi.fn(), userId: '123', sellerId: '456' });
  });

  it('should login successfully and return user data', async () => {
    const mockResponse = { data: { item: { token: 'fake-token', role: 'user', userId: '123', sellerId: '456' } } };
    mockPost.mockResolvedValue(mockResponse);

    const result = await AuthService.login('testuser', 'testpass');

    expect(mockPost).toHaveBeenCalledWith('/auth/login', { username: 'testuser', password: 'testpass' });
    expect(result).toEqual(mockResponse.data.item);
  });

  it('should handle login failure', async () => {
    const mockError = new Error('Invalid credentials');
    mockPost.mockRejectedValue(mockError);

    await expect(AuthService.login('wronguser', 'wrongpass')).rejects.toThrow('Invalid credentials');
    expect(mockPost).toHaveBeenCalledTimes(1);
  });

  it('should register a new user successfully', async () => {
    const mockResponse = { data: { success: true } };
    mockPost.mockResolvedValue(mockResponse);

    const result = await AuthService.register('newuser', 'email@example.com', 'newpass', 'USA');

    expect(mockPost).toHaveBeenCalledWith('/auth/register', {
      username: 'newuser',
      email: 'email@example.com',
      password: 'newpass',
      country: 'USA',
    });
    expect(result).toEqual(mockResponse.data);
  });

  it('should call logout on the auth store', () => {
    const mockLogout = vi.fn();
    mockedAuthStore.mockReturnValue({ logout: mockLogout });

    AuthService.logout();

    expect(mockLogout).toHaveBeenCalledTimes(1);
  });

  it('should add Authorization header if token exists', async () => {
    mockedAuthStore.mockReturnValue({ token: 'fake-token' });
    const mockResponse = { data: { item: { token: 'fake-token', role: 'user', userId: '123', sellerId: '456' } } };
    mockPost.mockResolvedValue(mockResponse);

    await AuthService.login('user', 'pass');

    const config = { headers: {} };
    const modifiedConfig = interceptorFn(config);
    expect(modifiedConfig.headers.Authorization).toBe('Bearer fake-token');
  });
});
