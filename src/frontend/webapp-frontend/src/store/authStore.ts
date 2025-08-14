import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { AuthState, User, LoginRequest, RegisterRequest } from '@/types';
import { TokenManager, apiHelpers, endpoints } from '@/lib/api';

interface AuthStore extends AuthState {
  // Actions
  login: (credentials: LoginRequest) => Promise<void>;
  register: (data: RegisterRequest) => Promise<void>;
  logout: () => void;
  refreshUser: () => Promise<void>;
  clearError: () => void;
  checkAuth: () => Promise<void>;
}

export const useAuthStore = create<AuthStore>()(
  persist(
    (set, get) => ({
      // Initial state
      user: null,
      isAuthenticated: false,
      isLoading: false,
      error: undefined,

      // Login action
      login: async (credentials: LoginRequest) => {
        set({ isLoading: true, error: undefined });
        
        try {
          const response = await apiHelpers.post<{
            user: User;
            accessToken: string;
            refreshToken: string;
          }>(endpoints.auth.login, credentials);

          const { user, accessToken, refreshToken } = response;
          
          // Store tokens
          TokenManager.setTokens(accessToken, refreshToken);
          
          set({
            user,
            isAuthenticated: true,
            isLoading: false,
            error: undefined,
          });
        } catch (error: any) {
          const errorMessage = error.response?.data?.message || 'Login failed';
          set({
            isLoading: false,
            error: errorMessage,
          });
          throw error;
        }
      },

      // Register action
      register: async (data: RegisterRequest) => {
        set({ isLoading: true, error: undefined });
        
        try {
          const response = await apiHelpers.post<{
            user: User;
            accessToken: string;
            refreshToken: string;
          }>(endpoints.auth.register, data);

          const { user, accessToken, refreshToken } = response;
          
          // Store tokens
          TokenManager.setTokens(accessToken, refreshToken);
          
          set({
            user,
            isAuthenticated: true,
            isLoading: false,
            error: undefined,
          });
        } catch (error: any) {
          const errorMessage = error.response?.data?.message || 'Registration failed';
          set({
            isLoading: false,
            error: errorMessage,
          });
          throw error;
        }
      },

      // Logout action
      logout: () => {
        try {
          // Call logout endpoint to invalidate refresh token
          apiHelpers.post(endpoints.auth.logout);
        } catch (error) {
          // Continue with logout even if API call fails
          console.warn('Logout API call failed:', error);
        }
        
        // Clear tokens and state
        TokenManager.removeTokens();
        set({
          user: null,
          isAuthenticated: false,
          isLoading: false,
          error: undefined,
        });
      },

      // Refresh user data
      refreshUser: async () => {
        if (!get().isAuthenticated) return;
        
        set({ isLoading: true });
        
        try {
          const user = await apiHelpers.get<User>(endpoints.auth.profile);
          set({
            user,
            isLoading: false,
            error: undefined,
          });
        } catch (error: any) {
          // If refresh fails, user might need to re-authenticate
          if (error.response?.status === 401) {
            get().logout();
          } else {
            set({
              isLoading: false,
              error: 'Failed to refresh user data',
            });
          }
        }
      },

      // Clear error
      clearError: () => {
        set({ error: undefined });
      },

      // Check authentication status on app startup
      checkAuth: async () => {
        const token = TokenManager.getAccessToken();
        
        if (!token) {
          set({
            user: null,
            isAuthenticated: false,
            isLoading: false,
          });
          return;
        }

        set({ isLoading: true });
        
        try {
          const user = await apiHelpers.get<User>(endpoints.auth.profile);
          set({
            user,
            isAuthenticated: true,
            isLoading: false,
            error: undefined,
          });
        } catch (error) {
          // Token is invalid, clear everything
          TokenManager.removeTokens();
          set({
            user: null,
            isAuthenticated: false,
            isLoading: false,
            error: undefined,
          });
        }
      },
    }),
    {
      name: 'auth-store',
      // Only persist user data, not loading states
      partialize: (state) => ({
        user: state.user,
        isAuthenticated: state.isAuthenticated,
      }),
    }
  )
);
