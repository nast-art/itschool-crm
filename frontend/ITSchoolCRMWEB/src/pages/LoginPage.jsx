import { useState } from 'react'
import { useNavigate, Navigate, Link } from 'react-router-dom'
import { login } from '../auth/auth'
import { api } from '../api/client'
import { useAuth } from '../App.jsx'
import AuthShell from '../components/AuthShell.jsx'

// Экран входа по макету. Роль НЕ выбирается: она приходит в JWT из Keycloak.
export default function LoginPage() {
  const navigate = useNavigate()
  const { user, setUser } = useAuth()

  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [error, setError] = useState('')
  const [pending, setPending] = useState(false)

  if (user) return <Navigate to="/" replace />

  async function handleSubmit(e) {
    e.preventDefault()
    setError('')
    setPending(true)
    try {
      await login(username.trim(), password)
      // После получения токена забираем профиль из нашего API
      const me = await api.get('/Auth/me')
      setUser(me)
      navigate('/')
    } catch (err) {
      setError(err.message)
    } finally {
      setPending(false)
    }
  }

  return (
    <AuthShell>
      <form className="auth-card" onSubmit={handleSubmit}>
        <h2>Вход в систему</h2>

        <label className="field">
          <span className="field-label">Логин/Email</span>
          <input
            type="text"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            placeholder="ivanov@edu-rt.ru"
            autoComplete="username"
            required
          />
        </label>

        <label className="field">
          <span className="field-label">Пароль</span>
          <div className="password-wrap">
            <input
              type={showPassword ? 'text' : 'password'}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="••••••••"
              autoComplete="current-password"
              required
            />
            <button
              type="button"
              className="password-toggle"
              onClick={() => setShowPassword((v) => !v)}
              aria-label="Показать пароль"
            >
              {showPassword ? 'Скрыть' : 'Показать'}
            </button>
          </div>
        </label>

        {error && <div className="form-error" role="alert">{error}</div>}

        <button className="btn-primary" type="submit" disabled={pending}>
          {pending ? 'Вход…' : 'Войти'}
        </button>

        <p className="auth-switch">
          Нет аккаунта? <Link to="/register">Зарегистрироваться</Link>
        </p>
      </form>
    </AuthShell>
  )
}