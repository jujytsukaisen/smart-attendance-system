import { lazy } from 'react';
import { createBrowserRouter, Navigate } from 'react-router-dom';
import MainLayout from '@/layouts/MainLayout';
import AuthLayout from '@/layouts/AuthLayout';
import { ProtectedRoute } from '@/features/auth/components/ProtectedRoute';
import { PublicRoute } from '@/features/auth/components/PublicRoute';

// Lazy loading all pages to implement route-level code splitting
const LoginPage = lazy(() => import('@/features/auth/pages/LoginPage').then(m => ({ default: m.LoginPage })));
const DashboardPage = lazy(() => import('@/features/dashboard').then(m => ({ default: m.DashboardPage })));

// Companies
const CompaniesPage = lazy(() => import('@/features/companies').then(m => ({ default: m.CompaniesPage })));
const CreateCompanyPage = lazy(() => import('@/features/companies').then(m => ({ default: m.CreateCompanyPage })));
const EditCompanyPage = lazy(() => import('@/features/companies').then(m => ({ default: m.EditCompanyPage })));
const CompanyDetailsPage = lazy(() => import('@/features/companies').then(m => ({ default: m.CompanyDetailsPage })));

// Branches
const BranchesPage = lazy(() => import('@/features/branches').then(m => ({ default: m.BranchesPage })));
const CreateBranchPage = lazy(() => import('@/features/branches').then(m => ({ default: m.CreateBranchPage })));
const EditBranchPage = lazy(() => import('@/features/branches').then(m => ({ default: m.EditBranchPage })));
const BranchDetailsPage = lazy(() => import('@/features/branches').then(m => ({ default: m.BranchDetailsPage })));

// Departments
const DepartmentsPage = lazy(() => import('@/features/departments/pages').then(m => ({ default: m.DepartmentsPage })));
const CreateDepartmentPage = lazy(() => import('@/features/departments/pages').then(m => ({ default: m.CreateDepartmentPage })));
const EditDepartmentPage = lazy(() => import('@/features/departments/pages').then(m => ({ default: m.EditDepartmentPage })));
const DepartmentDetailsPage = lazy(() => import('@/features/departments/pages').then(m => ({ default: m.DepartmentDetailsPage })));

// Employees
const EmployeesPage = lazy(() => import('@/features/employees/pages').then(m => ({ default: m.EmployeesPage })));
const CreateEmployeePage = lazy(() => import('@/features/employees/pages').then(m => ({ default: m.CreateEmployeePage })));
const EditEmployeePage = lazy(() => import('@/features/employees/pages').then(m => ({ default: m.EditEmployeePage })));
const EmployeeDetailsPage = lazy(() => import('@/features/employees/pages').then(m => ({ default: m.EmployeeDetailsPage })));

// Attendance Policies
const AttendancePoliciesPage = lazy(() => import('@/features/attendance-policies/pages').then(m => ({ default: m.AttendancePoliciesPage })));
const CreateAttendancePolicyPage = lazy(() => import('@/features/attendance-policies/pages').then(m => ({ default: m.CreateAttendancePolicyPage })));
const EditAttendancePolicyPage = lazy(() => import('@/features/attendance-policies/pages').then(m => ({ default: m.EditAttendancePolicyPage })));
const AttendancePolicyDetailsPage = lazy(() => import('@/features/attendance-policies/pages').then(m => ({ default: m.AttendancePolicyDetailsPage })));

// Attendance
const AttendancePage = lazy(() => import('@/features/attendance/pages').then(m => ({ default: m.AttendancePage })));
const ManualAttendancePage = lazy(() => import('@/features/attendance/pages').then(m => ({ default: m.ManualAttendancePage })));
const AttendanceDetailsPage = lazy(() => import('@/features/attendance/pages').then(m => ({ default: m.AttendanceDetailsPage })));

// Leaves
const LeavesPage = lazy(() => import('@/features/leaves/pages').then(m => ({ default: m.LeavesPage })));
const CreateLeavePage = lazy(() => import('@/features/leaves/pages').then(m => ({ default: m.CreateLeavePage })));
const EditLeavePage = lazy(() => import('@/features/leaves/pages').then(m => ({ default: m.EditLeavePage })));
const LeaveDetailsPage = lazy(() => import('@/features/leaves/pages').then(m => ({ default: m.LeaveDetailsPage })));

// Users
const UsersPage = lazy(() => import('@/features/users/pages').then(m => ({ default: m.UsersPage })));
const CreateUserPage = lazy(() => import('@/features/users/pages').then(m => ({ default: m.CreateUserPage })));
const EditUserPage = lazy(() => import('@/features/users/pages').then(m => ({ default: m.EditUserPage })));
const UserDetailsPage = lazy(() => import('@/features/users/pages').then(m => ({ default: m.UserDetailsPage })));

// Roles
const RolesPage = lazy(() => import('@/features/roles/pages').then(m => ({ default: m.RolesPage })));
const CreateRolePage = lazy(() => import('@/features/roles/pages').then(m => ({ default: m.CreateRolePage })));
const EditRolePage = lazy(() => import('@/features/roles/pages').then(m => ({ default: m.EditRolePage })));
const RoleDetailsPage = lazy(() => import('@/features/roles/pages').then(m => ({ default: m.RoleDetailsPage })));

// Devices
const DevicesPage = lazy(() => import('@/features/devices/pages').then(m => ({ default: m.DevicesPage })));
const CreateDevicePage = lazy(() => import('@/features/devices/pages').then(m => ({ default: m.CreateDevicePage })));
const EditDevicePage = lazy(() => import('@/features/devices/pages').then(m => ({ default: m.EditDevicePage })));
const DeviceDetailsPage = lazy(() => import('@/features/devices/pages').then(m => ({ default: m.DeviceDetailsPage })));

// Reports
const ReportsDashboardPage = lazy(() => import('@/features/reports/pages').then(m => ({ default: m.ReportsDashboardPage })));
const AttendanceReportsPage = lazy(() => import('@/features/reports/pages').then(m => ({ default: m.AttendanceReportsPage })));
const EmployeeReportsPage = lazy(() => import('@/features/reports/pages').then(m => ({ default: m.EmployeeReportsPage })));
const LeaveReportsPage = lazy(() => import('@/features/reports/pages').then(m => ({ default: m.LeaveReportsPage })));
const AnalyticsPage = lazy(() => import('@/features/reports/pages').then(m => ({ default: m.AnalyticsPage })));

// Notifications
const NotificationsPage = lazy(() => import('@/features/notifications/pages').then(m => ({ default: m.NotificationsPage })));
const NotificationDetailsPage = lazy(() => import('@/features/notifications/pages').then(m => ({ default: m.NotificationDetailsPage })));

// Audit Logs
const AuditLogsPage = lazy(() => import('@/features/audit-logs/pages').then(m => ({ default: m.AuditLogsPage })));
const AuditLogDetailsPage = lazy(() => import('@/features/audit-logs/pages').then(m => ({ default: m.AuditLogDetailsPage })));

// Settings
const SettingsLayout = lazy(() => import('@/features/settings/pages').then(m => ({ default: m.SettingsLayout })));
const GeneralSettingsPage = lazy(() => import('@/features/settings/pages').then(m => ({ default: m.GeneralSettingsPage })));
const AttendanceSettingsPage = lazy(() => import('@/features/settings/pages').then(m => ({ default: m.AttendanceSettingsPage })));
const SecuritySettingsPage = lazy(() => import('@/features/settings/pages').then(m => ({ default: m.SecuritySettingsPage })));
const NotificationSettingsPage = lazy(() => import('@/features/settings/pages').then(m => ({ default: m.NotificationSettingsPage })));
const DeviceSettingsPage = lazy(() => import('@/features/settings/pages').then(m => ({ default: m.DeviceSettingsPage })));
const EmailSettingsPage = lazy(() => import('@/features/settings/pages').then(m => ({ default: m.EmailSettingsPage })));
const SmsSettingsPage = lazy(() => import('@/features/settings/pages').then(m => ({ default: m.SmsSettingsPage })));
const BackupSettingsPage = lazy(() => import('@/features/settings/pages').then(m => ({ default: m.BackupSettingsPage })));
const FaceRecognitionSettingsPage = lazy(() => import('@/features/settings/pages').then(m => ({ default: m.FaceRecognitionSettingsPage })));

// Pages placeholder
const NotFound = () => <div>404 Not Found</div>;
const Unauthorized = () => <div>403 Unauthorized</div>;

export const router = createBrowserRouter([
  {
    path: '/',
    element: <ProtectedRoute />, // All MainLayout routes are protected
    children: [
      {
        path: '/',
        element: <MainLayout />,
        children: [
          {
            path: '/',
            element: <Navigate to="/dashboard" replace />,
          },
          {
            path: 'dashboard',
            element: <DashboardPage />,
          },
          {
            path: 'companies',
            element: <CompaniesPage />,
          },
          {
            path: 'companies/new',
            element: <CreateCompanyPage />,
          },
          {
            path: 'companies/:id',
            element: <CompanyDetailsPage />,
          },
          {
            path: 'companies/:id/edit',
            element: <EditCompanyPage />,
          },
          {
            path: 'branches',
            element: <BranchesPage />,
          },
          {
            path: 'branches/new',
            element: <CreateBranchPage />,
          },
          {
            path: 'branches/:id',
            element: <BranchDetailsPage />,
          },
          {
            path: 'branches/:id/edit',
            element: <EditBranchPage />,
          },
          {
            path: 'departments',
            element: <DepartmentsPage />,
          },
          {
            path: 'departments/new',
            element: <CreateDepartmentPage />,
          },
          {
            path: 'departments/:id',
            element: <DepartmentDetailsPage />,
          },
          {
            path: 'departments/:id/edit',
            element: <EditDepartmentPage />,
          },
          {
            path: 'employees',
            element: <EmployeesPage />,
          },
          {
            path: 'employees/create',
            element: <CreateEmployeePage />,
          },
          {
            path: 'employees/:id',
            element: <EmployeeDetailsPage />,
          },
          {
            path: 'employees/:id/edit',
            element: <EditEmployeePage />,
          },
          {
            path: 'attendance-policies',
            element: <AttendancePoliciesPage />,
          },
          {
            path: 'attendance-policies/create',
            element: <CreateAttendancePolicyPage />,
          },
          {
            path: 'attendance-policies/:id',
            element: <AttendancePolicyDetailsPage />,
          },
          {
            path: 'attendance-policies/:id/edit',
            element: <EditAttendancePolicyPage />,
          },
          {
            path: 'attendance',
            element: <AttendancePage />,
          },
          {
            path: 'attendance/manual',
            element: <ManualAttendancePage />,
          },
          {
            path: 'attendance/:id',
            element: <AttendanceDetailsPage />,
          },
          {
            path: 'leaves',
            element: <LeavesPage />,
          },
          {
            path: 'leaves/new',
            element: <CreateLeavePage />,
          },
          {
            path: 'leaves/:id',
            element: <LeaveDetailsPage />,
          },
          {
            path: 'leaves/:id/edit',
            element: <EditLeavePage />,
          },
          {
            path: 'users',
            element: <UsersPage />,
          },
          {
            path: 'users/new',
            element: <CreateUserPage />,
          },
          {
            path: 'users/:id',
            element: <UserDetailsPage />,
          },
          {
            path: 'users/:id/edit',
            element: <EditUserPage />,
          },
          {
            path: 'roles',
            element: <RolesPage />,
          },
          {
            path: 'roles/new',
            element: <CreateRolePage />,
          },
          {
            path: 'roles/:id',
            element: <RoleDetailsPage />,
          },
          {
            path: 'roles/:id/edit',
            element: <EditRolePage />,
          },
          {
            path: 'devices',
            element: <DevicesPage />,
          },
          {
            path: 'devices/new',
            element: <CreateDevicePage />,
          },
          {
            path: 'devices/:id',
            element: <DeviceDetailsPage />,
          },
          {
            path: 'devices/:id/edit',
            element: <EditDevicePage />,
          },
          {
            path: 'reports',
            element: <ReportsDashboardPage />,
          },
          {
            path: 'reports/attendance',
            element: <AttendanceReportsPage />,
          },
          {
            path: 'reports/employees',
            element: <EmployeeReportsPage />,
          },
          {
            path: 'reports/leaves',
            element: <LeaveReportsPage />,
          },
          {
            path: 'reports/analytics',
            element: <AnalyticsPage />,
          },
          {
            path: 'notifications',
            element: <NotificationsPage />,
          },
          {
            path: 'notifications/:id',
            element: <NotificationDetailsPage />,
          },
          {
            path: 'audit-logs',
            element: <AuditLogsPage />,
          },
          {
            path: 'audit-logs/:id',
            element: <AuditLogDetailsPage />,
          },
          {
            path: 'settings',
            element: <SettingsLayout />,
            children: [
              {
                path: '',
                element: <Navigate to="general" replace />,
              },
              {
                path: 'general',
                element: <GeneralSettingsPage />,
              },
              {
                path: 'attendance',
                element: <AttendanceSettingsPage />,
              },
              {
                path: 'security',
                element: <SecuritySettingsPage />,
              },
              {
                path: 'notifications',
                element: <NotificationSettingsPage />,
              },
              {
                path: 'devices',
                element: <DeviceSettingsPage />,
              },
              {
                path: 'email',
                element: <EmailSettingsPage />,
              },
              {
                path: 'sms',
                element: <SmsSettingsPage />,
              },
              {
                path: 'backup',
                element: <BackupSettingsPage />,
              },
              {
                path: 'face-recognition',
                element: <FaceRecognitionSettingsPage />,
              },
            ],
          },
        ],
      },
    ]
  },
  {
    path: '/auth',
    element: <PublicRoute />, // Only for unauthenticated users
    children: [
      {
        path: '',
        element: <AuthLayout />,
        children: [
          {
            path: 'login',
            element: <LoginPage />,
          }
        ],
      }
    ]
  },
  {
    path: '/unauthorized',
    element: <Unauthorized />,
  },
  {
    path: '*',
    element: <NotFound />,
  },
]);
