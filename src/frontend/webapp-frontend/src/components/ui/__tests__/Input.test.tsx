import React from 'react'
import { render, screen, fireEvent } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Input } from '../Input'

describe('Input', () => {
  it('should render with default props', () => {
    render(<Input />)
    
    const input = screen.getByRole('textbox')
    expect(input).toBeInTheDocument()
    expect(input).toHaveClass('block', 'w-full', 'rounded-md', 'border')
    expect(input).not.toBeDisabled()
  })

  it('should render with placeholder', () => {
    render(<Input placeholder="Enter text here" />)
    
    const input = screen.getByPlaceholderText('Enter text here')
    expect(input).toBeInTheDocument()
  })

  it('should render with initial value', () => {
    render(<Input value="Initial value" onChange={() => {}} />)
    
    const input = screen.getByDisplayValue('Initial value')
    expect(input).toBeInTheDocument()
  })

  it('should render with default value', () => {
    render(<Input defaultValue="Default value" />)
    
    const input = screen.getByDisplayValue('Default value')
    expect(input).toBeInTheDocument()
  })

  describe('Label', () => {
    it('should render with label', () => {
      render(<Input label="Username" />)
      
      expect(screen.getByText('Username')).toBeInTheDocument()
      expect(screen.getByLabelText('Username')).toBeInTheDocument()
    })

    it('should not render label when not provided', () => {
      render(<Input />)
      
      const label = screen.queryByRole('label')
      expect(label).not.toBeInTheDocument()
    })

    it('should show required asterisk when required', () => {
      render(<Input label="Email" required />)
      
      expect(screen.getByText('Email')).toBeInTheDocument()
      expect(screen.getByText('*')).toBeInTheDocument()
      expect(screen.getByText('*')).toHaveClass('text-red-500')
    })

    it('should not show required asterisk when not required', () => {
      render(<Input label="Optional Field" />)
      
      expect(screen.getByText('Optional Field')).toBeInTheDocument()
      expect(screen.queryByText('*')).not.toBeInTheDocument()
    })

    it('should associate label with input', () => {
      render(<Input label="Email Address" />)
      
      const input = screen.getByRole('textbox')
      const label = screen.getByText('Email Address')
      
      expect(input).toHaveAccessibleName('Email Address')
      expect(label.tagName).toBe('LABEL')
    })
  })

  describe('Error States', () => {
    it('should render with error message', () => {
      render(<Input error="This field is required" />)
      
      const errorMessage = screen.getByText('This field is required')
      expect(errorMessage).toBeInTheDocument()
      expect(errorMessage).toHaveClass('text-sm', 'text-red-600')
    })

    it('should apply error styling to input', () => {
      render(<Input error="Invalid input" />)
      
      const input = screen.getByRole('textbox')
      expect(input).toHaveClass(
        'border-red-300',
        'bg-red-50',
        'text-red-900',
        'focus:border-red-500',
        'focus:ring-red-500'
      )
    })

    it('should not render error message when no error', () => {
      render(<Input />)
      
      const error = screen.queryByRole('alert')
      expect(error).not.toBeInTheDocument()
    })

    it('should combine label and error', () => {
      render(<Input label="Password" error="Password too short" />)
      
      expect(screen.getByText('Password')).toBeInTheDocument()
      expect(screen.getByText('Password too short')).toBeInTheDocument()
    })
  })

  describe('Input Types', () => {
    it('should render as text input by default', () => {
      render(<Input />)
      
      const input = screen.getByRole('textbox')
      expect(input).toHaveAttribute('type', 'text')
    })

    it('should render as password input', () => {
      render(<Input type="password" />)
      
      const input = screen.getByRole('textbox')
      expect(input).toHaveAttribute('type', 'password')
    })

    it('should render as email input', () => {
      render(<Input type="email" />)
      
      const input = screen.getByRole('textbox')
      expect(input).toHaveAttribute('type', 'email')
    })

    it('should render as number input', () => {
      render(<Input type="number" />)
      
      const input = screen.getByRole('spinbutton')
      expect(input).toHaveAttribute('type', 'number')
    })

    it('should render as search input', () => {
      render(<Input type="search" />)
      
      const input = screen.getByRole('searchbox')
      expect(input).toHaveAttribute('type', 'search')
    })
  })

  describe('Disabled State', () => {
    it('should be disabled when disabled prop is true', () => {
      render(<Input disabled />)
      
      const input = screen.getByRole('textbox')
      expect(input).toBeDisabled()
      expect(input).toHaveClass('cursor-not-allowed', 'bg-gray-50', 'opacity-50')
    })

    it('should not be disabled by default', () => {
      render(<Input />)
      
      const input = screen.getByRole('textbox')
      expect(input).not.toBeDisabled()
    })

    it('should not accept input when disabled', async () => {
      const user = userEvent.setup()
      render(<Input disabled />)
      
      const input = screen.getByRole('textbox')
      await user.type(input, 'test')
      
      expect(input).toHaveValue('')
    })
  })

  describe('Event Handlers', () => {
    it('should call onChange when input value changes', async () => {
      const handleChange = jest.fn()
      const user = userEvent.setup()
      render(<Input onChange={handleChange} />)
      
      const input = screen.getByRole('textbox')
      await user.type(input, 'hello')
      
      expect(handleChange).toHaveBeenCalled()
      expect(input).toHaveValue('hello')
    })

    it('should call onFocus when input is focused', () => {
      const handleFocus = jest.fn()
      render(<Input onFocus={handleFocus} />)
      
      const input = screen.getByRole('textbox')
      fireEvent.focus(input)
      
      expect(handleFocus).toHaveBeenCalledTimes(1)
    })

    it('should call onBlur when input loses focus', () => {
      const handleBlur = jest.fn()
      render(<Input onBlur={handleBlur} />)
      
      const input = screen.getByRole('textbox')
      fireEvent.focus(input)
      fireEvent.blur(input)
      
      expect(handleBlur).toHaveBeenCalledTimes(1)
    })

    it('should call onKeyDown when key is pressed', () => {
      const handleKeyDown = jest.fn()
      render(<Input onKeyDown={handleKeyDown} />)
      
      const input = screen.getByRole('textbox')
      fireEvent.keyDown(input, { key: 'Enter' })
      
      expect(handleKeyDown).toHaveBeenCalledTimes(1)
    })
  })

  describe('Custom Styling', () => {
    it('should accept custom className', () => {
      render(<Input className="custom-input" />)
      
      const input = screen.getByRole('textbox')
      expect(input).toHaveClass('custom-input')
    })

    it('should merge custom className with default classes', () => {
      render(<Input className="custom-class" />)
      
      const input = screen.getByRole('textbox')
      expect(input).toHaveClass('custom-class')
      expect(input).toHaveClass('block', 'w-full') // Default classes should still be there
    })

    it('should have correct default styling', () => {
      render(<Input />)
      
      const input = screen.getByRole('textbox')
      expect(input).toHaveClass(
        'block',
        'w-full',
        'rounded-md',
        'border',
        'px-3',
        'py-2',
        'text-sm',
        'border-gray-300',
        'bg-white',
        'text-gray-900'
      )
    })
  })

  describe('Form Integration', () => {
    it('should have name attribute', () => {
      render(<Input name="username" />)
      
      const input = screen.getByRole('textbox')
      expect(input).toHaveAttribute('name', 'username')
    })

    it('should be required when required prop is true', () => {
      render(<Input required />)
      
      const input = screen.getByRole('textbox')
      expect(input).toBeRequired()
    })

    it('should have correct id', () => {
      render(<Input id="email-input" />)
      
      const input = screen.getByRole('textbox')
      expect(input).toHaveAttribute('id', 'email-input')
    })

    it('should work with form submission', () => {
      const handleSubmit = jest.fn()
      render(
        <form onSubmit={handleSubmit}>
          <Input name="test" defaultValue="test value" />
          <button type="submit">Submit</button>
        </form>
      )
      
      const button = screen.getByRole('button')
      fireEvent.click(button)
      
      expect(handleSubmit).toHaveBeenCalled()
    })
  })

  describe('Ref Forwarding', () => {
    it('should forward ref to input element', () => {
      const ref = React.createRef<HTMLInputElement>()
      render(<Input ref={ref} />)
      
      expect(ref.current).toBeInstanceOf(HTMLInputElement)
      expect(ref.current).toBe(screen.getByRole('textbox'))
    })

    it('should allow ref methods to be called', () => {
      const ref = React.createRef<HTMLInputElement>()
      render(<Input ref={ref} />)
      
      expect(() => {
        ref.current?.focus()
        ref.current?.blur()
        ref.current?.select()
      }).not.toThrow()
    })
  })

  describe('Validation', () => {
    it('should support HTML5 validation attributes', () => {
      render(
        <Input
          pattern="[0-9]+"
          minLength={3}
          maxLength={10}
          min="0"
          max="100"
        />
      )
      
      const input = screen.getByRole('textbox')
      expect(input).toHaveAttribute('pattern', '[0-9]+')
      expect(input).toHaveAttribute('minlength', '3')
      expect(input).toHaveAttribute('maxlength', '10')
      expect(input).toHaveAttribute('min', '0')
      expect(input).toHaveAttribute('max', '100')
    })

    it('should support autocomplete attribute', () => {
      render(<Input autoComplete="email" />)
      
      const input = screen.getByRole('textbox')
      expect(input).toHaveAttribute('autocomplete', 'email')
    })
  })

  describe('Accessibility', () => {
    it('should be focusable', () => {
      render(<Input />)
      
      const input = screen.getByRole('textbox')
      input.focus()
      expect(input).toHaveFocus()
    })

    it('should support aria-describedby with error', () => {
      render(<Input error="Invalid email" aria-describedby="help-text" />)
      
      const input = screen.getByRole('textbox')
      expect(input).toHaveAttribute('aria-describedby', 'help-text')
    })

    it('should support aria-label', () => {
      render(<Input aria-label="Search products" />)
      
      const input = screen.getByLabelText('Search products')
      expect(input).toBeInTheDocument()
    })

    it('should support aria-invalid with error', () => {
      render(<Input error="This field is invalid" aria-invalid="true" />)
      
      const input = screen.getByRole('textbox')
      expect(input).toHaveAttribute('aria-invalid', 'true')
    })
  })

  describe('Complex Scenarios', () => {
    it('should render complete input with all props', () => {
      render(
        <Input
          label="Email Address"
          placeholder="Enter your email"
          type="email"
          required
          error="Invalid email format"
          className="custom-class"
          name="email"
          id="email-input"
        />
      )
      
      // Check label
      expect(screen.getByText('Email Address')).toBeInTheDocument()
      expect(screen.getByText('*')).toBeInTheDocument()
      
      // Check input
      const input = screen.getByRole('textbox')
      expect(input).toHaveAttribute('type', 'email')
      expect(input).toHaveAttribute('placeholder', 'Enter your email')
      expect(input).toBeRequired()
      expect(input).toHaveClass('custom-class')
      expect(input).toHaveAttribute('name', 'email')
      expect(input).toHaveAttribute('id', 'email-input')
      
      // Check error styling
      expect(input).toHaveClass('border-red-300', 'bg-red-50')
      expect(screen.getByText('Invalid email format')).toBeInTheDocument()
    })

    it('should handle dynamic error states', () => {
      const TestComponent = () => {
        const [error, setError] = React.useState('')
        const [value, setValue] = React.useState('')
        
        const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
          const newValue = e.target.value
          setValue(newValue)
          setError(newValue.length < 3 ? 'Too short' : '')
        }
        
        return (
          <Input
            label="Username"
            value={value}
            onChange={handleChange}
            error={error}
          />
        )
      }
      
      render(<TestComponent />)
      
      const input = screen.getByRole('textbox')
      
      // Initially no error
      expect(screen.queryByText('Too short')).not.toBeInTheDocument()
      expect(input).not.toHaveClass('border-red-300')
      
      // Type short text to trigger error
      fireEvent.change(input, { target: { value: 'ab' } })
      expect(screen.getByText('Too short')).toBeInTheDocument()
      expect(input).toHaveClass('border-red-300')
      
      // Type longer text to clear error
      fireEvent.change(input, { target: { value: 'abc' } })
      expect(screen.queryByText('Too short')).not.toBeInTheDocument()
      expect(input).not.toHaveClass('border-red-300')
    })
  })

  describe('Edge Cases', () => {
    it('should handle empty label prop', () => {
      render(<Input label="" />)
      
      const label = screen.queryByRole('label')
      expect(label).not.toBeInTheDocument()
    })

    it('should handle empty error prop', () => {
      render(<Input error="" />)
      
      const error = screen.queryByText('')
      expect(error).not.toBeInTheDocument()
    })

    it('should handle whitespace-only error', () => {
      render(<Input error="   " />)
      
      const input = screen.getByRole('textbox')
      expect(input).toHaveClass('border-red-300') // Error styling should still apply
      expect(screen.getByText('   ')).toBeInTheDocument()
    })

    it('should handle special characters in label', () => {
      render(<Input label="Email & Username (required)" required />)
      
      expect(screen.getByText('Email & Username (required)')).toBeInTheDocument()
      expect(screen.getByText('*')).toBeInTheDocument()
    })
  })
})
