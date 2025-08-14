import { setupServer } from 'msw/node'
import { http, HttpResponse } from 'msw'
import { User } from '@/types'

// Mock data
const mockUser: User = {
  id: 'user-1',
  username: 'testuser',
  email: 'test@example.com',
  firstName: 'Test',
  lastName: 'User',
  displayName: 'Test User',
  bio: 'Test user bio',
  avatarUrl: null,
  isVerified: false,
  createdAt: new Date().toISOString(),
  updatedAt: new Date().toISOString(),
  followersCount: 10,
  followingCount: 5,
  postsCount: 3,
}

const mockTokens = {
  accessToken: 'mock-access-token',
  refreshToken: 'mock-refresh-token',
}

// Define request handlers
export const handlers = [
  // Auth endpoints
  http.post('/auth/login', async ({ request }) => {
    const body = await request.json() as any
    
    if (body.email === 'test@example.com' && body.password === 'password') {
      return HttpResponse.json({
        data: {
          user: mockUser,
          ...mockTokens,
        },
        success: true,
      })
    }
    
    return HttpResponse.json(
      { message: 'Invalid credentials' },
      { status: 401 }
    )
  }),

  http.post('/auth/register', async ({ request }) => {
    const body = await request.json() as any
    
    if (body.email && body.password && body.username) {
      return HttpResponse.json({
        data: {
          user: { ...mockUser, ...body },
          ...mockTokens,
        },
        success: true,
      })
    }
    
    return HttpResponse.json(
      { message: 'Registration failed' },
      { status: 400 }
    )
  }),

  http.post('/auth/refresh', async ({ request }) => {
    const body = await request.json() as any
    
    if (body.refreshToken === 'mock-refresh-token') {
      return HttpResponse.json({
        data: mockTokens,
        success: true,
      })
    }
    
    return HttpResponse.json(
      { message: 'Invalid refresh token' },
      { status: 401 }
    )
  }),

  http.post('/auth/logout', () => {
    return HttpResponse.json({
      data: null,
      success: true,
      message: 'Logged out successfully',
    })
  }),

  http.get('/auth/profile', ({ request }) => {
    const authHeader = request.headers.get('Authorization')
    
    if (authHeader?.includes('mock-access-token')) {
      return HttpResponse.json({
        data: mockUser,
        success: true,
      })
    }
    
    return HttpResponse.json(
      { message: 'Unauthorized' },
      { status: 401 }
    )
  }),

  // User endpoints
  http.get('/users/profile', ({ request }) => {
    const authHeader = request.headers.get('Authorization')
    
    if (authHeader?.includes('mock-access-token')) {
      return HttpResponse.json({
        data: mockUser,
        success: true,
      })
    }
    
    return HttpResponse.json(
      { message: 'Unauthorized' },
      { status: 401 }
    )
  }),

  // Posts endpoints
  http.get('/posts', () => {
    return HttpResponse.json({
      data: [
        {
          id: 'post-1',
          title: 'Test Post',
          content: 'This is a test post',
          author: mockUser,
          createdAt: new Date().toISOString(),
          likesCount: 5,
          commentsCount: 2,
        },
      ],
      success: true,
    })
  }),

  // Comments endpoints
  http.get('/posts/:postId/comments', () => {
    return HttpResponse.json({
      data: [
        {
          id: 'comment-1',
          content: 'Great post!',
          author: mockUser,
          createdAt: new Date().toISOString(),
          likesCount: 1,
        },
      ],
      success: true,
    })
  }),

  // Notifications endpoints
  http.get('/notifications', ({ request }) => {
    const authHeader = request.headers.get('Authorization')
    
    if (authHeader?.includes('mock-access-token')) {
      return HttpResponse.json({
        data: [
          {
            id: 'notification-1',
            type: 'like',
            title: 'New like on your post',
            message: 'Someone liked your post',
            isRead: false,
            createdAt: new Date().toISOString(),
          },
        ],
        success: true,
      })
    }
    
    return HttpResponse.json(
      { message: 'Unauthorized' },
      { status: 401 }
    )
  }),

  http.get('/notifications/count', ({ request }) => {
    const authHeader = request.headers.get('Authorization')
    
    if (authHeader?.includes('mock-access-token')) {
      return HttpResponse.json({
        data: {
          unread: 3,
          total: 10,
        },
        success: true,
      })
    }
    
    return HttpResponse.json(
      { message: 'Unauthorized' },
      { status: 401 }
    )
  }),

  // Media endpoints
  http.post('/media/upload', () => {
    return HttpResponse.json({
      data: {
        id: 'media-1',
        url: 'https://example.com/image.jpg',
        type: 'image/jpeg',
        size: 12345,
      },
      success: true,
    })
  }),

  // Fallback for unhandled requests
  http.all('*', ({ request }) => {
    console.error(`Unhandled ${request.method} request to ${request.url}`)
    return HttpResponse.json(
      { message: 'Not found' },
      { status: 404 }
    )
  }),
]

// Setup the server
export const server = setupServer(...handlers)
