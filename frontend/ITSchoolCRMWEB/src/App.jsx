import { useEffect, useState, createContext, useContext } from 'react'
import { Routes, Route, Navigate } from 'react-router-dom'
import { api } from './api/client'
import { isAuthenticated, logout } from './auth/auth'
import Sidebar from './components/Sidebar.jsx'
import Topbar from './components/Topbar.jsx'
import StubPage from './components/StubPage.jsx'
import LoginPage from './pages/LoginPage.jsx'
import RegisterPage from './pages/RegisterPage.jsx'
import DashboardPage from './pages/DashboardPage.jsx'
import InteractionsPage from './pages/InteractionsPage.jsx'
import UniversitiesPage from './pages/UniversitiesPage.jsx'
import DirectionsPage from './pages/DirectionsPage.jsx'

const AuthContext = createContext(null)

export function useAuth() {
  return useContext(AuthContext)
}

// Каркас внутренних страниц: боковое меню + верхняя панель + контент.
// title — слово в верхней панели (меняется от раздела к разделу).
function Shell({ title, children }) {
  return (
    <div className="shell">
      <input type="checkbox" id="nav-toggle" className="nav-toggle" />
      <Sidebar />
      {/* Клик по затемнению закрывает боковое меню (снимает чекбокс) */}
      <label className="nav-overlay" htmlFor="nav-toggle" aria-label="Закрыть меню" />
      <div className="shell-main">
        <Topbar title={title} />
        <main className="shell-content">{children}</main>
      </div>
    </div>
  )
}

function ProtectedRoute({ children }) {
  const { user, loading } = useAuth()
  if (loading) return <div className="page-loader">Загрузка…</div>
  if (!user) return <Navigate to="/login" replace />
  return children
}

// Раздел «Система» — только для администраторов
function AdminRoute({ children }) {
  const { user } = useAuth()
  if (!(user?.roles ?? []).includes('admin')) {
    return <Navigate to="/" replace />
  }
  return children
}

export default function App() {
  const [user, setUser] = useState(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    // При старте приложения: если есть токен — проверяем его через /api/Auth/me
    if (!isAuthenticated()) {
      setLoading(false)
      return
    }
    api.get('/Auth/me')
      .then((data) => setUser(data))
      .catch(() => logout())
      .finally(() => setLoading(false))
  }, [])

  return (
    <AuthContext.Provider value={{ user, setUser, loading }}>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />

        <Route
          path="/"
          element={
            <ProtectedRoute>
              <Shell title="Главная">
                <DashboardPage />
              </Shell>
            </ProtectedRoute>
          }
        />

        <Route
          path="/interactions"
          element={
            <ProtectedRoute>
              <Shell title="Взаимодействия">
                <InteractionsPage />
              </Shell>
            </ProtectedRoute>
          }
        />

        <Route
          path="/universities"
          element={
            <ProtectedRoute>
              <Shell title="Вузы">
                <UniversitiesPage />
              </Shell>
            </ProtectedRoute>
          }
        />

        <Route
          path="/directions"
          element={
            <ProtectedRoute>
              <Shell title="ИТ-направления">
                <DirectionsPage />
              </Shell>
            </ProtectedRoute>
          }
        />

        <Route
          path="/responsibles"
          element={
            <ProtectedRoute>
              <Shell title="Ответственные">
                <StubPage title="Ответственные" description="Менеджеры (КАМ) и контакты вузов." />
              </Shell>
            </ProtectedRoute>
          }
        />

        <Route
          path="/contracts"
          element={
            <ProtectedRoute>
              <Shell title="Договоры">
                <StubPage title="Договоры" description="Договоры и лицензии по вузам." />
              </Shell>
            </ProtectedRoute>
          }
        />

        <Route
          path="/reports"
          element={
            <ProtectedRoute>
              <Shell title="Отчёты">
                <StubPage title="Отчёты" description="Формирование отчётов за период в форматах xls, xlsx, pdf." />
              </Shell>
            </ProtectedRoute>
          }
        />

        <Route
          path="/visualization"
          element={
            <ProtectedRoute>
              <Shell title="Визуализация">
                <StubPage title="Визуализация" description="Диаграммы и графики по данным CRM." />
              </Shell>
            </ProtectedRoute>
          }
        />

        <Route
          path="/docs"
          element={
            <ProtectedRoute>
              <Shell title="Документация">
                <StubPage title="Документация" description="Руководство пользователя и системного администратора." />
              </Shell>
            </ProtectedRoute>
          }
        />

        <Route
          path="/system"
          element={
            <ProtectedRoute>
              <AdminRoute>
                <Shell title="Система">
                  <StubPage title="Система" description="Пользователи и права доступа, интеграции API." />
                </Shell>
              </AdminRoute>
            </ProtectedRoute>
          }
        />

        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </AuthContext.Provider>
  )
}