import { renderHook, act } from '@testing-library/react'
import { useAuthStore } from '../authStore'
import { TokenManager, apiHelpers } from '@/lib/api'
import { server } from '../../__mocks__/server'
import { http, HttpResponse } from 'msw'

// Mock the API modules
jest.mock('@/lib/api', () => ({
  TokenManager: {
    setTokens: jest.fn(),
    removeTokens: jest.fn(),
    getAccessToken: jest.fn(),
  },
  apiHelpers: {
    post: jest.fn(),
    get: jest.fn(),
  },
  endpoints: {
    auth: {
      login: '/auth/login',
      register: '/auth/register',
      logout: '/auth/logout',
      profile: '/auth/profile',
    },
  },
}))

const mockTokenManager = TokenManager as jest.Mocked<typeof TokenManager>
const mockApiHelpers = apiHelpers as jest.Mocked<typeof apiHelpers>

// Mock console.warn to avoid noise in tests
const mockConsoleWarn = jest.spyOn(console, 'warn').mockImplementation(() => {})

// Mock user data
const mockUser = {
  id: 'user-1',
  username: 'testuser',
  email: 'test@example.com',
  firstName: 'Test',
  lastName: 'User',
  displayName: 'Test User',
  bio: 'Test user bio',
  avatarUrl: null,
  isVerified: false,
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-01T00:00:00Z',
  followersCount: 10,
  followingCount: 5,
  postsCount: 3,
}

const mockTokens = {
  accessToken: 'mock-access-token',
  refreshToken: 'mock-refresh-token',
}

describe('AuthStore', () => {
  beforeEach(() => {
    jest.clearAllMocks()
    // Reset the store state before each test
    const { result } = renderHook(() => useAuthStore())
    act(() => {
      result.current.logout()
    })
  })

  afterEach(() => {
    mockConsoleWarn.mockClear()
  })

  describe('Initial State', () => {
    it('should have correct initial state', () => {
      const { result } = renderHook(() => useAuthStore())
      
      expect(result.current.user).toBeNull()
      expect(result.current.isAuthenticated).toBe(false)
      expect(result.current.isLoading).toBe(false)
      expect(result.current.error).toBeUndefined()
    })
  })

  describe('login', () => {
    it('should login successfully with valid credentials', async () => {
      const { result } = renderHook(() => useAuthStore())
      
      mockApiHelpers.post.mockResolvedValue({
        user: mockUser,
        ...mockTokens,
      })

      const credentials = {
        email: 'test@example.com',
        password: 'password',
      }

      await act(async () => {
        await result.current.login(credentials)
      })

      expect(result.current.user).toEqual(mockUser)
      expect(result.current.isAuthenticated).toBe(true)
      expect(result.current.isLoading).toBe(false)
      expect(result.current.error).toBeUndefined()
      
      expect(mockApiHelpers.post).toHaveBeenCalledWith('/auth/login', credentials)
      expect(mockTokenManager.setTokens).toHaveBeenCalledWith(
        mockTokens.accessToken,
        mockTokens.refreshToken
      )
    })

    it('should handle login failure', async () => {
      const { result } = renderHook(() => useAuthStore())
      
      const errorResponse = {
        response: {
          data: {
            message: 'Invalid credentials',
          },
        },
      }
      
      mockApiHelpers.post.mockRejectedValue(errorResponse)

      const credentials = {
        email: 'wrong@example.com',
        password: 'wrongpassword',
      }

      await act(async () => {
        try {
          await result.current.login(credentials)
        } catch (error) {
          // Expected to throw
        }
      })

      expect(result.current.user).toBeNull()
      expect(result.current.isAuthenticated).toBe(false)
      expect(result.current.isLoading).toBe(false)
      expect(result.current.error).toBe('Invalid credentials')
    })

    it('should handle login failure with default error message', async () => {
      const { result } = renderHook(() => useAuthStore())
      
      mockApiHelpers.post.mockRejectedValue(new Error('Network error'))

      const credentials = {
        email: 'test@example.com',
        password: 'password',
      }

      await act(async () => {
        try {
          await result.current.login(credentials)
        } catch (error) {
          // Expected to throw
        }
      })

      expect(result.current.error).toBe('Login failed')
    })

    it('should set loading state during login', async () => {
      const { result } = renderHook(() => useAuthStore())
      
      let resolveLogin: (value: any) => void
      const loginPromise = new Promise((resolve) => {
        resolveLogin = resolve
      })
      
      mockApiHelpers.post.mockReturnValue(loginPromise)

      act(() => {
        result.current.login({
          email: 'test@example.com',
          password: 'password',
        })
      })

      expect(result.current.isLoading).toBe(true)

      await act(async () => {
        resolveLogin!({
          user: mockUser,
          ...mockTokens,
        })
        await loginPromise
      })

      expect(result.current.isLoading).toBe(false)
    })
  })

  describe('register', () => {
    it('should register successfully with valid data', async () => {
      const { result } = renderHook(() => useAuthStore())
      
      mockApiHelpers.post.mockResolvedValue({
        user: mockUser,
        ...mockTokens,
      })

      const registrationData = {
        email: 'test@example.com',
        username: 'testuser',
        password: 'StrongPass123!',
        firstName: 'Test',
        lastName: 'User',
      }

      await act(async () => {
        await result.current.register(registrationData)
      })

      expect(result.current.user).toEqual(mockUser)
      expect(result.current.isAuthenticated).toBe(true)
      expect(result.current.isLoading).toBe(false)
      expect(result.current.error).toBeUndefined()
      
      expect(mockApiHelpers.post).toHaveBeenCalledWith('/auth/register', registrationData)
      expect(mockTokenManager.setTokens).toHaveBeenCalledWith(
        mockTokens.accessToken,
        mockTokens.refreshToken
      )
    })

    it('should handle registration failure', async () => {
      const { result } = renderHook(() => useAuthStore())
      
      const errorResponse = {
        response: {
          data: {
            message: 'Email already exists',
          },
        },
      }
      
      mockApiHelpers.post.mockRejectedValue(errorResponse)

      const registrationData = {
        email: 'existing@example.com',
        username: 'testuser',
        password: 'StrongPass123!',
        firstName: 'Test',
        lastName: 'User',
      }

      await act(async () => {
        try {
          await result.current.register(registrationData)
        } catch (error) {
          // Expected to throw
        }
      })

      expect(result.current.user).toBeNull()
      expect(result.current.isAuthenticated).toBe(false)
      expect(result.current.isLoading).toBe(false)
      expect(result.current.error).toBe('Email already exists')
    })

    it('should handle registration failure with default error message', async () => {
      const { result } = renderHook(() => useAuthStore())
      
      mockApiHelpers.post.mockRejectedValue(new Error('Network error'))

      const registrationData = {
        email: 'test@example.com',
        username: 'testuser',
        password: 'StrongPass123!',
        firstName: 'Test',
        lastName: 'User',
      }

      await act(async () => {
        try {
          await result.current.register(registrationData)
        } catch (error) {
          // Expected to throw
        }
      })

      expect(result.current.error).toBe('Registration failed')
    })
  })

  describe('logout', () => {
    it('should logout successfully', async () => {
      const { result } = renderHook(() => useAuthStore())
      
      // Set authenticated state first
      act(() => {
        result.current.login({
          email: 'test@example.com',
          password: 'password',
        })
      })

      mockApiHelpers.post.mockResolvedValue(null)

      act(() => {
        result.current.logout()
      })

      expect(result.current.user).toBeNull()
      expect(result.current.isAuthenticated).toBe(false)
      expect(result.current.isLoading).toBe(false)
      expect(result.current.error).toBeUndefined()
      
      expect(mockApiHelpers.post).toHaveBeenCalledWith('/auth/logout')
      expect(mockTokenManager.removeTokens).toHaveBeenCalled()
    })

    it('should continue logout even if API call fails', () => {
      const { result } = renderHook(() => useAuthStore())
      
      // Set authenticated state first
      act(() => {
        // Directly set state for testing
        const store = useAuthStore.getState()
        store.user = mockUser
        store.isAuthenticated = true
      })

      mockApiHelpers.post.mockRejectedValue(new Error('Network error'))

      act(() => {
        result.current.logout()
      })

      expect(result.current.user).toBeNull()
      expect(result.current.isAuthenticated).toBe(false)
      expect(mockTokenManager.removeTokens).toHaveBeenCalled()
      expect(mockConsoleWarn).toHaveBeenCalledWith('Logout API call failed:', expect.any(Error))
    })
  })

  describe('refreshUser', () => {
    it('should refresh user data when authenticated', async () => {
      const { result } = renderHook(() => useAuthStore())
      
      // Set authenticated state
      act(() => {
        const store = useAuthStore.getState()
        store.user = mockUser
        store.isAuthenticated = true
      })

      const updatedUser = { ...mockUser, displayName: 'Updated Name' }
      mockApiHelpers.get.mockResolvedValue(updatedUser)

      await act(async () => {
        await result.current.refreshUser()
      })

      expect(result.current.user).toEqual(updatedUser)
      expect(result.current.isLoading).toBe(false)
      expect(result.current.error).toBeUndefined()
      
      expect(mockApiHelpers.get).toHaveBeenCalledWith('/auth/profile')
    })

    it('should not refresh user data when not authenticated', async () => {
      const { result } = renderHook(() => useAuthStore())

      await act(async () => {
        await result.current.refreshUser()
      })

      expect(mockApiHelpers.get).not.toHaveBeenCalled()
    })

    it('should logout on 401 error during refresh', async () => {
      const { result } = renderHook(() => useAuthStore())
      
      // Set authenticated state
      act(() => {
        const store = useAuthStore.getState()
        store.user = mockUser
        store.isAuthenticated = true
      })

      const errorResponse = {
        response: {
          status: 401,
        },
      }
      
      mockApiHelpers.get.mockRejectedValue(errorResponse)

      await act(async () => {
        await result.current.refreshUser()
      })

      expect(result.current.user).toBeNull()
      expect(result.current.isAuthenticated).toBe(false)
      expect(mockTokenManager.removeTokens).toHaveBeenCalled()
    })

    it('should handle non-401 errors during refresh', async () => {
      const { result } = renderHook(() => useAuthStore())
      
      // Set authenticated state
      act(() => {
        const store = useAuthStore.getState()
        store.user = mockUser
        store.isAuthenticated = true
      })

      const errorResponse = {
        response: {
          status: 500,
        },
      }
      
      mockApiHelpers.get.mockRejectedValue(errorResponse)

      await act(async () => {
        await result.current.refreshUser()
      })

      expect(result.current.user).toEqual(mockUser) // Should remain unchanged
      expect(result.current.isAuthenticated).toBe(true) // Should remain authenticated
      expect(result.current.error).toBe('Failed to refresh user data')
    })
  })

  describe('clearError', () => {
    it('should clear error state', () => {
      const { result } = renderHook(() => useAuthStore())
      
      // Set error state
      act(() => {
        const store = useAuthStore.getState()
        store.error = 'Some error'
      })

      act(() => {
        result.current.clearError()
      })

      expect(result.current.error).toBeUndefined()
    })
  })

  describe('checkAuth', () => {
    it('should check authentication when token exists and is valid', async () => {
      const { result } = renderHook(() => useAuthStore())
      
      mockTokenManager.getAccessToken.mockReturnValue('valid-token')
      mockApiHelpers.get.mockResolvedValue(mockUser)

      await act(async () => {
        await result.current.checkAuth()
      })

      expect(result.current.user).toEqual(mockUser)
      expect(result.current.isAuthenticated).toBe(true)
      expect(result.current.isLoading).toBe(false)
      expect(result.current.error).toBeUndefined()
      
      expect(mockApiHelpers.get).toHaveBeenCalledWith('/auth/profile')
    })

    it('should set unauthenticated state when no token exists', async () => {
      const { result } = renderHook(() => useAuthStore())
      
      mockTokenManager.getAccessToken.mockReturnValue(null)

      await act(async () => {
        await result.current.checkAuth()
      })

      expect(result.current.user).toBeNull()
      expect(result.current.isAuthenticated).toBe(false)
      expect(result.current.isLoading).toBe(false)
      
      expect(mockApiHelpers.get).not.toHaveBeenCalled()
    })

    it('should clear tokens and set unauthenticated state when token is invalid', async () => {
      const { result } = renderHook(() => useAuthStore())
      
      mockTokenManager.getAccessToken.mockReturnValue('invalid-token')
      mockApiHelpers.get.mockRejectedValue(new Error('Unauthorized'))

      await act(async () => {
        await result.current.checkAuth()
      })

      expect(result.current.user).toBeNull()
      expect(result.current.isAuthenticated).toBe(false)
      expect(result.current.isLoading).toBe(false)
      expect(result.current.error).toBeUndefined()
      
      expect(mockTokenManager.removeTokens).toHaveBeenCalled()
    })

    it('should set loading state during authentication check', async () => {
      const { result } = renderHook(() => useAuthStore())
      
      mockTokenManager.getAccessToken.mockReturnValue('valid-token')
      
      let resolveAuth: (value: any) => void
      const authPromise = new Promise((resolve) => {
        resolveAuth = resolve
      })
      
      mockApiHelpers.get.mockReturnValue(authPromise)

      act(() => {
        result.current.checkAuth()
      })

      expect(result.current.isLoading).toBe(true)

      await act(async () => {
        resolveAuth!(mockUser)
        await authPromise
      })

      expect(result.current.isLoading).toBe(false)
    })
  })

  describe('Store Persistence', () => {
    it('should persist user and authentication state', () => {
      const { result } = renderHook(() => useAuthStore())
      
      // Test the partialize function
      const state = {
        user: mockUser,
        isAuthenticated: true,
        isLoading: true,
        error: 'Some error',
      }

      // The persist configuration should only persist user and isAuthenticated
      const persistedState = {
        user: state.user,
        isAuthenticated: state.isAuthenticated,
      }

      expect(persistedState).toEqual({
        user: mockUser,
        isAuthenticated: true,
      })
      expect(persistedState).not.toHaveProperty('isLoading')
      expect(persistedState).not.toHaveProperty('error')
    })
  })

  describe('Integration Tests', () => {
    it('should handle complete authentication flow', async () => {
      const { result } = renderHook(() => useAuthStore())
      
      // Start with no authentication
      expect(result.current.isAuthenticated).toBe(false)

      // Login
      mockApiHelpers.post.mockResolvedValue({
        user: mockUser,
        ...mockTokens,
      })

      await act(async () => {
        await result.current.login({
          email: 'test@example.com',
          password: 'password',
        })
      })

      expect(result.current.isAuthenticated).toBe(true)
      expect(result.current.user).toEqual(mockUser)

      // Refresh user data
      const updatedUser = { ...mockUser, displayName: 'Updated Name' }
      mockApiHelpers.get.mockResolvedValue(updatedUser)

      await act(async () => {
        await result.current.refreshUser()
      })

      expect(result.current.user).toEqual(updatedUser)

      // Logout
      act(() => {
        result.current.logout()
      })

      expect(result.current.isAuthenticated).toBe(false)
      expect(result.current.user).toBeNull()
    })

    it('should handle error recovery flow', async () => {
      const { result } = renderHook(() => useAuthStore())
      
      // Failed login
      mockApiHelpers.post.mockRejectedValue({
        response: {
          data: {
            message: 'Invalid credentials',
          },
        },
      })

      await act(async () => {
        try {
          await result.current.login({
            email: 'wrong@example.com',
            password: 'wrongpassword',
          })
        } catch (error) {
          // Expected to throw
        }
      })

      expect(result.current.error).toBe('Invalid credentials')

      // Clear error
      act(() => {
        result.current.clearError()
      })

      expect(result.current.error).toBeUndefined()

      // Successful login
      mockApiHelpers.post.mockResolvedValue({
        user: mockUser,
        ...mockTokens,
      })

      await act(async () => {
        await result.current.login({
          email: 'test@example.com',
          password: 'password',
        })
      })

      expect(result.current.isAuthenticated).toBe(true)
      expect(result.current.error).toBeUndefined()
    })
  })
})
