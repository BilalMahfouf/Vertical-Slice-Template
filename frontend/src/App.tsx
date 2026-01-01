import { useState, type FormEvent } from 'react'
import { authApi } from './lib/api/auth'
import { tokenManager } from './lib/api/tokenManager'
import './App.css'

function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(false)
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const [tokenInfo, setTokenInfo] = useState<string | null>(null)

  const handleLogin = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    setLoading(true)

    try {
      const response = await authApi.login({ email, password })
      setIsLoggedIn(true)
      setTokenInfo(response.value.token)
    } catch (err: any) {
      setError(err.response?.data?.message || 'Login failed. Please check your credentials.')
    } finally {
      setLoading(false)
    }
  }

  const handleRefreshToken = async () => {
    setLoading(true)
    setError('')
    try {
      const newToken = await authApi.refresh()
      if (newToken) {
        setTokenInfo(newToken)
        alert('Token refreshed successfully!')
      }
    } catch (err: any) {
      setError('Failed to refresh token')
    } finally {
      setLoading(false)
    }
  }

  const handleLogout = async () => {
    await authApi.logout()
    setIsLoggedIn(false)
    setTokenInfo(null)
    setEmail('')
    setPassword('')
  }

  if (isLoggedIn) {
    return (
      <div className="min-h-screen bg-gray-100 flex items-center justify-center px-4">
        <div className="bg-white rounded-lg shadow-lg p-8 max-w-2xl w-full">
          <div className="text-center mb-8">
            <div className="inline-flex items-center justify-center w-16 h-16 bg-blue-500 rounded-2xl mb-4">
              <svg className="w-10 h-10 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
            <h1 className="text-3xl font-bold text-gray-800 mb-2">Dashboard</h1>
            <p className="text-gray-600">You are successfully logged in!</p>
          </div>

          <div className="space-y-4 mb-6">
            <div className="bg-gray-50 p-4 rounded-lg">
              <h3 className="font-semibold text-gray-700 mb-2">Current Access Token:</h3>
              <div className="bg-white p-3 rounded border border-gray-200 break-all text-xs font-mono text-gray-600">
                {tokenInfo || tokenManager.getAccessToken()}
              </div>
            </div>

            <div className="bg-blue-50 p-4 rounded-lg">
              <h3 className="font-semibold text-blue-900 mb-2">ℹ️ Token Information</h3>
              <ul className="text-sm text-blue-800 space-y-1">
                <li>• Access token is stored in memory</li>
                <li>• Refresh token is stored in httpOnly cookie</li>
                <li>• Token auto-refreshes on 401 errors</li>
              </ul>
            </div>
          </div>

          <div className="flex gap-3">
            <button
              onClick={handleRefreshToken}
              disabled={loading}
              className="flex-1 bg-blue-500 text-white py-3 px-6 rounded-lg font-semibold hover:bg-blue-600 disabled:bg-gray-300 disabled:cursor-not-allowed transition-colors"
            >
              {loading ? 'Refreshing...' : 'Test Refresh Token'}
            </button>
            <button
              onClick={handleLogout}
              className="flex-1 bg-red-500 text-white py-3 px-6 rounded-lg font-semibold hover:bg-red-600 transition-colors"
            >
              Logout
            </button>
          </div>

          {error && (
            <div className="mt-4 bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg">
              {error}
            </div>
          )}
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-gray-100 flex items-center justify-center px-4">
      <div className="w-full max-w-md">
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-16 h-16 bg-blue-500 rounded-2xl mb-4">
            <svg className="w-10 h-10 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
            </svg>
          </div>
          <h1 className="text-4xl font-bold text-gray-800 mb-2">Secure Portal</h1>
          <p className="text-gray-600">Access your account securely.</p>
        </div>

        <div className="bg-white rounded-lg shadow-lg p-8">
          <h2 className="text-2xl font-bold text-gray-800 mb-2">Welcome back</h2>
          <p className="text-gray-600 mb-6">Sign in to access your account.</p>

          <form onSubmit={handleLogin}>
            <div className="mb-4">
              <label htmlFor="email" className="block text-gray-700 font-semibold mb-2">
                Email
              </label>
              <input
                type="email"
                id="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="user@example.com"
                className="w-full px-4 py-3 border border-gray-300 rounded-lg bg-white text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                required
              />
            </div>

            <div className="mb-6">
              <div className="flex justify-between items-center mb-2">
                <label htmlFor="password" className="block text-gray-700 font-semibold">
                  Password
                </label>
                <a href="#" className="text-blue-500 text-sm hover:underline">
                  Forgot password?
                </a>
              </div>
              <input
                type="password"
                id="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="••••••••"
                className="w-full px-4 py-3 border border-gray-300 rounded-lg bg-white text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                required
              />
            </div>

            {error && (
              <div className="mb-4 bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg text-sm">
                {error}
              </div>
            )}

            <button
              type="submit"
              disabled={loading}
              className="w-full bg-blue-500 text-white py-3 px-6 rounded-lg font-semibold hover:bg-blue-600 disabled:bg-gray-300 disabled:cursor-not-allowed transition-colors"
            >
              {loading ? 'Signing in...' : 'Sign In to Platform'}
            </button>
          </form>
        </div>

        <p className="text-center text-gray-500 text-sm mt-6">
          © 2025 All rights reserved.
        </p>
      </div>
    </div>
  )
}

export default App