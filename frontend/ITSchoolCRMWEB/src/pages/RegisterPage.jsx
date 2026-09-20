import { useState } from 'react'
import { useNavigate, Navigate, Link } from 'react-router-dom'
import { register, login } from '../auth/auth'
import { api } from '../api/client'
import { useAuth } from '../App.jsx'
import AuthShell from '../components/AuthShell.jsx'

// Экран регистрации — стилистический аналог экрана входа (общий AuthShell).
// ФИО вводится тремя частями (русский формат):
//   Фамилия*  -> users.last_name, Keycloak lastName
//   Имя*      -> users.first_name, Keycloak firstName (совместно с отчеством)
//   Отчество  -> users.middle_name, атрибут Keycloak middleName (необязательно)
// Email -> users.email (unique), логин Keycloak.
// Пароль хранится только в Keycloak (users не содержит хэшей).
// users.keycloak_user_id бэкенд подтягивает из поля "sub" JWT,
// users.is_active = true по умолчанию.
// users.full_name собирается на сервере ("Фамилия Имя Отчество")
// и используется в отчётах.
// Роль НЕ выбирается: автоматически назначается "user" (на бэкенде).
// После успешной регистрации выполняется автоматический вход.
// Фамилия+Имя и Пароль+Подтверждение объединены в строки по два поля —
// чтобы форма не была чрезмерно длинной, при этом оставаясь
// одноколоночной по логике чтения (UX-паттерн коротких связанных пар).
export default function RegisterPage() {
  const navigate = useNavigate()
  const { user, setUser } = useAuth()

  const [lastName, setLastName] = useState('')
  const [firstName, setFirstName] = useState('')
  const [middleName, setMiddleName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [error, setError] = useState('')
  const [pending, setPending] = useState(false)

  if (user) return <Navigate to="/" replace />

  // Допустимые символы в частях ФИО: буквы (рус/лат) и дефис.
  const namePattern = /^[A-Za-zА-Яа-яЁё-]+$/

  // Клиентская валидация до обращения к API.
  // Повторяет правила AuthService.ValidateRegisterDto.
  function validate() {
    if (!lastName.trim()) return 'Введите фамилию.'
    if (lastName.trim().length < 2 || lastName.trim().length > 100 || !namePattern.test(lastName.trim())) {
      return 'Фамилия: 2–100 символов, только буквы и дефис.'
    }

    if (!firstName.trim()) return 'Введите имя.'
    if (firstName.trim().length < 2 || firstName.trim().length > 100 || !namePattern.test(firstName.trim())) {
      return 'Имя: 2–100 символов, только буквы и дефис.'
    }

    if (middleName.trim()) {
      if (middleName.trim().length < 2 || middleName.trim().length > 100 || !namePattern.test(middleName.trim())) {
        return 'Отчество: 2–100 символов, только буквы и дефис.'
      }
    }

    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim())) {
      return 'Введите корректный email.'
    }

    if (password.length < 8) return 'Пароль должен содержать не менее 8 символов.'
    if (password !== confirmPassword) return 'Пароли не совпадают.'
    return null
  }

  async function handleSubmit(e) {
    e.preventDefault()
    setError('')
    const validationError = validate()
    if (validationError) {
      setError(validationError)
      return
    }
    setPending(true)
    try {
      // Регистрация через наш бэкенд (а уже он — через Keycloak Admin API)
      await register({
        lastName: lastName.trim(),
        firstName: firstName.trim(),
        middleName: middleName.trim() || null,
        email: email.trim(),
        password,
      })
      // Автоматический вход после регистрации
      await login(email.trim(), password)
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
         <form className="auth-card auth-card--wide" onSubmit={handleSubmit}>
        <h2>Регистрация</h2>

        <div className="field-row">
          <label className="field">
            <span className="field-label">Фамилия</span>
            <input
              type="text"
              value={lastName}
              onChange={(e) => setLastName(e.target.value)}
              placeholder="Иванов"
              autoComplete="family-name"
              required
            />
          </label>

          <label className="field">
            <span className="field-label">Имя</span>
            <input
              type="text"
              value={firstName}
              onChange={(e) => setFirstName(e.target.value)}
              placeholder="Иван"
              autoComplete="given-name"
              required
            />
          </label>
        </div>

        <label className="field">
          <span className="field-label">Отчество (необязательно)</span>
          <input
            type="text"
            value={middleName}
            onChange={(e) => setMiddleName(e.target.value)}
            placeholder="Иванович"
            autoComplete="additional-name"
          />
        </label>

        <label className="field">
          <span className="field-label">Email (логин)</span>
          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="ivanov@edu-rt.ru"
            autoComplete="email"
            required
          />
          <span className="form-hint">Email используется как логин для входа</span>
        </label>

        <div className="field-row">
          <label className="field">
            <span className="field-label">Пароль</span>
            <div className="password-wrap">
                <input
                type={showPassword ? 'text' : 'password'}
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                autoComplete="new-password"
                 placeholder="Пароль"
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

          <label className="field">
            <span className="field-label">Подтверждение пароля</span>
            <input
              type={showPassword ? 'text' : 'password'}
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              placeholder="Повторите пароль"
              autoComplete="new-password"
              required
            />
          </label>
        </div>
 <span className="form-hint">Пароль — не менее 8 символов</span>
        {error && <div className="form-error" role="alert">{error}</div>}

        <button className="btn-primary" type="submit" disabled={pending}>
          {pending ? 'Регистрация…' : 'Зарегистрироваться'}
        </button>

        <p className="auth-switch">
          Уже есть аккаунт? <Link to="/login">Войти</Link>
        </p>
      </form>
    </AuthShell>
  )
}