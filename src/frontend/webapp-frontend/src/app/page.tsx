'use client';

import React, { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useAuthStore } from '@/store/authStore';
import { Navigation } from '@/components/layout/Navigation';
import { Button } from '@/components/ui/Button';
import { Plus, TrendingUp, Users, MessageCircle } from 'lucide-react';

export default function Home() {
  const router = useRouter();
  const { isAuthenticated, isLoading, checkAuth, user } = useAuthStore();

  useEffect(() => {
    checkAuth();
  }, [checkAuth]);

  // Redirect to login if not authenticated
  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.push('/auth/login');
    }
  }, [isAuthenticated, isLoading, router]);

  // Show loading while checking authentication
  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  // Don't render anything if not authenticated (will redirect)
  if (!isAuthenticated) {
    return null;
  }

  return (
    <>
      <Navigation />
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="grid grid-cols-1 lg:grid-cols-4 gap-8">
          {/* Left Sidebar */}
          <div className="lg:col-span-1">
            <div className="bg-white rounded-lg shadow-sm p-6 mb-6">
              <div className="flex items-center space-x-3 mb-4">
                <div className="w-12 h-12 bg-gray-300 rounded-full flex items-center justify-center">
                  {user?.avatarUrl ? (
                    <img
                      src={user.avatarUrl}
                      alt={user.displayName}
                      className="w-12 h-12 rounded-full"
                    />
                  ) : (
                    <span className="text-lg font-semibold text-gray-600">
                      {user?.displayName?.[0]?.toUpperCase()}
                    </span>
                  )}
                </div>
                <div>
                  <h3 className="font-semibold text-gray-900">{user?.displayName}</h3>
                  <p className="text-sm text-gray-500">@{user?.username}</p>
                </div>
              </div>
              <div className="grid grid-cols-3 gap-4 text-center">
                <div>
                  <div className="text-lg font-semibold text-gray-900">{user?.postsCount || 0}</div>
                  <div className="text-xs text-gray-500">Posts</div>
                </div>
                <div>
                  <div className="text-lg font-semibold text-gray-900">{user?.followersCount || 0}</div>
                  <div className="text-xs text-gray-500">Followers</div>
                </div>
                <div>
                  <div className="text-lg font-semibold text-gray-900">{user?.followingCount || 0}</div>
                  <div className="text-xs text-gray-500">Following</div>
                </div>
              </div>
            </div>

            {/* Quick Actions */}
            <div className="bg-white rounded-lg shadow-sm p-6">
              <h3 className="font-semibold text-gray-900 mb-4">Quick Actions</h3>
              <div className="space-y-3">
                <Button variant="outline" size="sm" className="w-full justify-start">
                  <Plus className="w-4 h-4 mr-2" />
                  Create Post
                </Button>
                <Button variant="outline" size="sm" className="w-full justify-start">
                  <Users className="w-4 h-4 mr-2" />
                  Find Friends
                </Button>
                <Button variant="outline" size="sm" className="w-full justify-start">
                  <TrendingUp className="w-4 h-4 mr-2" />
                  Trending
                </Button>
              </div>
            </div>
          </div>

          {/* Main Content */}
          <div className="lg:col-span-2">
            {/* Create Post */}
            <div className="bg-white rounded-lg shadow-sm p-6 mb-6">
              <div className="flex items-start space-x-3">
                <div className="w-10 h-10 bg-gray-300 rounded-full flex items-center justify-center">
                  {user?.avatarUrl ? (
                    <img
                      src={user.avatarUrl}
                      alt={user.displayName}
                      className="w-10 h-10 rounded-full"
                    />
                  ) : (
                    <span className="text-sm font-semibold text-gray-600">
                      {user?.displayName?.[0]?.toUpperCase()}
                    </span>
                  )}
                </div>
                <div className="flex-1">
                  <textarea
                    className="w-full border-0 resize-none focus:ring-0 placeholder-gray-400"
                    placeholder="What's on your mind?"
                    rows={3}
                  />
                  <div className="flex justify-between items-center mt-4">
                    <div className="flex space-x-4 text-gray-400">
                      {/* Media upload icons would go here */}
                    </div>
                    <Button size="sm">
                      Post
                    </Button>
                  </div>
                </div>
              </div>
            </div>

            {/* Feed */}
            <div className="space-y-6">
              {/* Sample posts - these would come from API */}
              {[1, 2, 3].map((i) => (
                <div key={i} className="bg-white rounded-lg shadow-sm p-6">
                  <div className="flex items-start space-x-3 mb-4">
                    <div className="w-10 h-10 bg-gray-300 rounded-full"></div>
                    <div className="flex-1">
                      <div className="flex items-center space-x-2">
                        <h4 className="font-semibold text-gray-900">Sample User {i}</h4>
                        <span className="text-sm text-gray-500">@user{i}</span>
                        <span className="text-sm text-gray-400">·</span>
                        <span className="text-sm text-gray-400">2h</span>
                      </div>
                      <p className="text-gray-800 mt-1">
                        This is a sample post content. In a real application, this would be dynamic content from your backend API.
                      </p>
                    </div>
                  </div>
                  
                  <div className="flex items-center space-x-6 text-gray-500">
                    <button className="flex items-center space-x-2 hover:text-blue-600">
                      <MessageCircle className="w-4 h-4" />
                      <span className="text-sm">12</span>
                    </button>
                    <button className="flex items-center space-x-2 hover:text-red-600">
                      <span className="text-sm">❤️ 24</span>
                    </button>
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Right Sidebar */}
          <div className="lg:col-span-1">
            {/* Trending */}
            <div className="bg-white rounded-lg shadow-sm p-6 mb-6">
              <h3 className="font-semibold text-gray-900 mb-4">Trending</h3>
              <div className="space-y-3">
                {['#WebDevelopment', '#React', '#TypeScript', '#NextJS'].map((tag) => (
                  <div key={tag} className="text-blue-600 hover:underline cursor-pointer">
                    {tag}
                  </div>
                ))}
              </div>
            </div>

            {/* Suggested Users */}
            <div className="bg-white rounded-lg shadow-sm p-6">
              <h3 className="font-semibold text-gray-900 mb-4">People you may know</h3>
              <div className="space-y-4">
                {[1, 2, 3].map((i) => (
                  <div key={i} className="flex items-center justify-between">
                    <div className="flex items-center space-x-3">
                      <div className="w-8 h-8 bg-gray-300 rounded-full"></div>
                      <div>
                        <div className="text-sm font-medium text-gray-900">User {i}</div>
                        <div className="text-xs text-gray-500">@user{i}</div>
                      </div>
                    </div>
                    <Button variant="outline" size="sm">
                      Follow
                    </Button>
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      </main>
    </>
  );
}
