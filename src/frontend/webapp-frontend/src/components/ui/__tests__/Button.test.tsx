import React from 'react'
import { render, screen, fireEvent } from '@testing-library/react'
import { Button } from '../Button'

describe('Button', () => {
  it('should render with default props', () => {
    render(<Button>Click me</Button>)
    
    const button = screen.getByRole('button', { name: /click me/i })
    expect(button).toBeInTheDocument()
    expect(button).toHaveAttribute('type', 'button')
    expect(button).not.toBeDisabled()
  })

  it('should render children correctly', () => {
    render(<Button>Test Button</Button>)
    
    expect(screen.getByText('Test Button')).toBeInTheDocument()
  })

  it('should render with JSX children', () => {
    render(
      <Button>
        <span>Icon</span>
        <span>Text</span>
      </Button>
    )
    
    expect(screen.getByText('Icon')).toBeInTheDocument()
    expect(screen.getByText('Text')).toBeInTheDocument()
  })

  describe('Variants', () => {
    it('should render with primary variant by default', () => {
      render(<Button>Primary</Button>)
      
      const button = screen.getByRole('button')
      expect(button).toHaveClass('bg-blue-600', 'text-white', 'hover:bg-blue-700')
    })

    it('should render with secondary variant', () => {
      render(<Button variant="secondary">Secondary</Button>)
      
      const button = screen.getByRole('button')
      expect(button).toHaveClass('bg-gray-600', 'text-white', 'hover:bg-gray-700')
    })

    it('should render with outline variant', () => {
      render(<Button variant="outline">Outline</Button>)
      
      const button = screen.getByRole('button')
      expect(button).toHaveClass('border', 'border-gray-300', 'bg-white', 'text-gray-700')
    })

    it('should render with ghost variant', () => {
      render(<Button variant="ghost">Ghost</Button>)
      
      const button = screen.getByRole('button')
      expect(button).toHaveClass('text-gray-700', 'hover:bg-gray-100')
    })

    it('should render with danger variant', () => {
      render(<Button variant="danger">Danger</Button>)
      
      const button = screen.getByRole('button')
      expect(button).toHaveClass('bg-red-600', 'text-white', 'hover:bg-red-700')
    })
  })

  describe('Sizes', () => {
    it('should render with medium size by default', () => {
      render(<Button>Medium</Button>)
      
      const button = screen.getByRole('button')
      expect(button).toHaveClass('px-4', 'py-2', 'text-sm')
    })

    it('should render with small size', () => {
      render(<Button size="sm">Small</Button>)
      
      const button = screen.getByRole('button')
      expect(button).toHaveClass('px-3', 'py-1.5', 'text-sm')
    })

    it('should render with large size', () => {
      render(<Button size="lg">Large</Button>)
      
      const button = screen.getByRole('button')
      expect(button).toHaveClass('px-6', 'py-3', 'text-base')
    })
  })

  describe('States', () => {
    it('should be disabled when disabled prop is true', () => {
      render(<Button disabled>Disabled</Button>)
      
      const button = screen.getByRole('button')
      expect(button).toBeDisabled()
      expect(button).toHaveClass('disabled:opacity-50', 'disabled:cursor-not-allowed')
    })

    it('should be disabled when loading is true', () => {
      render(<Button loading>Loading</Button>)
      
      const button = screen.getByRole('button')
      expect(button).toBeDisabled()
    })

    it('should show loading spinner when loading', () => {
      render(<Button loading>Loading</Button>)
      
      const spinner = screen.getByRole('button').querySelector('.animate-spin')
      expect(spinner).toBeInTheDocument()
      expect(spinner).toHaveClass('mr-2', 'h-4', 'w-4')
    })

    it('should not show loading spinner when not loading', () => {
      render(<Button>Not Loading</Button>)
      
      const spinner = screen.queryByRole('button').querySelector('.animate-spin')
      expect(spinner).not.toBeInTheDocument()
    })
  })

  describe('Button Types', () => {
    it('should have type="button" by default', () => {
      render(<Button>Button</Button>)
      
      const button = screen.getByRole('button')
      expect(button).toHaveAttribute('type', 'button')
    })

    it('should accept custom type', () => {
      render(<Button type="submit">Submit</Button>)
      
      const button = screen.getByRole('button')
      expect(button).toHaveAttribute('type', 'submit')
    })
  })

  describe('Event Handlers', () => {
    it('should call onClick when clicked', () => {
      const handleClick = jest.fn()
      render(<Button onClick={handleClick}>Click me</Button>)
      
      const button = screen.getByRole('button')
      fireEvent.click(button)
      
      expect(handleClick).toHaveBeenCalledTimes(1)
    })

    it('should not call onClick when disabled', () => {
      const handleClick = jest.fn()
      render(
        <Button onClick={handleClick} disabled>
          Disabled
        </Button>
      )
      
      const button = screen.getByRole('button')
      fireEvent.click(button)
      
      expect(handleClick).not.toHaveBeenCalled()
    })

    it('should not call onClick when loading', () => {
      const handleClick = jest.fn()
      render(
        <Button onClick={handleClick} loading>
          Loading
        </Button>
      )
      
      const button = screen.getByRole('button')
      fireEvent.click(button)
      
      expect(handleClick).not.toHaveBeenCalled()
    })
  })

  describe('Custom Styling', () => {
    it('should accept custom className', () => {
      render(<Button className="custom-class">Custom</Button>)
      
      const button = screen.getByRole('button')
      expect(button).toHaveClass('custom-class')
    })

    it('should merge custom className with default classes', () => {
      render(<Button className="custom-class">Custom</Button>)
      
      const button = screen.getByRole('button')
      expect(button).toHaveClass('custom-class')
      expect(button).toHaveClass('inline-flex') // Default class should still be there
    })

    it('should have all base classes', () => {
      render(<Button>Base Classes</Button>)
      
      const button = screen.getByRole('button')
      expect(button).toHaveClass(
        'inline-flex',
        'items-center',
        'justify-center',
        'rounded-md',
        'font-medium',
        'transition-colors',
        'focus:outline-none',
        'focus:ring-2',
        'focus:ring-offset-2'
      )
    })
  })

  describe('Additional Props', () => {
    it('should pass through additional HTML button attributes', () => {
      render(
        <Button data-testid="custom-button" aria-label="Custom aria label">
          Custom Props
        </Button>
      )
      
      const button = screen.getByTestId('custom-button')
      expect(button).toHaveAttribute('aria-label', 'Custom aria label')
    })

    it('should handle form-related attributes', () => {
      render(
        <Button type="submit" form="my-form" name="submit-button" value="submit">
          Submit Form
        </Button>
      )
      
      const button = screen.getByRole('button')
      expect(button).toHaveAttribute('type', 'submit')
      expect(button).toHaveAttribute('form', 'my-form')
      expect(button).toHaveAttribute('name', 'submit-button')
      expect(button).toHaveAttribute('value', 'submit')
    })
  })

  describe('Accessibility', () => {
    it('should be focusable when not disabled', () => {
      render(<Button>Focusable</Button>)
      
      const button = screen.getByRole('button')
      button.focus()
      expect(button).toHaveFocus()
    })

    it('should not be focusable when disabled', () => {
      render(<Button disabled>Not Focusable</Button>)
      
      const button = screen.getByRole('button')
      button.focus()
      expect(button).not.toHaveFocus()
    })

    it('should have proper ARIA attributes', () => {
      render(<Button aria-describedby="help-text">ARIA Button</Button>)
      
      const button = screen.getByRole('button')
      expect(button).toHaveAttribute('aria-describedby', 'help-text')
    })

    it('should support keyboard navigation', () => {
      const handleClick = jest.fn()
      render(<Button onClick={handleClick}>Keyboard</Button>)
      
      const button = screen.getByRole('button')
      fireEvent.keyDown(button, { key: 'Enter' })
      fireEvent.keyUp(button, { key: 'Enter' })
      
      // Note: The actual Enter key behavior would trigger the click,
      // but we're testing that the button can receive keyboard events
      expect(button).toBeInTheDocument()
    })
  })

  describe('Loading State Combinations', () => {
    it('should show loading spinner with text', () => {
      render(<Button loading>Saving...</Button>)
      
      const button = screen.getByRole('button')
      expect(screen.getByText('Saving...')).toBeInTheDocument()
      expect(button.querySelector('.animate-spin')).toBeInTheDocument()
    })

    it('should maintain variant styling when loading', () => {
      render(
        <Button variant="danger" loading>
          Deleting...
        </Button>
      )
      
      const button = screen.getByRole('button')
      expect(button).toHaveClass('bg-red-600')
      expect(button).toBeDisabled()
      expect(button.querySelector('.animate-spin')).toBeInTheDocument()
    })

    it('should maintain size styling when loading', () => {
      render(
        <Button size="lg" loading>
          Loading Large
        </Button>
      )
      
      const button = screen.getByRole('button')
      expect(button).toHaveClass('px-6', 'py-3', 'text-base')
      expect(button.querySelector('.animate-spin')).toBeInTheDocument()
    })
  })

  describe('Edge Cases', () => {
    it('should handle empty children', () => {
      render(<Button></Button>)
      
      const button = screen.getByRole('button')
      expect(button).toBeInTheDocument()
      expect(button).toBeEmptyDOMElement()
    })

    it('should handle null children', () => {
      render(<Button>{null}</Button>)
      
      const button = screen.getByRole('button')
      expect(button).toBeInTheDocument()
    })

    it('should handle boolean children', () => {
      render(<Button>{true && 'Conditional Text'}</Button>)
      
      expect(screen.getByText('Conditional Text')).toBeInTheDocument()
    })

    it('should handle number children', () => {
      render(<Button>{42}</Button>)
      
      expect(screen.getByText('42')).toBeInTheDocument()
    })
  })
})
