import React, { forwardRef } from 'react';
import { cn } from '@/lib/utils';

// Updated interface to be compatible with both custom usage and React Hook Form
interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ label, error, required, className, ...props }, ref) => {
    const inputClasses = cn(
      'block w-full rounded-md border px-3 py-2 text-sm placeholder-gray-400 shadow-sm transition-colors',
      'focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500',
      error
        ? 'border-red-300 bg-red-50 text-red-900 placeholder-red-300 focus:border-red-500 focus:ring-red-500'
        : 'border-gray-300 bg-white text-gray-900',
      props.disabled && 'cursor-not-allowed bg-gray-50 opacity-50',
      className
    );

    return (
      <div className="space-y-1">
        {label && (
          <label className="block text-sm font-medium text-gray-700">
            {label}
            {required && <span className="text-red-500 ml-1">*</span>}
          </label>
        )}
        <input
          ref={ref}
          required={required}
          className={inputClasses}
          {...props}
        />
        {error && (
          <p className="text-sm text-red-600">{error}</p>
        )}
      </div>
    );
  }
);

Input.displayName = 'Input';
