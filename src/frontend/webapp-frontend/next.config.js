/** @type {import('next').NextConfig} */
const nextConfig = {
  // Enable standalone output for Docker
  output: 'standalone',
  
  // Environment variables
  env: {
    NEXT_PUBLIC_API_URL: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:7009',
  },

  // External packages for server components
  serverExternalPackages: [],

  // Images configuration
  images: {
    remotePatterns: [
      {
        protocol: 'http',
        hostname: 'localhost',
        port: '9000',
        pathname: '/webapp-bucket/**',
      },
      {
        protocol: 'https',
        hostname: '*.amazonaws.com',
        pathname: '/**',
      },
    ],
  },

  // Security headers
  async headers() {
    return [
      {
        source: '/(.*)',
        headers: [
          {
            key: 'X-Frame-Options',
            value: 'DENY',
          },
          {
            key: 'X-Content-Type-Options',
            value: 'nosniff',
          },
          {
            key: 'Referrer-Policy',
            value: 'origin-when-cross-origin',
          },
        ],
      },
    ];
  },

  // Rewrites for API proxy in development
  async rewrites() {
    return [
      {
        source: '/api/:path*',
        destination: `${process.env.NEXT_PUBLIC_API_URL || 'http://localhost:7009'}/:path*`,
      },
    ];
  },

  // Webpack configuration
  webpack: (config, { dev, isServer }) => {
    // Optimization for production builds
    if (!dev && !isServer) {
      config.resolve.alias = {
        ...config.resolve.alias,
        '@mui/styled-engine': '@mui/styled-engine-sc',
      };
    }

    return config;
  },

  // TypeScript configuration
  typescript: {
    // Ignore TypeScript errors during build for development
    ignoreBuildErrors: true,
  },

  // ESLint configuration
  eslint: {
    // Ignore ESLint during builds for now
    ignoreDuringBuilds: true,
  },

  // Performance optimizations
  compiler: {
    // Remove console logs in production
    removeConsole: process.env.NODE_ENV === 'production',
  },
};

module.exports = nextConfig;
