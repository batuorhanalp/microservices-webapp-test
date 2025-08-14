// User types
export interface User {
  id: string;
  email: string;
  username: string;
  displayName: string;
  bio?: string;
  avatarUrl?: string;
  isVerified: boolean;
  followersCount: number;
  followingCount: number;
  postsCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface UserProfile extends User {
  isFollowing?: boolean;
  isOwnProfile?: boolean;
}

// Authentication types
export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  username: string;
  displayName: string;
  password: string;
}

export interface AuthResponse {
  user: User;
  accessToken: string;
  refreshToken: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  token: string;
  newPassword: string;
}

// Post types
export interface Post {
  id: string;
  content: string;
  authorId: string;
  author: User;
  mediaAttachments?: MediaAttachment[];
  likesCount: number;
  commentsCount: number;
  isLiked: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreatePostRequest {
  content: string;
  mediaIds?: string[];
}

export interface UpdatePostRequest {
  content: string;
}

// Comment types
export interface Comment {
  id: string;
  content: string;
  postId: string;
  authorId: string;
  author: User;
  likesCount: number;
  isLiked: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateCommentRequest {
  content: string;
  postId: string;
}

// Media types
export interface MediaAttachment {
  id: string;
  type: 'image' | 'video';
  url: string;
  thumbnailUrl?: string;
  altText?: string;
  width?: number;
  height?: number;
  size: number;
  createdAt: string;
}

export interface UploadMediaResponse {
  media: MediaAttachment;
}

// Notification types
export enum NotificationType {
  LIKE = 'like',
  COMMENT = 'comment',
  FOLLOW = 'follow',
  MENTION = 'mention',
  SYSTEM = 'system'
}

export enum NotificationStatus {
  UNREAD = 'unread',
  READ = 'read',
  ARCHIVED = 'archived'
}

export interface Notification {
  id: string;
  type: NotificationType;
  status: NotificationStatus;
  title: string;
  message: string;
  actionUrl?: string;
  relatedUserId?: string;
  relatedUser?: User;
  relatedPostId?: string;
  relatedPost?: Post;
  metadata?: Record<string, any>;
  createdAt: string;
  readAt?: string;
  archivedAt?: string;
}

export interface NotificationCount {
  total: number;
  unread: number;
}

// API Response types
export interface PaginatedResponse<T> {
  data: T[];
  pagination: {
    page: number;
    limit: number;
    total: number;
    pages: number;
    hasNext: boolean;
    hasPrev: boolean;
  };
}

export interface ApiError {
  message: string;
  code?: string;
  status?: number;
  details?: any;
}

// Form types
export interface LoginFormData {
  email: string;
  password: string;
}

export interface RegisterFormData {
  email: string;
  username: string;
  displayName: string;
  password: string;
  confirmPassword: string;
}

export interface ForgotPasswordFormData {
  email: string;
}

export interface ResetPasswordFormData {
  password: string;
  confirmPassword: string;
}

export interface ProfileUpdateFormData {
  displayName: string;
  bio: string;
  username: string;
}

export interface PostFormData {
  content: string;
  media?: File[];
}

export interface CommentFormData {
  content: string;
}

// UI State types
export interface LoadingState {
  isLoading: boolean;
  error?: string;
}

export interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error?: string;
}

export interface PostsState {
  posts: Post[];
  isLoading: boolean;
  error?: string;
  hasMore: boolean;
  page: number;
}

export interface NotificationsState {
  notifications: Notification[];
  count: NotificationCount;
  isLoading: boolean;
  error?: string;
  hasMore: boolean;
  page: number;
}

// Theme and UI types
export type Theme = 'light' | 'dark' | 'system';

export interface UIPreferences {
  theme: Theme;
  language: string;
  notifications: {
    email: boolean;
    push: boolean;
    likes: boolean;
    comments: boolean;
    follows: boolean;
    mentions: boolean;
  };
}

// Filter and sort types
export interface PostFilters {
  authorId?: string;
  search?: string;
  sortBy?: 'newest' | 'oldest' | 'popular';
  timeRange?: 'day' | 'week' | 'month' | 'year' | 'all';
}

export interface UserFilters {
  search?: string;
  sortBy?: 'newest' | 'popular' | 'alphabetical';
}

export interface NotificationFilters {
  type?: NotificationType;
  status?: NotificationStatus;
  sortBy?: 'newest' | 'oldest';
}

// Component props types
export interface BaseComponentProps {
  className?: string;
  children?: React.ReactNode;
}

export interface ModalProps extends BaseComponentProps {
  isOpen: boolean;
  onClose: () => void;
  title?: string;
}

export interface ButtonProps extends BaseComponentProps {
  variant?: 'primary' | 'secondary' | 'outline' | 'ghost' | 'danger';
  size?: 'sm' | 'md' | 'lg';
  disabled?: boolean;
  loading?: boolean;
  onClick?: () => void;
  type?: 'button' | 'submit' | 'reset';
}

export interface InputProps {
  label?: string;
  placeholder?: string;
  type?: string;
  value?: string;
  onChange?: (value: string) => void;
  error?: string;
  disabled?: boolean;
  required?: boolean;
  className?: string;
}
